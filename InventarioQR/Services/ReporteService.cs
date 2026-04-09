using ClosedXML.Excel;
using InventarioQR.Data;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _db;
    public ReporteService(ApplicationDbContext db) => _db = db;

    public async Task<ReporteResumenGlobalDto> GetResumenAsync()
    {
        var ahora = DateTime.UtcNow;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
        var hace7 = ahora.AddDays(-7);

        var totalProd = await _db.Productos.CountAsync();
        var movMes = await _db.Movimientos
            .CountAsync(m => m.FechaHora >= inicioMes);

        var sinStock = await _db.Inventarios
            .GroupBy(i => i.ProductoId)
            .Where(g => g.Sum(x => x.CantidadDisponible) == 0)
            .CountAsync();

        var bajoStock = await _db.Inventarios
            .GroupBy(i => i.ProductoId)
            .Where(g => g.Sum(x => x.CantidadDisponible) > 0 &&
                        g.Sum(x => x.CantidadDisponible) < 10)
            .CountAsync();

        var entradas = await _db.Movimientos
            .Where(m => m.FechaHora >= inicioMes && m.Tipo == "Entrada")
            .SumAsync(m => (int?)m.Cantidad) ?? 0;

        var salidas = await _db.Movimientos
            .Where(m => m.FechaHora >= inicioMes && m.Tipo == "Salida")
            .SumAsync(m => (int?)m.Cantidad) ?? 0;

        // Gráfica semana
        var rawSemana = await _db.Movimientos
            .Where(m => m.FechaHora >= hace7)
            .GroupBy(m => new
            {
                Fecha = m.FechaHora.Date,
                m.Tipo
            })
            .Select(g => new
            {
                g.Key.Fecha,
                g.Key.Tipo,
                Total = g.Count()
            })
            .ToListAsync();

        var semana = Enumerable.Range(0, 7)
            .Select(i => ahora.AddDays(-6 + i).Date)
            .Select(d => new GraficaDiaDto
            {
                Dia = d.ToString("ddd dd"),
                Entradas = rawSemana
                    .FirstOrDefault(x => x.Fecha == d && x.Tipo == "Entrada")?.Total ?? 0,
                Salidas = rawSemana
                    .FirstOrDefault(x => x.Fecha == d && x.Tipo == "Salida")?.Total ?? 0,
                Transferencias = rawSemana
                    .FirstOrDefault(x => x.Fecha == d && x.Tipo == "Transferencia")?.Total ?? 0
            }).ToList();

        // Gráfica por tipo
        var porTipo = await _db.Movimientos
            .Where(m => m.FechaHora >= inicioMes)
            .GroupBy(m => m.Tipo)
            .Select(g => new GraficaTipoDto
            {
                Tipo = g.Key,
                Total = g.Count()
            })
            .ToListAsync();

        return new ReporteResumenGlobalDto
        {
            TotalProductos = totalProd,
            TotalMovimientosMes = movMes,
            ProductosSinStock = sinStock,
            ProductosBajoStock = bajoStock,
            TotalEntradas = entradas,
            TotalSalidas = salidas,
            MovimientosSemana = semana,
            MovimientosPorTipo = porTipo
        };
    }

    public async Task<List<ReporteStockBajoDto>> GetStockBajoAsync(int limite = 50)
    {
        var grupos = await _db.Inventarios
            .Include(i => i.Producto)
            .Include(i => i.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .ToListAsync();

        return grupos
            .GroupBy(i => new
            {
                i.ProductoId,
                i.Producto.SKU,
                i.Producto.Nombre,
                i.Producto.ImagenUrl
            })
            .Select(g => new ReporteStockBajoDto
            {
                ProductoId = g.Key.ProductoId,
                SKU = g.Key.SKU,
                Nombre = g.Key.Nombre,
                ImagenUrl = g.Key.ImagenUrl,
                StockTotal = g.Sum(x => x.CantidadDisponible),
                StockReservado = g.Sum(x => x.CantidadReservada),
                Nivel = g.Sum(x => x.CantidadDisponible) == 0
                    ? "sinstock"
                    : g.Sum(x => x.CantidadDisponible) < 5
                        ? "critico"
                        : "bajo",
                Ubicaciones = g
                    .Where(x => x.CantidadDisponible > 0)
                    .Select(x =>
                        x.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre +
                        " › " + x.Posicion.Nombre)
                    .ToList()
            })
            .Where(x => x.StockTotal < 10)
            .OrderBy(x => x.StockTotal)
            .Take(limite)
            .ToList();
    }

    public async Task<IPagedList<ReporteMovimientoDto>> GetMovimientosAsync(
        string? tipo, string? fechaDesde,
        string? fechaHasta, int pagina, int tamano)
    {
        var q = _db.Movimientos
            .Include(m => m.Producto)
            .Include(m => m.PosicionOrigen)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .Include(m => m.PosicionDestino)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(m => m.Tipo == tipo);

        if (DateTime.TryParse(fechaDesde, out var fd))
            q = q.Where(m => m.FechaHora >= fd);

        if (DateTime.TryParse(fechaHasta, out var fh))
            q = q.Where(m => m.FechaHora <= fh.AddDays(1));

        var lista = q.OrderByDescending(m => m.FechaHora)
            .Select(m => new ReporteMovimientoDto
            {
                Id = m.Id,
                Tipo = m.Tipo,
                ProductoNombre = m.Producto.Nombre,
                ProductoSKU = m.Producto.SKU,
                Cantidad = m.Cantidad,
                NombreUsuario = m.NombreUsuario,
                Motivo = m.Motivo,
                FechaHora = m.FechaHora,
                UbicacionOrigen = m.PosicionOrigen == null ? null :
                    m.PosicionOrigen.Nivel.Estanteria.Zona.Bodega.Nombre +
                    " › " + m.PosicionOrigen.Nombre,
                UbicacionDestino = m.PosicionDestino == null ? null :
                    m.PosicionDestino.Nivel.Estanteria.Zona.Bodega.Nombre +
                    " › " + m.PosicionDestino.Nombre
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<List<ReporteRotacionDto>> GetRotacionAsync(int dias = 30)
    {
        var desde = DateTime.UtcNow.AddDays(-dias);

        var movs = await _db.Movimientos
            .Include(m => m.Producto)
            .Where(m => m.FechaHora >= desde)
            .AsNoTracking()
            .ToListAsync();

        var stockActual = await _db.Inventarios
            .GroupBy(i => i.ProductoId)
            .Select(g => new
            {
                ProductoId = g.Key,
                Stock = g.Sum(x => x.CantidadDisponible)
            })
            .AsNoTracking()
            .ToListAsync();

        return movs
            .GroupBy(m => new
            {
                m.ProductoId,
                m.Producto.SKU,
                m.Producto.Nombre
            })
            .Select(g => new ReporteRotacionDto
            {
                ProductoId = g.Key.ProductoId,
                SKU = g.Key.SKU,
                Nombre = g.Key.Nombre,
                TotalEntradas = g.Where(x => x.Tipo == "Entrada")
                    .Sum(x => x.Cantidad),
                TotalSalidas = g.Where(x => x.Tipo == "Salida")
                    .Sum(x => x.Cantidad),
                StockActual = stockActual
                    .FirstOrDefault(s => s.ProductoId == g.Key.ProductoId)
                    ?.Stock ?? 0
            })
            .OrderByDescending(x => x.TotalSalidas)
            .Take(30)
            .ToList();
    }

    // ── Exportar Excel Stock Bajo ──
    public async Task<byte[]> ExportarStockBajoExcelAsync()
    {
        var data = await GetStockBajoAsync(500);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Stock Bajo");

        // Encabezados
        var headers = new[]
        {
            "SKU", "Producto", "Stock Total",
            "Reservado", "Nivel", "Ubicaciones"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Datos
        for (int r = 0; r < data.Count; r++)
        {
            var item = data[r];
            ws.Cell(r + 2, 1).Value = item.SKU;
            ws.Cell(r + 2, 2).Value = item.Nombre;
            ws.Cell(r + 2, 3).Value = item.StockTotal;
            ws.Cell(r + 2, 4).Value = item.StockReservado;
            ws.Cell(r + 2, 5).Value = item.Nivel;
            ws.Cell(r + 2, 6).Value =
                string.Join(", ", item.Ubicaciones);

            // Color por nivel
            var rowColor = item.Nivel switch
            {
                "sinstock" => XLColor.FromHtml("#FEF2F2"),
                "critico" => XLColor.FromHtml("#FFF7ED"),
                _ => XLColor.FromHtml("#FEFCE8")
            };

            ws.Row(r + 2).Style.Fill.BackgroundColor = rowColor;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Exportar Excel Movimientos ──
    public async Task<byte[]> ExportarMovimientosExcelAsync(
        string? tipo, string? fechaDesde, string? fechaHasta)
    {
        var data = await GetMovimientosAsync(
            tipo, fechaDesde, fechaHasta, 1, 5000);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Movimientos");

        var headers = new[]
        {
            "#", "Tipo", "Producto", "SKU",
            "Origen", "Destino", "Cantidad",
            "Usuario", "Motivo", "Fecha"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var m in data)
        {
            ws.Cell(row, 1).Value = m.Id;
            ws.Cell(row, 2).Value = m.Tipo;
            ws.Cell(row, 3).Value = m.ProductoNombre;
            ws.Cell(row, 4).Value = m.ProductoSKU;
            ws.Cell(row, 5).Value = m.UbicacionOrigen ?? "—";
            ws.Cell(row, 6).Value = m.UbicacionDestino ?? "—";
            ws.Cell(row, 7).Value = m.Cantidad;
            ws.Cell(row, 8).Value = m.NombreUsuario;
            ws.Cell(row, 9).Value = m.Motivo ?? "—";
            ws.Cell(row, 10).Value = m.FechaHora.ToString("dd/MM/yyyy HH:mm");
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class QRService : IQRService
{
    private readonly ApplicationDbContext _db;

    public QRService(ApplicationDbContext db) => _db = db;

    public async Task<IPagedList<QRListaDto>> GetPagedAsync(
        int? productoId, int pagina, int tamano)
    {
        var q = _db.CodigosQR
            .Include(x => x.Producto)
            .Include(x => x.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking();

        if (productoId.HasValue)
            q = q.Where(x => x.ProductoId == productoId.Value);

        var lista = q.OrderByDescending(x => x.FechaGeneracion)
            .Select(x => new QRListaDto
            {
                Id = x.Id,
                ProductoNombre = x.Producto.Nombre,
                ProductoSKU = x.Producto.SKU,
                Payload = x.Payload,
                FechaGeneracion = x.FechaGeneracion,
                Ubicacion =
                    x.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    x.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                    x.Posicion.Nivel.Estanteria.Nombre + " › " +
                    x.Posicion.Nombre,
                StockDisponible = _db.Inventarios
                    .Where(i => i.ProductoId == x.ProductoId &&
                                i.PosicionId == x.PosicionId)
                    .Sum(i => (int?)i.CantidadDisponible) ?? 0
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<QRDetalleDto?> GetDetalleAsync(int id)
    {
        var qr = await _db.CodigosQR
            .Include(x => x.Producto)
            .Include(x => x.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (qr == null) return null;

        return new QRDetalleDto
        {
            Id = qr.Id,
            ProductoId = qr.ProductoId,
            ProductoNombre = qr.Producto.Nombre,
            ProductoSKU = qr.Producto.SKU,
            ProductoImagenUrl = qr.Producto.ImagenUrl,
            Payload = qr.Payload,
            FechaGeneracion = qr.FechaGeneracion,
            QRImagenBase64 = await GenerarImagenBase64Async(qr.Payload),
            Ubicacion =
                qr.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                qr.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                qr.Posicion.Nivel.Estanteria.Nombre + " › " +
                qr.Posicion.Nombre
        };
    }

    public async Task<string> GenerarImagenBase64Async(string payload)
    {
        return await Task.Run(() =>
        {
            using var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            using var qr = new PngByteQRCode(data);
            var bytes = qr.GetGraphic(8);
            return Convert.ToBase64String(bytes);
        });
    }

    public async Task<(bool ok, string msg, int qrId)> GenerarAsync(
        int productoId, int posicionId,
        string userId, string userName)
    {
        // Evitar duplicados
        var existe = await _db.CodigosQR
            .AnyAsync(q => q.ProductoId == productoId &&
                           q.PosicionId == posicionId);
        if (existe)
            return (false, "Ya existe un QR para este producto en esta posición.", 0);

        var payload = $"IQR:{productoId}:{posicionId}:{Guid.NewGuid():N}";

        var qr = new CodigoQR
        {
            ProductoId = productoId,
            PosicionId = posicionId,
            Payload = payload,
            FechaGeneracion = DateTime.UtcNow
        };
        _db.CodigosQR.Add(qr);
        await _db.SaveChangesAsync();

        await Auditar("CREATE", "QR",
            $"QR generado para ProductoId:{productoId} PosicionId:{posicionId}",
            userId, userName);

        return (true, "Código QR generado correctamente.", qr.Id);
    }

    public async Task<(bool ok, string msg)> GenerarMasivoAsync(
        int productoId, List<int> posicionIds,
        string userId, string userName)
    {
        int creados = 0;
        foreach (var posId in posicionIds)
        {
            var existe = await _db.CodigosQR
                .AnyAsync(q => q.ProductoId == productoId &&
                               q.PosicionId == posId);
            if (existe) continue;

            var payload = $"IQR:{productoId}:{posId}:{Guid.NewGuid():N}";
            _db.CodigosQR.Add(new CodigoQR
            {
                ProductoId = productoId,
                PosicionId = posId,
                Payload = payload,
                FechaGeneracion = DateTime.UtcNow
            });
            creados++;
        }

        if (creados == 0)
            return (false, "Todos los QR ya existían para las posiciones seleccionadas.");

        await _db.SaveChangesAsync();
        await Auditar("CREATE", "QR",
            $"Generación masiva: {creados} QR para ProductoId:{productoId}",
            userId, userName);

        return (true, $"{creados} código(s) QR generados correctamente.");
    }

    public async Task<(bool ok, string msg)> EliminarAsync(
        int id, string userId, string userName)
    {
        var qr = await _db.CodigosQR.FindAsync(id);
        if (qr == null) return (false, "QR no encontrado.");

        _db.CodigosQR.Remove(qr);
        await _db.SaveChangesAsync();

        await Auditar("DELETE", "QR",
            $"QR eliminado Id:{id}", userId, userName);

        return (true, "QR eliminado.");
    }

    public async Task<QRGenerarViewModel> PrepararFormularioAsync()
    {
        var productos = await _db.Productos
            .Where(p => p.Estado == "Activo")
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoSelectDto
            {
                Id = p.Id,
                Texto = $"{p.SKU} — {p.Nombre}"
            })
            .AsNoTracking()
            .ToListAsync();

        var posiciones = await _db.Posiciones
            .Include(p => p.Nivel)
                .ThenInclude(n => n.Estanteria)
                    .ThenInclude(e => e.Zona)
                        .ThenInclude(z => z.Bodega)
            .OrderBy(p => p.Nivel.Estanteria.Zona.Bodega.Nombre)
            .Select(p => new PosicionSelectDto
            {
                Id = p.Id,
                Texto = p.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                        p.Nivel.Estanteria.Zona.Nombre + " › " +
                        p.Nivel.Estanteria.Nombre + " › " +
                        p.Nombre
            })
            .AsNoTracking()
            .ToListAsync();

        return new QRGenerarViewModel
        {
            Productos = productos,
            Posiciones = posiciones
        };
    }

    // ── Portal de escaneo ──
    public async Task<EscaneoResultadoDto> EscanearAsync(string payload)
    {
        var qr = await _db.CodigosQR
            .Include(x => x.Producto)
            .Include(x => x.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Payload == payload);

        if (qr == null)
            return new EscaneoResultadoDto
            {
                Encontrado = false,
                Error = "Código QR no reconocido o inválido."
            };

        var stockUbicacion = await _db.Inventarios
            .Where(i => i.ProductoId == qr.ProductoId &&
                        i.PosicionId == qr.PosicionId)
            .Select(i => new
            {
                i.CantidadDisponible,
                i.CantidadReservada
            })
            .FirstOrDefaultAsync();

        var stockGlobal = await _db.Inventarios
            .Where(i => i.ProductoId == qr.ProductoId)
            .SumAsync(i => (int?)i.CantidadDisponible) ?? 0;

        var stockReservadoGlobal = await _db.Inventarios
            .Where(i => i.ProductoId == qr.ProductoId)
            .SumAsync(i => (int?)i.CantidadReservada) ?? 0;

        var otrasUbicaciones = await _db.Inventarios
            .Include(i => i.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .Where(i => i.ProductoId == qr.ProductoId &&
                        i.PosicionId != qr.PosicionId &&
                        i.CantidadDisponible > 0)
            .Select(i => new OtraUbicacionDto
            {
                Ubicacion =
                    i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    i.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                    i.Posicion.Nivel.Estanteria.Nombre + " › " +
                    i.Posicion.Nombre,
                Disponible = i.CantidadDisponible
            })
            .AsNoTracking()
            .ToListAsync();

        return new EscaneoResultadoDto
        {
            Encontrado = true,
            ProductoId = qr.ProductoId,
            ProductoNombre = qr.Producto.Nombre,
            ProductoSKU = qr.Producto.SKU,
            ProductoImagenUrl = qr.Producto.ImagenUrl,
            Descripcion = qr.Producto.Descripcion,
            PrecioBase = qr.Producto.PrecioBase,
            EstadoProducto = qr.Producto.Estado,
            Ubicacion =
                qr.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                qr.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                qr.Posicion.Nivel.Estanteria.Nombre + " › " +
                qr.Posicion.Nombre,
            StockEnUbicacion = stockUbicacion?.CantidadDisponible ?? 0,
            StockGlobal = stockGlobal,
            StockReservado = stockReservadoGlobal,
            OtrasUbicaciones = otrasUbicaciones
        };
    }

    private async Task Auditar(
        string accion, string modulo, string detalle,
        string userId, string userName)
    {
        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            UsuarioId = userId,
            NombreUsuario = userName,
            Accion = accion,
            Modulo = modulo,
            Detalle = detalle,
            FechaHora = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
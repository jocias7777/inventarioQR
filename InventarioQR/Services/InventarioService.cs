using InventarioQR.Data;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class InventarioService : IInventarioService
{
    private readonly ApplicationDbContext _db;

    public InventarioService(ApplicationDbContext db) => _db = db;

    public async Task<IPagedList<InventarioListaDto>> GetPagedAsync(
        string? busqueda, string? bodega, int pagina, int tamano)
    {
        var q = _db.Inventarios
            .Include(i => i.Producto)
            .Include(i => i.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(i =>
                i.Producto.Nombre.Contains(busqueda) ||
                i.Producto.SKU.Contains(busqueda));

        if (!string.IsNullOrWhiteSpace(bodega))
            q = q.Where(i =>
                i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre == bodega);

        var lista = q.OrderByDescending(i => i.UltimaActualizacion)
            .Select(i => new InventarioListaDto
            {
                Id = i.Id,
                ProductoNombre = i.Producto.Nombre,
                ProductoSKU = i.Producto.SKU,
                ProductoImagenUrl = i.Producto.ImagenUrl,
                BodegaNombre = i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre,
                Ubicacion =
                    i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    i.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                    i.Posicion.Nivel.Estanteria.Nombre + " › " +
                    i.Posicion.Nombre,
                CantidadDisponible = i.CantidadDisponible,
                CantidadReservada = i.CantidadReservada,
                CantidadDañada = i.CantidadDañada,
                CantidadBloqueada = i.CantidadBloqueada,
                CantidadEnTransito = i.CantidadEnTransito,
                UltimaActualizacion = i.UltimaActualizacion
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<InventarioDetalleDto?> GetDetalleAsync(int id)
    {
        var i = await _db.Inventarios
            .Include(x => x.Producto)
            .Include(x => x.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (i == null) return null;

        var movs = await _db.Movimientos
            .Include(m => m.Producto)
            .Where(m => m.ProductoId == i.ProductoId &&
                       (m.PosicionOrigenId == i.PosicionId ||
                        m.PosicionDestinoId == i.PosicionId))
            .OrderByDescending(m => m.FechaHora)
            .Take(5)
            .Select(m => new MovimientoResumenDto
            {
                Id = m.Id,
                Tipo = m.Tipo,
                ProductoNombre = m.Producto.Nombre,
                ProductoSKU = m.Producto.SKU,
                Cantidad = m.Cantidad,
                NombreUsuario = m.NombreUsuario,
                FechaHora = m.FechaHora
            })
            .ToListAsync();

        return new InventarioDetalleDto
        {
            Id = i.Id,
            ProductoId = i.ProductoId,
            ProductoNombre = i.Producto.Nombre,
            ProductoSKU = i.Producto.SKU,
            ProductoImagenUrl = i.Producto.ImagenUrl,
            Ubicacion =
                i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                i.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                i.Posicion.Nivel.Estanteria.Nombre + " › " +
                i.Posicion.Nombre,
            CantidadDisponible = i.CantidadDisponible,
            CantidadReservada = i.CantidadReservada,
            CantidadDañada = i.CantidadDañada,
            CantidadBloqueada = i.CantidadBloqueada,
            CantidadEnTransito = i.CantidadEnTransito,
            UltimaActualizacion = i.UltimaActualizacion,
            UltimosMovimientos = movs
        };
    }

    public async Task<InventarioResumenGlobalDto> GetResumenAsync()
    {
        var stats = await _db.Inventarios
            .GroupBy(i => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Disponible = g.Sum(x => x.CantidadDisponible),
                Reservado = g.Sum(x => x.CantidadReservada),
                Dañado = g.Sum(x => x.CantidadDañada)
            })
            .FirstOrDefaultAsync();

        // Bajo stock: agrupamos por producto y filtramos suma < 10
        var bajoStock = await _db.Inventarios
            .GroupBy(i => i.ProductoId)
            .Where(g => g.Sum(x => x.CantidadDisponible) > 0 &&
                        g.Sum(x => x.CantidadDisponible) < 10)
            .CountAsync();

        var sinStock = await _db.Inventarios
            .GroupBy(i => i.ProductoId)
            .Where(g => g.Sum(x => x.CantidadDisponible) == 0)
            .CountAsync();

        return new InventarioResumenGlobalDto
        {
            TotalRegistros = stats?.Total ?? 0,
            TotalDisponible = stats?.Disponible ?? 0,
            TotalReservado = stats?.Reservado ?? 0,
            TotalDañado = stats?.Dañado ?? 0,
            ProductosBajoStock = bajoStock,
            ProductosSinStock = sinStock
        };
    }

    public async Task<List<string>> GetBodegasAsync() =>
        await _db.Bodegas
            .Where(b => b.Activo)
            .Select(b => b.Nombre)
            .AsNoTracking()
            .ToListAsync();
}
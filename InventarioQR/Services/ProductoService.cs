using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductoService(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IPagedList<ProductoListaDto>> GetPagedAsync(
        string? busqueda, string? estado, int pagina, int tamano)
    {
        var q = _db.Productos
            .Include(p => p.Inventarios)
            .Include(p => p.Variantes)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(p =>
                p.Nombre.Contains(busqueda) ||
                p.SKU.Contains(busqueda) ||
                (p.Descripcion != null && p.Descripcion.Contains(busqueda)));

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado);

        var lista = q.OrderByDescending(p => p.FechaCreacion)
            .Select(p => new ProductoListaDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Nombre = p.Nombre,
                PrecioBase = p.PrecioBase,
                Estado = p.Estado,
                ImagenUrl = p.ImagenUrl,
                StockTotal = p.Inventarios.Sum(i => i.CantidadDisponible),
                TotalVariantes = p.Variantes.Count(),
                FechaCreacion = p.FechaCreacion
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<ProductoDetalleDto?> GetDetalleAsync(int id)
    {
        var p = await _db.Productos
            .Include(x => x.Variantes)
            .Include(x => x.Inventarios)
                .ThenInclude(i => i.Posicion)
                    .ThenInclude(pos => pos.Nivel)
                        .ThenInclude(n => n.Estanteria)
                            .ThenInclude(e => e.Zona)
                                .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new ProductoDetalleDto
        {
            Id = p.Id,
            SKU = p.SKU,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            PrecioBase = p.PrecioBase,
            Estado = p.Estado,
            ImagenUrl = p.ImagenUrl,
            StockTotal = p.Inventarios.Sum(i => i.CantidadDisponible),
            StockReservado = p.Inventarios.Sum(i => i.CantidadReservada),
            Variantes = p.Variantes.Select(v => new VarianteViewModel
            {
                Id = v.Id,
                Color = v.Color,
                Tamaño = v.Tamaño,
                SKUVariante = v.SKUVariante
            }).ToList(),
            Ubicaciones = p.Inventarios
                .Where(i => i.CantidadDisponible > 0)
                .Select(i => new UbicacionStockDto
                {
                    Ubicacion = $"{i.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre} › " +
                                $"{i.Posicion.Nivel.Estanteria.Zona.Nombre} › " +
                                $"{i.Posicion.Nivel.Estanteria.Nombre} › " +
                                $"{i.Posicion.Nombre}",
                    Disponible = i.CantidadDisponible,
                    Reservado = i.CantidadReservada
                }).ToList()
        };
    }

    public async Task<ProductoViewModel?> GetParaEditarAsync(int id)
    {
        var p = await _db.Productos
            .Include(x => x.Variantes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new ProductoViewModel
        {
            Id = p.Id,
            SKU = p.SKU,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            PrecioBase = p.PrecioBase,
            Estado = p.Estado,
            ImagenUrl = p.ImagenUrl,
            Variantes = p.Variantes.Select(v => new VarianteViewModel
            {
                Id = v.Id,
                Color = v.Color,
                Tamaño = v.Tamaño,
                SKUVariante = v.SKUVariante
            }).ToList()
        };
    }

    public async Task<(bool ok, string mensaje)> CrearAsync(
        ProductoViewModel vm, string userId, string userName)
    {
        var skuNormalizado = vm.SKU.Trim().ToUpper();

        var existente = await _db.Productos
            .IgnoreQueryFilters()
            .Include(p => p.Variantes)
            .FirstOrDefaultAsync(p => p.SKU == skuNormalizado);

        if (existente != null && !existente.Eliminado)
            return (false, $"El SKU '{skuNormalizado}' ya existe.");

        if (existente != null && existente.Eliminado)
        {
            existente.Eliminado = false;
            existente.SKU = skuNormalizado;
            existente.Nombre = vm.Nombre.Trim();
            existente.Descripcion = vm.Descripcion?.Trim();
            existente.PrecioBase = vm.PrecioBase;
            existente.Estado = vm.Estado;

            if (vm.ImagenFile != null)
                existente.ImagenUrl = await GuardarImagenAsync(vm.ImagenFile);

            _db.Variantes.RemoveRange(existente.Variantes);
            foreach (var v in vm.Variantes.Where(v => !string.IsNullOrWhiteSpace(v.Color)))
            {
                _db.Variantes.Add(new Variante
                {
                    ProductoId = existente.Id,
                    Color = v.Color,
                    Tamaño = v.Tamaño,
                    SKUVariante = $"{existente.SKU}-{v.Color}-{v.Tamaño}".ToUpper()
                });
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsSkuDuplicado(ex))
            {
                return (false, $"El SKU '{skuNormalizado}' ya existe.");
            }

            await Auditar("RESTORE", "Productos",
                $"Producto reactivado: {existente.SKU} — {existente.Nombre}", userId, userName);

            return (true, "Producto creado correctamente.");
        }

        var producto = new Producto
        {
            SKU = skuNormalizado,
            Nombre = vm.Nombre.Trim(),
            Descripcion = vm.Descripcion?.Trim(),
            PrecioBase = vm.PrecioBase,
            Estado = vm.Estado,
            ImagenUrl = await GuardarImagenAsync(vm.ImagenFile)
        };

        _db.Productos.Add(producto);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (EsSkuDuplicado(ex))
        {
            return (false, $"El SKU '{skuNormalizado}' ya existe.");
        }

        // Variantes
        foreach (var v in vm.Variantes.Where(v => !string.IsNullOrWhiteSpace(v.Color)))
        {
            _db.Variantes.Add(new Variante
            {
                ProductoId = producto.Id,
                Color = v.Color,
                Tamaño = v.Tamaño,
                SKUVariante = $"{producto.SKU}-{v.Color}-{v.Tamaño}".ToUpper()
            });
        }

        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Productos",
            $"Producto creado: {producto.SKU} — {producto.Nombre}", userId, userName);

        return (true, "Producto creado correctamente.");
    }

    public async Task<(bool ok, string mensaje)> EditarAsync(
        ProductoViewModel vm, string userId, string userName)
    {
        var producto = await _db.Productos
            .Include(p => p.Variantes)
            .FirstOrDefaultAsync(p => p.Id == vm.Id);

        if (producto == null) return (false, "Producto no encontrado.");

        if (await _db.Productos.AnyAsync(p => p.SKU == vm.SKU && p.Id != vm.Id))
            return (false, $"El SKU '{vm.SKU}' ya está en uso.");

        producto.SKU = vm.SKU.Trim().ToUpper();
        producto.Nombre = vm.Nombre.Trim();
        producto.Descripcion = vm.Descripcion?.Trim();
        producto.PrecioBase = vm.PrecioBase;
        producto.Estado = vm.Estado;

        if (vm.ImagenFile != null)
            producto.ImagenUrl = await GuardarImagenAsync(vm.ImagenFile);

        // Variantes: reemplazar
        _db.Variantes.RemoveRange(producto.Variantes);
        foreach (var v in vm.Variantes.Where(v => !string.IsNullOrWhiteSpace(v.Color)))
        {
            _db.Variantes.Add(new Variante
            {
                ProductoId = producto.Id,
                Color = v.Color,
                Tamaño = v.Tamaño,
                SKUVariante = $"{producto.SKU}-{v.Color}-{v.Tamaño}".ToUpper()
            });
        }

        await _db.SaveChangesAsync();
        await Auditar("UPDATE", "Productos",
            $"Producto editado: {producto.SKU}", userId, userName);

        return (true, "Producto actualizado.");
    }

    public async Task<(bool ok, string mensaje)> EliminarAsync(
        int id, string userId, string userName)
    {
        var producto = await _db.Productos
            .Include(p => p.Inventarios)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null) return (false, "Producto no encontrado.");

        if (producto.Inventarios.Any(i => i.CantidadDisponible > 0))
            return (false, "No se puede eliminar: el producto tiene inventario activo.");

        producto.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Productos",
            $"Producto eliminado (soft): {producto.SKU}", userId, userName);

        return (true, "Producto eliminado correctamente.");
    }

    public async Task<List<ProductoListaDto>> BuscarAjaxAsync(string q)
    {
        return await _db.Productos
            .Include(p => p.Inventarios)
            .Where(p => p.Nombre.Contains(q) || p.SKU.Contains(q))
            .Take(8)
            .Select(p => new ProductoListaDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Nombre = p.Nombre,
                Estado = p.Estado,
                StockTotal = p.Inventarios.Sum(i => i.CantidadDisponible)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Helpers ──
    private async Task<string?> GuardarImagenAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLower();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext)) return null;

        var folder = Path.Combine(_env.WebRootPath, "img", "productos");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(folder, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/img/productos/{fileName}";
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

    private static bool EsSkuDuplicado(DbUpdateException ex)
        => ex.InnerException is SqlException sqlEx
           && sqlEx.Message.Contains("IX_Productos_SKU", StringComparison.OrdinalIgnoreCase);
}
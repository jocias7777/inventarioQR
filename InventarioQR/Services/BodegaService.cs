using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Services;

public class BodegaService : IBodegaService
{
    private readonly ApplicationDbContext _db;
    public BodegaService(ApplicationDbContext db) => _db = db;

    // ── ÁRBOL COMPLETO ──
    public async Task<List<BodegaArbolDto>> GetArbolAsync()
    {
        var bodegas = await _db.Bodegas
            .Include(b => b.Zonas)
                .ThenInclude(z => z.Estanterias)
                    .ThenInclude(e => e.Niveles)
                        .ThenInclude(n => n.Posiciones)
                            .ThenInclude(p => p.Inventarios)
            .AsNoTracking()
            .ToListAsync();

        return bodegas.Select(b => new BodegaArbolDto
        {
            Id = b.Id,
            Nombre = b.Nombre,
            CodigoInterno = b.CodigoInterno,
            Descripcion = b.Descripcion,
            Ubicacion = b.Ubicacion,
            Activo = b.Activo,
            TotalPosiciones = b.Zonas
                .SelectMany(z => z.Estanterias)
                .SelectMany(e => e.Niveles)
                .SelectMany(n => n.Posiciones).Count(),
            TotalConStock = b.Zonas
                .SelectMany(z => z.Estanterias)
                .SelectMany(e => e.Niveles)
                .SelectMany(n => n.Posiciones)
                .Count(p => p.Inventarios.Any(i => i.CantidadDisponible > 0)),
            Zonas = b.Zonas.Select(z => new ZonaArbolDto
            {
                Id = z.Id,
                Nombre = z.Nombre,
                CodigoInterno = z.CodigoInterno,
                Estanterias = z.Estanterias.Select(e => new EstanteriaArbolDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    CodigoInterno = e.CodigoInterno,
                    Niveles = e.Niveles.Select(n => new NivelArbolDto
                    {
                        Id = n.Id,
                        Nombre = n.Nombre,
                        CodigoInterno = n.CodigoInterno,
                        Posiciones = n.Posiciones.Select(p => new PosicionArbolDto
                        {
                            Id = p.Id,
                            Nombre = p.Nombre,
                            CodigoInterno = p.CodigoInterno,
                            StockDisponible = p.Inventarios.Sum(i => i.CantidadDisponible)
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        }).ToList();
    }

    // ── BODEGA ──
    public async Task<(bool ok, string msg)> CrearBodegaAsync(
        BodegaFormViewModel vm, string uid, string uname)
    {
        if (await _db.Bodegas.AnyAsync(b => b.CodigoInterno == vm.CodigoInterno))
            return (false, "Código interno ya existe.");

        var b = new Bodega
        {
            Nombre = vm.Nombre.Trim(),
            CodigoInterno = string.IsNullOrWhiteSpace(vm.CodigoInterno)
                ? $"BOD-{DateTime.UtcNow:yyyyMMddHHmm}"
                : vm.CodigoInterno.Trim().ToUpper(),
            Descripcion = vm.Descripcion?.Trim(),
            Ubicacion = vm.Ubicacion?.Trim()
        };
        _db.Bodegas.Add(b);
        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Bodega", $"Bodega creada: {b.Nombre}", uid, uname);
        return (true, "Bodega creada correctamente.");
    }

    public async Task<(bool ok, string msg)> EditarBodegaAsync(
        BodegaFormViewModel vm, string uid, string uname)
    {
        var b = await _db.Bodegas.FindAsync(vm.Id);
        if (b == null) return (false, "No encontrada.");
        b.Nombre = vm.Nombre.Trim();
        b.Descripcion = vm.Descripcion?.Trim();
        b.Ubicacion = vm.Ubicacion?.Trim();
        await _db.SaveChangesAsync();
        await Auditar("UPDATE", "Bodega", $"Bodega editada: {b.Nombre}", uid, uname);
        return (true, "Bodega actualizada.");
    }

    public async Task<(bool ok, string msg)> EliminarBodegaAsync(
        int id, string uid, string uname)
    {
        var b = await _db.Bodegas
            .Include(x => x.Zonas)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return (false, "No encontrada.");
        if (b.Zonas.Any()) return (false, "No se puede eliminar: tiene zonas asociadas.");
        b.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Bodega", $"Bodega eliminada: {b.Nombre}", uid, uname);
        return (true, "Bodega eliminada.");
    }

    public async Task<BodegaFormViewModel?> GetBodegaAsync(int id)
    {
        var b = await _db.Bodegas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return null;
        return new BodegaFormViewModel
        {
            Id = b.Id,
            Nombre = b.Nombre,
            CodigoInterno = b.CodigoInterno,
            Descripcion = b.Descripcion,
            Ubicacion = b.Ubicacion
        };
    }

    // ── ZONA ──
    public async Task<(bool ok, string msg)> CrearZonaAsync(
        ZonaFormViewModel vm, string uid, string uname)
    {
        if (await _db.Zonas.AnyAsync(z =>
            z.BodegaId == vm.BodegaId && z.Nombre == vm.Nombre))
            return (false, "Ya existe una zona con ese nombre en esta bodega.");

        _db.Zonas.Add(new Zona
        {
            Nombre = vm.Nombre.Trim(),
            CodigoInterno = string.IsNullOrWhiteSpace(vm.CodigoInterno)
                ? $"ZN-{DateTime.UtcNow:mmss}"
                : vm.CodigoInterno.Trim().ToUpper(),
            BodegaId = vm.BodegaId
        });
        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Bodega", $"Zona creada: {vm.Nombre}", uid, uname);
        return (true, "Zona creada.");
    }

    public async Task<(bool ok, string msg)> EditarZonaAsync(
        ZonaFormViewModel vm, string uid, string uname)
    {
        var z = await _db.Zonas.FindAsync(vm.Id);
        if (z == null) return (false, "No encontrada.");
        z.Nombre = vm.Nombre.Trim();
        await _db.SaveChangesAsync();
        await Auditar("UPDATE", "Bodega", $"Zona editada: {z.Nombre}", uid, uname);
        return (true, "Zona actualizada.");
    }

    public async Task<(bool ok, string msg)> EliminarZonaAsync(
        int id, string uid, string uname)
    {
        var z = await _db.Zonas
            .Include(x => x.Estanterias).FirstOrDefaultAsync(x => x.Id == id);
        if (z == null) return (false, "No encontrada.");
        if (z.Estanterias.Any()) return (false, "Tiene estanterías asociadas.");
        z.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Bodega", $"Zona eliminada: {z.Nombre}", uid, uname);
        return (true, "Zona eliminada.");
    }

    public async Task<ZonaFormViewModel?> GetZonaAsync(int id)
    {
        var z = await _db.Zonas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (z == null) return null;
        return new ZonaFormViewModel
        {
            Id = z.Id,
            Nombre = z.Nombre,
            CodigoInterno = z.CodigoInterno,
            BodegaId = z.BodegaId
        };
    }

    // ── ESTANTERÍA ──
    public async Task<(bool ok, string msg)> CrearEstanteriaAsync(
        EstanteriaFormViewModel vm, string uid, string uname)
    {
        _db.Estanterias.Add(new Estanteria
        {
            Nombre = vm.Nombre.Trim(),
            CodigoInterno = string.IsNullOrWhiteSpace(vm.CodigoInterno)
                ? $"EST-{DateTime.UtcNow:mmss}"
                : vm.CodigoInterno.Trim().ToUpper(),
            ZonaId = vm.ZonaId
        });
        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Bodega", $"Estantería creada: {vm.Nombre}", uid, uname);
        return (true, "Estantería creada.");
    }

    public async Task<(bool ok, string msg)> EliminarEstanteriaAsync(
        int id, string uid, string uname)
    {
        var e = await _db.Estanterias
            .Include(x => x.Niveles).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return (false, "No encontrada.");
        if (e.Niveles.Any()) return (false, "Tiene niveles asociados.");
        e.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Bodega", $"Estantería eliminada: {e.Nombre}", uid, uname);
        return (true, "Estantería eliminada.");
    }

    public async Task<EstanteriaFormViewModel?> GetEstanteriaAsync(int id)
    {
        var e = await _db.Estanterias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return null;
        return new EstanteriaFormViewModel
        {
            Id = e.Id,
            Nombre = e.Nombre,
            CodigoInterno = e.CodigoInterno,
            ZonaId = e.ZonaId
        };
    }

    // ── NIVEL ──
    public async Task<(bool ok, string msg)> CrearNivelAsync(
        NivelFormViewModel vm, string uid, string uname)
    {
        _db.Niveles.Add(new Nivel
        {
            Nombre = vm.Nombre.Trim(),
            CodigoInterno = string.IsNullOrWhiteSpace(vm.CodigoInterno)
                ? $"NV-{DateTime.UtcNow:mmss}"
                : vm.CodigoInterno.Trim().ToUpper(),
            EstanteriaId = vm.EstanteriaId
        });
        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Bodega", $"Nivel creado: {vm.Nombre}", uid, uname);
        return (true, "Nivel creado.");
    }

    public async Task<(bool ok, string msg)> EliminarNivelAsync(
        int id, string uid, string uname)
    {
        var n = await _db.Niveles
            .Include(x => x.Posiciones).FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return (false, "No encontrado.");
        if (n.Posiciones.Any()) return (false, "Tiene posiciones asociadas.");
        n.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Bodega", $"Nivel eliminado: {n.Nombre}", uid, uname);
        return (true, "Nivel eliminado.");
    }

    public async Task<NivelFormViewModel?> GetNivelAsync(int id)
    {
        var n = await _db.Niveles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return null;
        return new NivelFormViewModel
        {
            Id = n.Id,
            Nombre = n.Nombre,
            CodigoInterno = n.CodigoInterno,
            EstanteriaId = n.EstanteriaId
        };
    }

    // ── POSICIÓN ──
    public async Task<(bool ok, string msg)> CrearPosicionAsync(
        PosicionFormViewModel vm, string uid, string uname)
    {
        _db.Posiciones.Add(new Posicion
        {
            Nombre = vm.Nombre.Trim(),
            CodigoInterno = string.IsNullOrWhiteSpace(vm.CodigoInterno)
                ? $"POS-{DateTime.UtcNow:mmssff}"
                : vm.CodigoInterno.Trim().ToUpper(),
            NivelId = vm.NivelId
        });
        await _db.SaveChangesAsync();
        await Auditar("CREATE", "Bodega", $"Posición creada: {vm.Nombre}", uid, uname);
        return (true, "Posición creada.");
    }

    public async Task<(bool ok, string msg)> EliminarPosicionAsync(
        int id, string uid, string uname)
    {
        var p = await _db.Posiciones
            .Include(x => x.Inventarios).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return (false, "No encontrada.");
        if (p.Inventarios.Any(i => i.CantidadDisponible > 0))
            return (false, "Tiene inventario activo. Mueve el stock primero.");
        p.Eliminado = true;
        await _db.SaveChangesAsync();
        await Auditar("DELETE", "Bodega", $"Posición eliminada: {p.Nombre}", uid, uname);
        return (true, "Posición eliminada.");
    }

    public async Task<PosicionFormViewModel?> GetPosicionAsync(int id)
    {
        var p = await _db.Posiciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;
        return new PosicionFormViewModel
        {
            Id = p.Id,
            Nombre = p.Nombre,
            CodigoInterno = p.CodigoInterno,
            NivelId = p.NivelId
        };
    }

    // ── SELECTS AJAX ──
    public async Task<List<(int Id, string Nombre)>> GetBodegasSelectAsync() =>
        (await _db.Bodegas.Where(b => b.Activo)
            .OrderBy(b => b.Nombre)
            .Select(b => new { b.Id, b.Nombre })
            .AsNoTracking().ToListAsync())
            .Select(x => (x.Id, x.Nombre)).ToList();

    public async Task<List<(int Id, string Nombre)>> GetZonasSelectAsync(int bodegaId) =>
        (await _db.Zonas.Where(z => z.BodegaId == bodegaId)
            .OrderBy(z => z.Nombre)
            .Select(z => new { z.Id, z.Nombre })
            .AsNoTracking().ToListAsync())
            .Select(x => (x.Id, x.Nombre)).ToList();

    public async Task<List<(int Id, string Nombre)>> GetEstanteriasSelectAsync(int zonaId) =>
        (await _db.Estanterias.Where(e => e.ZonaId == zonaId)
            .OrderBy(e => e.Nombre)
            .Select(e => new { e.Id, e.Nombre })
            .AsNoTracking().ToListAsync())
            .Select(x => (x.Id, x.Nombre)).ToList();

    public async Task<List<(int Id, string Nombre)>> GetNivelesSelectAsync(int estanteriaId) =>
        (await _db.Niveles.Where(n => n.EstanteriaId == estanteriaId)
            .OrderBy(n => n.Nombre)
            .Select(n => new { n.Id, n.Nombre })
            .AsNoTracking().ToListAsync())
            .Select(x => (x.Id, x.Nombre)).ToList();

    // ── Auditoría ──
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
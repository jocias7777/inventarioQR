using InventarioQR.Data;
using InventarioQR.Helpers;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.Administrador)]
public class AuditoriaController : Controller
{
    private readonly ApplicationDbContext _db;
    public AuditoriaController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        string? usuario, string? accion,
        string? modulo, string? fechaDesde,
        string? fechaHasta, int pagina = 1)
    {
        var q = _db.AuditoriaLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(usuario))
            q = q.Where(a => a.NombreUsuario.Contains(usuario) ||
                              a.UsuarioId.Contains(usuario));

        if (!string.IsNullOrWhiteSpace(accion))
            q = q.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(modulo))
            q = q.Where(a => a.Modulo == modulo);

        if (DateTime.TryParse(fechaDesde, out var fDesde))
            q = q.Where(a => a.FechaHora >= fDesde);

        if (DateTime.TryParse(fechaHasta, out var fHasta))
            q = q.Where(a => a.FechaHora <= fHasta.AddDays(1));

        var lista = q
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AuditoriaListaDto
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId,
                NombreUsuario = a.NombreUsuario,
                Accion = a.Accion,
                Modulo = a.Modulo,
                Detalle = a.Detalle,
                IpOrigen = a.IpOrigen,
                FechaHora = a.FechaHora
            })
            .ToPagedList(pagina, 20);

        // Resumen
        var ahora = DateTime.UtcNow;
        var hoy = ahora.Date;
        var semana = ahora.AddDays(-7);

        var totalHoy = await _db.AuditoriaLogs
            .CountAsync(a => a.FechaHora >= hoy);
        var totalSemana = await _db.AuditoriaLogs
            .CountAsync(a => a.FechaHora >= semana);

        var porAccion = await _db.AuditoriaLogs
            .GroupBy(a => a.Accion)
            .Select(g => new AccionCountDto
            {
                Accion = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var modMasActivo = await _db.AuditoriaLogs
            .GroupBy(a => a.Modulo)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "—";

        var resumen = new AuditoriaResumenDto
        {
            TotalHoy = totalHoy,
            TotalSemana = totalSemana,
            AccionMasFrecuente = porAccion.FirstOrDefault()?.Accion ?? "—",
            ModuloMasActivo = modMasActivo,
            AccionesPorTipo = porAccion
        };

        // Módulos y acciones únicos para filtros
        var modulos = await _db.AuditoriaLogs
            .Select(a => a.Modulo).Distinct()
            .OrderBy(x => x).ToListAsync();
        var acciones = await _db.AuditoriaLogs
            .Select(a => a.Accion).Distinct()
            .OrderBy(x => x).ToListAsync();

        ViewBag.Resumen = resumen;
        ViewBag.Modulos = modulos;
        ViewBag.Acciones = acciones;
        ViewBag.FiltUsuario = usuario;
        ViewBag.FiltAccion = accion;
        ViewBag.FiltModulo = modulo;
        ViewBag.FiltFechaDesde = fechaDesde;
        ViewBag.FiltFechaHasta = fechaHasta;

        return View(lista);
    }

    // AJAX detalle de un log
    public async Task<IActionResult> Detalle(int id)
    {
        var log = await _db.AuditoriaLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (log == null) return NotFound();

        return Json(new
        {
            log.Id,
            log.NombreUsuario,
            log.UsuarioId,
            log.Accion,
            log.Modulo,
            log.Detalle,
            log.IpOrigen,
            FechaHora = log.FechaHora.ToString("dd/MM/yyyy HH:mm:ss")
        });
    }

    // Exportar CSV
    public async Task<IActionResult> ExportarCSV()
    {
        var logs = await _db.AuditoriaLogs
            .OrderByDescending(a => a.FechaHora)
            .AsNoTracking()
            .ToListAsync();

        var csv = "Id,Usuario,Accion,Modulo,Detalle,IP,FechaHora\n";
        csv += string.Join("\n", logs.Select(l =>
            $"{l.Id}," +
            $"\"{l.NombreUsuario}\"," +
            $"{l.Accion}," +
            $"{l.Modulo}," +
            $"\"{l.Detalle?.Replace("\"", "'")}\"," +
            $"{l.IpOrigen}," +
            $"{l.FechaHora:yyyy-MM-dd HH:mm:ss}"));

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv",
            $"auditoria_{DateTime.Now:yyyyMMdd}.csv");
    }
}
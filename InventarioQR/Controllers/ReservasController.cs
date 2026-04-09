using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Controllers;

[Authorize]
public class ReservasController : Controller
{
    private readonly IReservaService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public ReservasController(
        IReservaService svc,
        UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    public async Task<IActionResult> Index(
        string? estado, string? busqueda, int pagina = 1)
    {
        await _svc.ProcesarExpiradas();

        var lista = await _svc.GetPagedAsync(estado, busqueda, pagina, 12);
        var resumen = await _svc.GetResumenAsync();
        var form = await _svc.PrepararFormularioAsync();

        ViewBag.Estado = estado;
        ViewBag.Busqueda = busqueda;
        ViewBag.Resumen = resumen;
        ViewBag.Form = form;

        return View(lista);
    }

    // AJAX panel lateral
    public async Task<IActionResult> Detalle(int id)
    {
        var d = await _svc.GetDetalleAsync(id);
        if (d == null) return NotFound();
        return PartialView("_DetalleReserva", d);
    }

    // POST crear desde modal
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ReservaCrearViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // POST completar
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleHelper.AdminYBodega)]
    public async Task<IActionResult> Completar(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CompletarAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "UpdateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // POST cancelar
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CancelarAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // AJAX: stock disponible para validación en modal
    public async Task<IActionResult> GetStock(int productoId, int posicionId)
    {
        var stock = await _svc.PrepararFormularioAsync();
        var inv = await HttpContext.RequestServices
            .GetRequiredService<Data.ApplicationDbContext>()
            .Inventarios
            .Where(i => i.ProductoId == productoId &&
                        i.PosicionId == posicionId)
            .Select(i => i.CantidadDisponible)
            .FirstOrDefaultAsync();

        return Json(new { disponible = inv });
    }
}
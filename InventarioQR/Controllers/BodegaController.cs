using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.Administrador)]
public class BodegaController : Controller
{
    private readonly IBodegaService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public BodegaController(IBodegaService svc, UserManager<ApplicationUser> um)
    {
        _svc = svc; _um = um;
    }

    public async Task<IActionResult> Index()
    {
        var arbol = await _svc.GetArbolAsync();
        return View(arbol);
    }

    // ── BODEGA ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearBodega(BodegaFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState.Values
                .SelectMany(v => v.Errors).First().ErrorMessage);

        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearBodegaAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarBodega(BodegaFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EditarBodegaAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarBodega(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarBodegaAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetBodega(int id)
    {
        var b = await _svc.GetBodegaAsync(id);
        return b == null ? NotFound() : Json(b);
    }

    // ── ZONA ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearZona(ZonaFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearZonaAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarZona(ZonaFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EditarZonaAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarZona(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarZonaAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetZona(int id)
    {
        var z = await _svc.GetZonaAsync(id);
        return z == null ? NotFound() : Json(z);
    }

    // ── ESTANTERÍA ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEstanteria(EstanteriaFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearEstanteriaAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEstanteria(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarEstanteriaAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // ── NIVEL ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearNivel(NivelFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearNivelAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarNivel(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarNivelAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // ── POSICIÓN ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPosicion(PosicionFormViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearPosicionAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPosicion(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarPosicionAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // ── SELECTS AJAX en cascada ──
    public async Task<IActionResult> GetZonasSelect(int bodegaId)
    {
        var items = await _svc.GetZonasSelectAsync(bodegaId);
        return Json(items.Select(x => new { id = x.Id, nombre = x.Nombre }));
    }

    public async Task<IActionResult> GetEstanteriasSelect(int zonaId)
    {
        var items = await _svc.GetEstanteriasSelectAsync(zonaId);
        return Json(items.Select(x => new { id = x.Id, nombre = x.Nombre }));
    }

    public async Task<IActionResult> GetNivelesSelect(int estanteriaId)
    {
        var items = await _svc.GetNivelesSelectAsync(estanteriaId);
        return Json(items.Select(x => new { id = x.Id, nombre = x.Nombre }));
    }
}
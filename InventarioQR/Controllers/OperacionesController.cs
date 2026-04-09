using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.AdminYBodega)]
public class OperacionesController : Controller
{
    private readonly IOperacionService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public OperacionesController(
        IOperacionService svc,
        UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    public async Task<IActionResult> Index(
        string? tipo, string? estado, int pagina = 1)
    {
        var lista = await _svc.GetPagedAsync(tipo, estado, pagina, 12);
        var resumen = await _svc.GetResumenAsync();
        var form = await _svc.PrepararFormularioAsync();

        ViewBag.Tipo = tipo;
        ViewBag.Estado = estado;
        ViewBag.Resumen = resumen;
        ViewBag.Form = form;

        return View(lista);
    }

    // AJAX panel lateral
    public async Task<IActionResult> Detalle(int id)
    {
        var d = await _svc.GetDetalleAsync(id);
        if (d == null) return NotFound();
        return PartialView("_DetalleOperacion", d);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(OperacionCrearViewModel vm)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearAsync(vm, u!.Id, u.NombreCompleto);
        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Iniciar(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.IniciarAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Completar(int id, string? payloadQR)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CompletarAsync(
            id, payloadQR ?? "", u!.Id, u.NombreCompleto);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CancelarAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "Success" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}
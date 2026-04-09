using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.AdminYBodega)]
public class MovimientosController : Controller
{
    private readonly IMovimientoService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public MovimientosController(
        IMovimientoService svc,
        UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    public async Task<IActionResult> Index(
        string? tipo, string? busqueda, int pagina = 1)
    {
        var lista = await _svc.GetPagedAsync(tipo, busqueda, pagina, 15);

        // Cargamos productos y posiciones para el modal de Registrar
        var form = await _svc.PrepararFormularioAsync();
        ViewBag.Productos = form.Productos;
        ViewBag.Posiciones = form.Posiciones;

        ViewBag.Tipo = tipo;
        ViewBag.Busqueda = busqueda;

        return View(lista);
    }

    public async Task<IActionResult> Registrar()
    {
        var vm = await _svc.PrepararFormularioAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(MovimientoCrearViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var prep = await _svc.PrepararFormularioAsync();
            vm.Productos = prep.Productos;
            vm.Posiciones = prep.Posiciones;
            return View(vm);
        }

        var user = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.RegistrarAsync(vm, user!.Id, user.NombreCompleto);

        if (!ok)
        {
            TempData["Error"] = msg;
            var prep = await _svc.PrepararFormularioAsync();
            vm.Productos = prep.Productos;
            vm.Posiciones = prep.Posiciones;
            return View(vm);
        }

        TempData["CreateSuccess"] = msg;
        return RedirectToAction(nameof(Index));
    }
}
using InventarioQR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize]
public class InventarioController : Controller
{
    private readonly IInventarioService _svc;

    public InventarioController(IInventarioService svc) => _svc = svc;

    public async Task<IActionResult> Index(
        string? busqueda, string? bodega, int pagina = 1)
    {
        var bodegas = await _svc.GetBodegasAsync();
        var resumen = await _svc.GetResumenAsync();
        var lista = await _svc.GetPagedAsync(busqueda, bodega, pagina, 15);

        ViewBag.Busqueda = busqueda;
        ViewBag.Bodega = bodega;
        ViewBag.Bodegas = bodegas;
        ViewBag.Resumen = resumen;

        return View(lista);
    }

    // AJAX panel lateral
    public async Task<IActionResult> Detalle(int id)
    {
        var d = await _svc.GetDetalleAsync(id);
        if (d == null) return NotFound();
        return PartialView("_DetallePanel", d);
    }
}
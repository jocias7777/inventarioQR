using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize]
public class EscaneoController : Controller
{
    private readonly IQRService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public EscaneoController(IQRService svc, UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    // Portal principal
    public IActionResult Index() => View();

    // POST: recibe payload escaneado o manual
    [HttpPost]
    public async Task<IActionResult> Consultar(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            ViewBag.Error = "Payload vacío.";
            return View("Index");
        }

        var resultado = await _svc.EscanearAsync(payload.Trim());
        return View("Resultado", resultado);
    }

    // GET: consulta directa por URL (para link desde QR web)
    public async Task<IActionResult> Ver(string p)
    {
        if (string.IsNullOrWhiteSpace(p))
            return RedirectToAction(nameof(Index));

        var resultado = await _svc.EscanearAsync(p.Trim());
        return View("Resultado", resultado);
    }

    // AJAX: reservar desde portal de escaneo
    [HttpPost]
    public async Task<IActionResult> ReservarRapido(
        int productoId, int posicionId, int cantidad)
    {
        // Delegamos al servicio de reservas
        // (se conectará cuando hagamos el módulo de reservas)
        return Json(new { ok = false, msg = "Módulo de reservas próximamente." });
    }
}
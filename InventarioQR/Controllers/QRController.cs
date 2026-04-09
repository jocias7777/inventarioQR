using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.AdminYBodega)]
public class QRController : Controller
{
    private readonly IQRService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public QRController(IQRService svc, UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    public async Task<IActionResult> Index(int? productoId, int pagina = 1)
    {
        var lista = await _svc.GetPagedAsync(productoId, pagina, 12);
        ViewBag.ProductoId = productoId;
        return View(lista);
    }

    // AJAX: detalle con imagen QR
    public async Task<IActionResult> Detalle(int id)
    {
        var d = await _svc.GetDetalleAsync(id);
        if (d == null) return NotFound();
        return PartialView("_DetalleQR", d);
    }

    // GET: formulario generar
    public async Task<IActionResult> Generar()
    {
        var vm = await _svc.PrepararFormularioAsync();
        return View(vm);
    }

    // POST: generar 1 QR
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(int productoId, int posicionId)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg, qrId) = await _svc.GenerarAsync(
            productoId, posicionId, u!.Id, u.NombreCompleto);

        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // POST: generar masivo
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerarMasivo(
        int productoId, List<int> posicionIds)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.GenerarMasivoAsync(
            productoId, posicionIds, u!.Id, u.NombreCompleto);

        TempData[ok ? "CreateSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // Descarga PNG de un QR
    public async Task<IActionResult> Descargar(int id)
    {
        var qr = await _svc.GetDetalleAsync(id);
        if (qr == null) return NotFound();

        var bytes = Convert.FromBase64String(qr.QRImagenBase64);
        var filename = $"QR-{qr.ProductoSKU}-{id}.png";
        return File(bytes, "image/png", filename);
    }

    // GET: datos para selects en modales
    public async Task<IActionResult> GetSelectData()
    {
        var vm = await _svc.PrepararFormularioAsync();
        return Json(new
        {
            productos = vm.Productos.Select(p => new
            { id = p.Id, texto = p.Texto }),
            posiciones = vm.Posiciones.Select(p => new
            { id = p.Id, texto = p.Texto })
        });
    }

    // Eliminar
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var u = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarAsync(id, u!.Id, u.NombreCompleto);
        TempData[ok ? "DeleteSuccess" : "Error"] = msg;
        return RedirectToAction(nameof(Index));
    }
}
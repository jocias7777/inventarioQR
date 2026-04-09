using InventarioQR.Helpers;
using InventarioQR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly IReporteService _svc;
    public ReportesController(IReporteService svc) => _svc = svc;

    public async Task<IActionResult> Index()
    {
        var resumen = await _svc.GetResumenAsync();
        return View(resumen);
    }

    public async Task<IActionResult> StockBajo()
    {
        var data = await _svc.GetStockBajoAsync();
        return View(data);
    }

    public async Task<IActionResult> Movimientos(
        string? tipo, string? fechaDesde,
        string? fechaHasta, int pagina = 1)
    {
        var data = await _svc.GetMovimientosAsync(
            tipo, fechaDesde, fechaHasta, pagina, 20);

        ViewBag.Tipo = tipo;
        ViewBag.FechaDesde = fechaDesde;
        ViewBag.FechaHasta = fechaHasta;
        return View(data);
    }

    public async Task<IActionResult> Rotacion(int dias = 30)
    {
        var data = await _svc.GetRotacionAsync(dias);
        ViewBag.Dias = dias;
        return View(data);
    }

    // ── Exportaciones Excel ──
    [Authorize(Roles = RoleHelper.Administrador)]
    public async Task<IActionResult> ExportarStockBajo()
    {
        var bytes = await _svc.ExportarStockBajoExcelAsync();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"stock_bajo_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [Authorize(Roles = RoleHelper.Administrador)]
    public async Task<IActionResult> ExportarMovimientos(
        string? tipo, string? fechaDesde, string? fechaHasta)
    {
        var bytes = await _svc.ExportarMovimientosExcelAsync(
            tipo, fechaDesde, fechaHasta);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"movimientos_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
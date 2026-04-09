using InventarioQR.Data;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.UtcNow;
        var hace7 = hoy.AddDays(-7);

        // ── Stats ──
        var totalProductos = await _db.Productos.CountAsync();

        var totalDisponibles = await _db.Inventarios
            .SumAsync(i => (int?)i.CantidadDisponible) ?? 0;

        var reservasActivas = await _db.Reservas
            .CountAsync(r => r.Estado == "Activa" && r.FechaExpiracion > hoy);

        // Alertas: productos con stock total < 10
        var alertas = await _db.Inventarios
            .Include(i => i.Producto)
            .GroupBy(i => new { i.ProductoId, i.Producto.Nombre, i.Producto.SKU })
            .Select(g => new AlertaStockDto
            {
                ProductoNombre = g.Key.Nombre,
                SKU = g.Key.SKU,
                StockTotal = g.Sum(x => x.CantidadDisponible),
                Nivel = g.Sum(x => x.CantidadDisponible) == 0 ? "critico" : "bajo"
            })
            .Where(a => a.StockTotal < 10)
            .OrderBy(a => a.StockTotal)
            .Take(5)
            .ToListAsync();

        // ── Movimientos recientes ──
        var movimientos = await _db.Movimientos
            .Include(m => m.Producto)
            .OrderByDescending(m => m.FechaHora)
            .Take(8)
            .Select(m => new MovimientoResumenDto
            {
                Id = m.Id,
                Tipo = m.Tipo,
                ProductoNombre = m.Producto.Nombre,
                ProductoSKU = m.Producto.SKU,
                Cantidad = m.Cantidad,
                NombreUsuario = m.NombreUsuario,
                FechaHora = m.FechaHora
            })
            .ToListAsync();

        // ── Gráfica últimos 7 días ──
        var movsPorDia = await _db.Movimientos
            .Where(m => m.FechaHora >= hace7)
            .GroupBy(m => m.FechaHora.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Entradas = g.Count(x => x.Tipo == "Entrada"),
                Salidas = g.Count(x => x.Tipo == "Salida")
            })
            .OrderBy(g => g.Fecha)
            .ToListAsync();

        var diasCompletos = Enumerable.Range(0, 7)
            .Select(i => hoy.AddDays(-6 + i).Date)
            .Select(d =>
            {
                var found = movsPorDia.FirstOrDefault(x => x.Fecha == d);
                return new MovimientoDiaDto
                {
                    Dia = d.ToString("ddd dd"),
                    Entradas = found?.Entradas ?? 0,
                    Salidas = found?.Salidas ?? 0
                };
            }).ToList();

        var vm = new DashboardViewModel
        {
            TotalProductos = totalProductos,
            TotalUnidadesDisponibles = totalDisponibles,
            TotalReservasActivas = reservasActivas,
            AlertasStockBajo = alertas.Count,
            MovimientosRecientes = movimientos,
            AlertasCriticas = alertas,
            MovimientosPorDia = diasCompletos
        };

        return View(vm);
    }
}
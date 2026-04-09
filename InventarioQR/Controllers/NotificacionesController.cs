using InventarioQR.Data;
using InventarioQR.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Controllers;

[Authorize]
public class NotificacionesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificacionesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Resumen()
    {
        var now = DateTime.UtcNow;
        var expiranHasta = now.AddHours(2);
        var creadasDesde = now.AddMinutes(-30);

        var user = await _userManager.GetUserAsync(User);
        var userId = user?.Id ?? string.Empty;
        var nombreUsuario = user?.NombreCompleto ?? User.Identity?.Name ?? string.Empty;

        var stockBajo = await _db.Inventarios
            .Include(i => i.Producto)
            .GroupBy(i => new { i.ProductoId, i.Producto.Nombre, i.Producto.SKU })
            .Select(g => new
            {
                g.Key.Nombre,
                g.Key.SKU,
                StockTotal = g.Sum(x => x.CantidadDisponible)
            })
            .Where(x => x.StockTotal < 10)
            .OrderBy(x => x.StockTotal)
            .Take(5)
            .ToListAsync();

        var reservasPorVencer = await _db.Reservas
            .Include(r => r.Producto)
            .Where(r => r.Estado == "Activa" && r.FechaExpiracion > now && r.FechaExpiracion <= expiranHasta)
            .OrderBy(r => r.FechaExpiracion)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                Producto = r.Producto.Nombre,
                r.FechaExpiracion
            })
            .ToListAsync();

        var reservasRecientes = await _db.Reservas
            .Include(r => r.Producto)
            .Where(r => r.Estado == "Activa"
                        && r.FechaCreacion >= creadasDesde
                        && r.UsuarioId == userId)
            .OrderByDescending(r => r.FechaCreacion)
            .Take(5)
            .Select(r => new
            {
                r.Id,
                Producto = r.Producto.Nombre,
                r.FechaCreacion
            })
            .ToListAsync();

        var operacionesPendientes = await _db.Operaciones
            .Include(o => o.Producto)
            .Where(o => o.Estado == "Pendiente"
                        && !string.IsNullOrEmpty(o.AsignadoA)
                        && (o.AsignadoA == nombreUsuario || o.AsignadoA == (User.Identity!.Name ?? string.Empty)))
            .OrderBy(o => o.FechaCreacion)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                o.Tipo,
                Producto = o.Producto.Nombre,
                o.FechaCreacion
            })
            .ToListAsync();

        var items = new List<object>();

        items.AddRange(stockBajo.Select(s => new
        {
            tipo = "stock",
            titulo = $"Stock bajo: {s.Nombre}",
            detalle = $"{s.StockTotal} unid. · SKU {s.SKU}",
            url = "/Inventario",
            severidad = s.StockTotal == 0 ? "critico" : "bajo"
        }));

        items.AddRange(reservasPorVencer.Select(r => new
        {
            tipo = "reserva",
            titulo = $"Reserva por vencer: #{r.Id}",
            detalle = $"{r.Producto} · expira {r.FechaExpiracion:HH:mm}",
            url = "/Reservas",
            severidad = "medio"
        }));

        items.AddRange(operacionesPendientes.Select(o => new
        {
            tipo = "operacion",
            titulo = $"Operación pendiente: #{o.Id}",
            detalle = $"{o.Tipo} · {o.Producto}",
            url = "/Operaciones",
            severidad = "medio"
        }));

        return Json(new
        {
            total = items.Count,
            conteoStockBajo = stockBajo.Count,
            conteoReservasPorVencer = reservasPorVencer.Count,
            conteoReservasRecientes = reservasRecientes.Count,
            conteoOperacionesPendientes = operacionesPendientes.Count,
            items
        });
    }
}

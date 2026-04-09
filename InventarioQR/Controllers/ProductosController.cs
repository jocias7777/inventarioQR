using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventarioQR.Models.Entities;
using InventarioQR.Helpers;

namespace InventarioQR.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _svc;
    private readonly UserManager<ApplicationUser> _um;

    public ProductosController(IProductoService svc, UserManager<ApplicationUser> um)
    {
        _svc = svc;
        _um = um;
    }

    // GET /Productos
    public async Task<IActionResult> Index(
        string? busqueda, string? estado, int pagina = 1)
    {
        var productos = await _svc.GetPagedAsync(busqueda, estado, pagina, 9);
        ViewBag.Busqueda = busqueda;
        ViewBag.Estado = estado;
        return View(productos);
    }

    // GET /Productos/Detalle/5  (AJAX panel lateral)
    public async Task<IActionResult> Detalle(int id)
    {
        var detalle = await _svc.GetDetalleAsync(id);
        if (detalle == null) return NotFound();
        return PartialView("_DetallePanel", detalle);
    }

    // GET /Productos/Crear
    [Authorize(Roles = RoleHelper.AdminYBodega)]
    public IActionResult Crear() => View(new ProductoViewModel());

    // POST /Productos/Crear
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleHelper.AdminYBodega)]
    public async Task<IActionResult> Crear(ProductoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.CrearAsync(vm, user!.Id, user.NombreCompleto);

        if (!ok) { TempData["Error"] = msg; return View(vm); }

        TempData["CreateSuccess"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // GET /Productos/Editar/5
    [Authorize(Roles = RoleHelper.AdminYBodega)]
    public async Task<IActionResult> Editar(int id)
    {
        var vm = await _svc.GetParaEditarAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // POST /Productos/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleHelper.AdminYBodega)]
    public async Task<IActionResult> Editar(ProductoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EditarAsync(vm, user!.Id, user.NombreCompleto);

        if (!ok) { TempData["Error"] = msg; return View(vm); }

        TempData["UpdateSuccess"] = msg;
        return RedirectToAction(nameof(Index));
    }

    // GET /Productos/Eliminar/5
    [Authorize(Roles = RoleHelper.Administrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var user = await _um.GetUserAsync(User);
        var (ok, msg) = await _svc.EliminarAsync(id, user!.Id, user.NombreCompleto);

        if (ok)
        {
            TempData["DeleteSuccess"] = "Producto eliminado exitosamente.";
        }
        else
        {
            TempData["Error"] = msg;
        }

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> ObtenerJson(int id)
    {
        var vm = await _svc.GetParaEditarAsync(id);
        if (vm == null)
            return NotFound();

        var resultado = new
        {
            id = vm.Id,
            sku = vm.SKU,
            nombre = vm.Nombre,
            estado = vm.Estado,
            precioBase = vm.PrecioBase,
            descripcion = vm.Descripcion,
            imagenUrl = vm.ImagenUrl,
            variantes = vm.Variantes.Select(v => new
            {
                color = v.Color,
                tamaño = v.Tamaño
            }).ToList()
        };

        return Json(resultado);
    }

    // GET /Productos/BuscarAjax?q=...
    public async Task<IActionResult> BuscarAjax(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var result = await _svc.BuscarAjaxAsync(q);
        return Json(result);
    }
}
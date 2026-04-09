using InventarioQR.Data;
using InventarioQR.Helpers;
using InventarioQR.Models.Entities;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Controllers;

[Authorize(Roles = RoleHelper.Administrador)]
public class UsuariosController : Controller
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly ApplicationDbContext _db;

    public UsuariosController(
        UserManager<ApplicationUser> um,
        ApplicationDbContext db)
    {
        _um = um;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _um.Users
            .Where(u => !u.Eliminado)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioListaDto
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email ?? "",
                Rol = u.Rol,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                UltimoAcceso = u.UltimoAcceso
            })
            .ToListAsync();

        return View(usuarios);
    }

    // GET detalle AJAX
    public async Task<IActionResult> GetUsuario(string id)
    {
        var u = await _um.FindByIdAsync(id);
        if (u == null) return NotFound();

        return Json(new
        {
            u.Id,
            u.NombreCompleto,
            u.Email,
            u.Rol,
            u.Activo
        });
    }

    // POST crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values
                .SelectMany(v => v.Errors)
                .First().ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        if (await _um.FindByEmailAsync(vm.Email) != null)
        {
            TempData["Error"] = "El correo ya está en uso.";
            return RedirectToAction(nameof(Index));
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            NombreCompleto = vm.NombreCompleto.Trim(),
            Rol = vm.Rol,
            EmailConfirmed = true,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var result = await _um.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(", ",
                result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _um.AddToRoleAsync(user, vm.Rol);
        await Auditar("CREATE", $"Usuario creado: {vm.Email} rol:{vm.Rol}", user.Id);

        TempData["Success"] = $"Usuario {vm.NombreCompleto} creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST editar
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioEditarViewModel vm)
    {
        var user = await _um.FindByIdAsync(vm.Id);
        if (user == null)
        {
            TempData["Error"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        // Prevenir auto-modificación de rol
        var currentUser = await _um.GetUserAsync(User);
        if (currentUser?.Id == vm.Id && currentUser.Rol != vm.Rol)
        {
            TempData["Error"] = "No puedes cambiar tu propio rol.";
            return RedirectToAction(nameof(Index));
        }

        // Actualizar datos
        user.NombreCompleto = vm.NombreCompleto.Trim();
        user.Activo = vm.Activo;

        // Cambiar rol si es diferente
        if (user.Rol != vm.Rol)
        {
            var rolesActuales = await _um.GetRolesAsync(user);
            await _um.RemoveFromRolesAsync(user, rolesActuales);
            await _um.AddToRoleAsync(user, vm.Rol);
            user.Rol = vm.Rol;
        }

        // Cambiar contraseña si se proveyó
        if (!string.IsNullOrWhiteSpace(vm.NuevaPassword))
        {
            var token = await _um.GeneratePasswordResetTokenAsync(user);
            var passResult = await _um.ResetPasswordAsync(
                user, token, vm.NuevaPassword);

            if (!passResult.Succeeded)
            {
                TempData["Error"] = string.Join(", ",
                    passResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        await _um.UpdateAsync(user);
        await Auditar("UPDATE",
            $"Usuario editado: {user.Email} activo:{vm.Activo} rol:{vm.Rol}",
            user.Id);

        TempData["Success"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST desactivar (soft)
    [HttpGet, ActionName("Desactivar")]
    public async Task<IActionResult> DesactivarGet(string id)
    {
        var currentUser = await _um.GetUserAsync(User);
        if (currentUser?.Id == id)
        {
            TempData["Error"] = "No puedes desactivar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _um.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        user.Activo = false;
        user.Eliminado = true;
        await _um.UpdateAsync(user);
        await Auditar("DELETE", $"Usuario desactivado: {user.Email}", user.Id);

        TempData["DeleteSuccess"] = "Usuario desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST desactivar (soft)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(string id)
    {
        var currentUser = await _um.GetUserAsync(User);
        if (currentUser?.Id == id)
        {
            TempData["Error"] = "No puedes desactivar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _um.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Usuario no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        user.Activo = false;
        user.Eliminado = true;
        await _um.UpdateAsync(user);
        await Auditar("DELETE", $"Usuario desactivado: {user.Email}", user.Id);

        TempData["DeleteSuccess"] = "Usuario desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task Auditar(string accion, string detalle, string userId)
    {
        var me = await _um.GetUserAsync(User);
        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            UsuarioId = me?.Id ?? userId,
            NombreUsuario = me?.NombreCompleto ?? "Sistema",
            Accion = accion,
            Modulo = "Usuarios",
            Detalle = detalle,
            FechaHora = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}

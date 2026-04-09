using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventarioQR.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public AuthController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _signIn = signIn;
        _userManager = userManager;
        _db = db;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !user.Activo || user.Eliminado)
        {
            ModelState.AddModelError("", "Credenciales inválidas o usuario inactivo.");
            await RegistrarAuditoria("LOGIN_FALLIDO", "Auth", $"Intento fallido: {model.Email}");
            return View(model);
        }

        var result = await _signIn.PasswordSignInAsync(
            user, model.Password, model.Recordarme, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.UltimoAcceso = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await RegistrarAuditoria("LOGIN", "Auth", $"Sesión iniciada: {user.Email}", user.Id);

            return user.Rol switch
            {
                "Administrador" => RedirectToAction("Index", "Dashboard"),
                "Bodega" => RedirectToAction("Index", "Operaciones"),
                "Vendedor" => RedirectToAction("Index", "Dashboard"),
                _ => RedirectToAction("Index", "Dashboard")
            };
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Cuenta bloqueada. Intente en 15 minutos.");
            return View(model);
        }

        ModelState.AddModelError("", "Credenciales incorrectas.");
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        await RegistrarAuditoria("LOGOUT", "Auth", $"Sesión cerrada", user?.Id);
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();

    // Helpers
    private async Task RegistrarAuditoria(
        string accion, string modulo, string detalle, string? userId = null)
    {
        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            UsuarioId = userId ?? "Anónimo",
            NombreUsuario = userId ?? "Anónimo",
            Accion = accion,
            Modulo = modulo,
            Detalle = detalle,
            IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();
    }
}
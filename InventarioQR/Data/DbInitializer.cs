using InventarioQR.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace InventarioQR.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // Roles
        string[] roles = { "Administrador", "Bodega", "Vendedor" };
        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        // Admin por defecto
        if (await userManager.FindByEmailAsync("admin@inventario.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@inventario.com",
                Email = "admin@inventario.com",
                NombreCompleto = "Administrador Principal",
                Rol = "Administrador",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@12345!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
}
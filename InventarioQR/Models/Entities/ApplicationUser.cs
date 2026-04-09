using Microsoft.AspNetCore.Identity;

namespace InventarioQR.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "Vendedor"; // Administrador | Bodega | Vendedor
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }

    // Soft delete
    public bool Eliminado { get; set; } = false;
}
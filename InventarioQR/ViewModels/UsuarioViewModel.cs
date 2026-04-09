using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class UsuarioListaDto
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class UsuarioCrearViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un rol")]
    public string Rol { get; set; } = "Vendedor";
}

public class UsuarioEditarViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un rol")]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }

    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    public string? NuevaPassword { get; set; }
}
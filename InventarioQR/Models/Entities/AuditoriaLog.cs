namespace InventarioQR.Models.Entities;

public class AuditoriaLog
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;      // CREATE | UPDATE | DELETE | LOGIN
    public string Modulo { get; set; } = string.Empty;       // Productos | Inventario | etc.
    public string? Detalle { get; set; }
    public string? IpOrigen { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
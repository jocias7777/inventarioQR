namespace InventarioQR.Models.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int PosicionId { get; set; }
    public Posicion Posicion { get; set; } = null!;
    public int Cantidad { get; set; }
    public string Estado { get; set; } = "Activa"; // Activa | Expirada | Completada
    public string UsuarioId { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
}
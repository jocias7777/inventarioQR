namespace InventarioQR.Models.Entities;

public class CodigoQR
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int PosicionId { get; set; }
    public Posicion Posicion { get; set; } = null!;
    public string Payload { get; set; } = string.Empty; // Solo contiene ID (seguridad)
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
}
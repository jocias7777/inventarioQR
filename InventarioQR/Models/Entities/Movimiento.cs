namespace InventarioQR.Models.Entities;

public class Movimiento
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Entrada | Salida | Transferencia | Ajuste
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int? PosicionOrigenId { get; set; }
    public Posicion? PosicionOrigen { get; set; }
    public int? PosicionDestinoId { get; set; }
    public Posicion? PosicionDestino { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    // NUNCA editable después de creado
}
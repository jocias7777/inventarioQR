namespace InventarioQR.Models.Entities;

public class Operacion
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int? PosicionOrigenId { get; set; }
    public Posicion? PosicionOrigen { get; set; }
    public int? PosicionDestinoId { get; set; }
    public Posicion? PosicionDestino { get; set; }
    public int Cantidad { get; set; }
    public string AsignadoA { get; set; } = string.Empty;
    public string CreadoPor { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public bool Eliminado { get; set; } = false;
}
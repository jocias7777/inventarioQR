namespace InventarioQR.Models.Entities;

public class Inventario
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int PosicionId { get; set; }
    public Posicion Posicion { get; set; } = null!;

    public int CantidadDisponible { get; set; } = 0;
    public int CantidadReservada { get; set; } = 0;
    public int CantidadDañada { get; set; } = 0;
    public int CantidadBloqueada { get; set; } = 0;
    public int CantidadEnTransito { get; set; } = 0;

    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
    public bool Eliminado { get; set; } = false;
}
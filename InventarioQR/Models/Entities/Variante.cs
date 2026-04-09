namespace InventarioQR.Models.Entities;

public class Variante
{
    public int Id { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Tamaño { get; set; } = string.Empty;
    public string SKUVariante { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public bool Eliminado { get; set; } = false;
}
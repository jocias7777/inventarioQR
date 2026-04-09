using DocumentFormat.OpenXml.VariantTypes;

namespace InventarioQR.Models.Entities;

public class Producto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public decimal PrecioBase { get; set; }
    public string Estado { get; set; } = "Activo"; // Activo | Inactivo
    public bool Eliminado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Variante> Variantes { get; set; } = new List<Variante>();
    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
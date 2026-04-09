namespace InventarioQR.Models.Entities;

public class Bodega
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public bool Activo { get; set; } = true;
    public bool Eliminado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}
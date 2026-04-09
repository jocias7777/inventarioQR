namespace InventarioQR.Models.Entities;

public class Estanteria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public int ZonaId { get; set; }
    public Zona Zona { get; set; } = null!;
    public bool Eliminado { get; set; } = false;

    public ICollection<Nivel> Niveles { get; set; } = new List<Nivel>();
}
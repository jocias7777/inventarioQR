namespace InventarioQR.Models.Entities;

public class Posicion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public int NivelId { get; set; }
    public Nivel Nivel { get; set; } = null!;
    public bool Eliminado { get; set; } = false;

    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
}
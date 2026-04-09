namespace InventarioQR.Models.Entities;

public class Zona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public bool Eliminado { get; set; } = false;

    public ICollection<Estanteria> Estanterias { get; set; } = new List<Estanteria>();
}
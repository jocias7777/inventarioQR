namespace InventarioQR.Models.Entities;

public class Nivel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public int EstanteriaId { get; set; }
    public Estanteria Estanteria { get; set; } = null!;
    public bool Eliminado { get; set; } = false;

    public ICollection<Posicion> Posiciones { get; set; } = new List<Posicion>();
}
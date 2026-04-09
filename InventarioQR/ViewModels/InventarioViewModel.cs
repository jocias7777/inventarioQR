using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class InventarioListaDto
{
    public int Id { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? ProductoImagenUrl { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string BodegaNombre { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int CantidadReservada { get; set; }
    public int CantidadDañada { get; set; }
    public int CantidadBloqueada { get; set; }
    public int CantidadEnTransito { get; set; }
    public int TotalGeneral => CantidadDisponible + CantidadReservada +
                               CantidadDañada + CantidadBloqueada + CantidadEnTransito;
    public DateTime UltimaActualizacion { get; set; }
}

public class InventarioDetalleDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? ProductoImagenUrl { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public int CantidadDisponible { get; set; }
    public int CantidadReservada { get; set; }
    public int CantidadDañada { get; set; }
    public int CantidadBloqueada { get; set; }
    public int CantidadEnTransito { get; set; }
    public DateTime UltimaActualizacion { get; set; }
    public List<MovimientoResumenDto> UltimosMovimientos { get; set; } = new();
}

public class InventarioResumenGlobalDto
{
    public int TotalRegistros { get; set; }
    public int TotalDisponible { get; set; }
    public int TotalReservado { get; set; }
    public int TotalDañado { get; set; }
    public int ProductosBajoStock { get; set; }
    public int ProductosSinStock { get; set; }
}
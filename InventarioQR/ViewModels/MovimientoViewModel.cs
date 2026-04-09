using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class MovimientoCrearViewModel
{
    [Required(ErrorMessage = "Selecciona el tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un producto")]
    public int ProductoId { get; set; }

    public int? PosicionOrigenId { get; set; }
    public int? PosicionDestinoId { get; set; }

    [Required(ErrorMessage = "Ingresa la cantidad")]
    [Range(1, 99999, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    [StringLength(300)]
    public string? Motivo { get; set; }

    // Para el dropdown
    public List<ProductoSelectDto> Productos { get; set; } = new();
    public List<PosicionSelectDto> Posiciones { get; set; } = new();
}

public class MovimientoListaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? UbicacionOrigen { get; set; }
    public string? UbicacionDestino { get; set; }
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

public class ProductoSelectDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty; // "SKU — Nombre"
}

public class PosicionSelectDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty; // "Bodega › Zona › Estant. › Pos."
}
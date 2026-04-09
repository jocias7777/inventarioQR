using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class ProductoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(0, 999999)]
    public decimal PrecioBase { get; set; }

    public string Estado { get; set; } = "Activo";
    public string? ImagenUrl { get; set; }
    public IFormFile? ImagenFile { get; set; }

    // Variantes
    public List<VarianteViewModel> Variantes { get; set; } = new();
}

public class VarianteViewModel
{
    public int Id { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Tamaño { get; set; } = string.Empty;
    public string SKUVariante { get; set; } = string.Empty;
}

public class ProductoListaDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int StockTotal { get; set; }
    public int TotalVariantes { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ProductoDetalleDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int StockTotal { get; set; }
    public int StockReservado { get; set; }
    public List<VarianteViewModel> Variantes { get; set; } = new();
    public List<UbicacionStockDto> Ubicaciones { get; set; } = new();
}

public class UbicacionStockDto
{
    public string Ubicacion { get; set; } = string.Empty;
    public int Disponible { get; set; }
    public int Reservado { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class ReservaCrearViewModel
{
    [Required(ErrorMessage = "Selecciona un producto")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Selecciona una posición")]
    public int PosicionId { get; set; }

    [Required(ErrorMessage = "Ingresa la cantidad")]
    [Range(1, 99999, ErrorMessage = "Cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }

    [StringLength(300)]
    public string? Referencia { get; set; }

    [Required]
    public int HorasExpiracion { get; set; } = 24;

    public List<ProductoSelectDto> Productos { get; set; } = new();
    public List<PosicionSelectDto> Posiciones { get; set; } = new();
}

public class ReservaListaDto
{
    public int Id { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool EstaVencida => Estado == "Activa" && FechaExpiracion < DateTime.UtcNow;
    public TimeSpan TiempoRestante => FechaExpiracion - DateTime.UtcNow;
}

public class ReservaDetalleDto : ReservaListaDto
{
    public int ProductoId { get; set; }
    public int PosicionId { get; set; }
    public string? ProductoImagenUrl { get; set; }
    public int StockActualUbicacion { get; set; }
}

public class ReservaResumenDto
{
    public int TotalActivas { get; set; }
    public int TotalExpiradas { get; set; }
    public int TotalCompletadas { get; set; }
    public int UnidadesReservadas { get; set; }
}

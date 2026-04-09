namespace InventarioQR.ViewModels;

public class QRListaDto
{
    public int Id { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public int StockDisponible { get; set; }
}

public class QRGenerarViewModel
{
    public int? ProductoId { get; set; }
    public int? PosicionId { get; set; }
    public List<ProductoSelectDto> Productos { get; set; } = new();
    public List<PosicionSelectDto> Posiciones { get; set; } = new();
}

public class QRDetalleDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? ProductoImagenUrl { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string QRImagenBase64 { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
}

// Portal de escaneo
public class EscaneoResultadoDto
{
    public bool Encontrado { get; set; }
    public string? Error { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? ProductoImagenUrl { get; set; }
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }
    public string EstadoProducto { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public int StockEnUbicacion { get; set; }
    public int StockGlobal { get; set; }
    public int StockReservado { get; set; }
    public List<OtraUbicacionDto> OtrasUbicaciones { get; set; } = new();
}

public class OtraUbicacionDto
{
    public string Ubicacion { get; set; } = string.Empty;
    public int Disponible { get; set; }
}
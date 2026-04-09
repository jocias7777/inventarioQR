namespace InventarioQR.ViewModels;

public class OperacionListaDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // Picking | Traslado | Confirmacion
    public string Estado { get; set; } = string.Empty; // Pendiente | EnProceso | Completada
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? UbicacionOrigen { get; set; }
    public string? UbicacionDestino { get; set; }
    public int Cantidad { get; set; }
    public string AsignadoA { get; set; } = string.Empty;
    public string CreadoPor { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public string? Notas { get; set; }
}

public class OperacionDetalleDto : OperacionListaDto
{
    public int ProductoId { get; set; }
    public int? PosicionOrigenId { get; set; }
    public int? PosicionDestinoId { get; set; }
    public string? ProductoImagenUrl { get; set; }
    public int StockOrigen { get; set; }
    public List<PasoOperacionDto> Pasos { get; set; } = new();
}

public class PasoOperacionDto
{
    public int Numero { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Completado { get; set; }
}

public class OperacionCrearViewModel
{
    public string Tipo { get; set; } = "Picking";
    public int ProductoId { get; set; }
    public int? PosicionOrigenId { get; set; }
    public int? PosicionDestinoId { get; set; }
    public int Cantidad { get; set; }
    public string? AsignadoA { get; set; }
    public string? Notas { get; set; }
    public List<ProductoSelectDto> Productos { get; set; } = new();
    public List<PosicionSelectDto> Posiciones { get; set; } = new();
    public List<UsuarioSelectDto> Usuarios { get; set; } = new();
}

public class UsuarioSelectDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class OperacionResumenDto
{
    public int Pendientes { get; set; }
    public int EnProceso { get; set; }
    public int CompletadasHoy { get; set; }
    public int TotalSemana { get; set; }
}
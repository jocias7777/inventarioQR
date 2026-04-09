namespace InventarioQR.ViewModels;

public class DashboardViewModel
{
    // Stats principales
    public int TotalProductos { get; set; }
    public int TotalUnidadesDisponibles { get; set; }
    public int TotalReservasActivas { get; set; }
    public int AlertasStockBajo { get; set; }

    // Movimientos recientes
    public List<MovimientoResumenDto> MovimientosRecientes { get; set; } = new();

    // Alertas críticas
    public List<AlertaStockDto> AlertasCriticas { get; set; } = new();

    // Para gráfica simple (últimos 7 días)
    public List<MovimientoDiaDto> MovimientosPorDia { get; set; } = new();
}

public class MovimientoResumenDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

public class AlertaStockDto
{
    public string ProductoNombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockTotal { get; set; }
    public string Nivel { get; set; } = "bajo"; // bajo | critico
}

public class MovimientoDiaDto
{
    public string Dia { get; set; } = string.Empty;
    public int Entradas { get; set; }
    public int Salidas { get; set; }
}
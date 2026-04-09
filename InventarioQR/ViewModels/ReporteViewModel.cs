namespace InventarioQR.ViewModels;

public class ReporteStockBajoDto
{
    public int ProductoId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public int StockTotal { get; set; }
    public int StockReservado { get; set; }
    public string Nivel { get; set; } = "bajo"; // bajo | critico | sinstock
    public List<string> Ubicaciones { get; set; } = new();
}

public class ReporteMovimientoDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSKU { get; set; } = string.Empty;
    public string? UbicacionOrigen { get; set; }
    public string? UbicacionDestino { get; set; }
    public int Cantidad { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public DateTime FechaHora { get; set; }
}

public class ReporteRotacionDto
{
    public int ProductoId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }
    public int StockActual { get; set; }
    public decimal IndiceRotacion =>
        TotalEntradas > 0
            ? Math.Round((decimal)TotalSalidas / TotalEntradas * 100, 1)
            : 0;
    public string Categoria =>
        IndiceRotacion >= 70 ? "Alta"
        : IndiceRotacion >= 30 ? "Media"
        : "Baja";
}

public class ReporteResumenGlobalDto
{
    public int TotalProductos { get; set; }
    public int TotalMovimientosMes { get; set; }
    public int ProductosSinStock { get; set; }
    public int ProductosBajoStock { get; set; }
    public int TotalEntradas { get; set; }
    public int TotalSalidas { get; set; }
    public List<GraficaDiaDto> MovimientosSemana { get; set; } = new();
    public List<GraficaTipoDto> MovimientosPorTipo { get; set; } = new();
}

public class GraficaDiaDto
{
    public string Dia { get; set; } = string.Empty;
    public int Entradas { get; set; }
    public int Salidas { get; set; }
    public int Transferencias { get; set; }
}

public class GraficaTipoDto
{
    public string Tipo { get; set; } = string.Empty;
    public int Total { get; set; }
}
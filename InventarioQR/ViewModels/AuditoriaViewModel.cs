namespace InventarioQR.ViewModels;

public class AuditoriaListaDto
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string? IpOrigen { get; set; }
    public DateTime FechaHora { get; set; }
}

public class AuditoriaFiltrosDto
{
    public string? Usuario { get; set; }
    public string? Accion { get; set; }
    public string? Modulo { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public class AuditoriaResumenDto
{
    public int TotalHoy { get; set; }
    public int TotalSemana { get; set; }
    public string AccionMasFrecuente { get; set; } = string.Empty;
    public string ModuloMasActivo { get; set; } = string.Empty;
    public List<AccionCountDto> AccionesPorTipo { get; set; } = new();
}

public class AccionCountDto
{
    public string Accion { get; set; } = string.Empty;
    public int Count { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace InventarioQR.ViewModels;

public class BodegaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoInterno { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [StringLength(200)]
    public string? Ubicacion { get; set; }
}

public class ZonaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoInterno { get; set; } = string.Empty;

    [Required]
    public int BodegaId { get; set; }
}

public class EstanteriaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoInterno { get; set; } = string.Empty;

    [Required]
    public int ZonaId { get; set; }
}

public class NivelFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoInterno { get; set; } = string.Empty;

    [Required]
    public int EstanteriaId { get; set; }
}

public class PosicionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoInterno { get; set; } = string.Empty;

    [Required]
    public int NivelId { get; set; }
}

// Vista completa del árbol
public class BodegaArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public bool Activo { get; set; }
    public int TotalPosiciones { get; set; }
    public int TotalConStock { get; set; }
    public List<ZonaArbolDto> Zonas { get; set; } = new();
}

public class ZonaArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public List<EstanteriaArbolDto> Estanterias { get; set; } = new();
}

public class EstanteriaArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public List<NivelArbolDto> Niveles { get; set; } = new();
}

public class NivelArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public List<PosicionArbolDto> Posiciones { get; set; } = new();
}

public class PosicionArbolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public int StockDisponible { get; set; }
}

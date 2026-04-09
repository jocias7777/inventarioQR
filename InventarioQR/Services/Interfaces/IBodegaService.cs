using InventarioQR.ViewModels;

namespace InventarioQR.Services.Interfaces;

public interface IBodegaService
{
    Task<List<BodegaArbolDto>> GetArbolAsync();

    // Bodega
    Task<(bool ok, string msg)> CrearBodegaAsync(BodegaFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EditarBodegaAsync(BodegaFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EliminarBodegaAsync(int id, string uid, string uname);
    Task<BodegaFormViewModel?> GetBodegaAsync(int id);

    // Zona
    Task<(bool ok, string msg)> CrearZonaAsync(ZonaFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EditarZonaAsync(ZonaFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EliminarZonaAsync(int id, string uid, string uname);
    Task<ZonaFormViewModel?> GetZonaAsync(int id);

    // Estantería
    Task<(bool ok, string msg)> CrearEstanteriaAsync(EstanteriaFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EliminarEstanteriaAsync(int id, string uid, string uname);
    Task<EstanteriaFormViewModel?> GetEstanteriaAsync(int id);

    // Nivel
    Task<(bool ok, string msg)> CrearNivelAsync(NivelFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EliminarNivelAsync(int id, string uid, string uname);
    Task<NivelFormViewModel?> GetNivelAsync(int id);

    // Posición
    Task<(bool ok, string msg)> CrearPosicionAsync(PosicionFormViewModel vm, string uid, string uname);
    Task<(bool ok, string msg)> EliminarPosicionAsync(int id, string uid, string uname);
    Task<PosicionFormViewModel?> GetPosicionAsync(int id);

    // Selects
    Task<List<(int Id, string Nombre)>> GetBodegasSelectAsync();
    Task<List<(int Id, string Nombre)>> GetZonasSelectAsync(int bodegaId);
    Task<List<(int Id, string Nombre)>> GetEstanteriasSelectAsync(int zonaId);
    Task<List<(int Id, string Nombre)>> GetNivelesSelectAsync(int estanteriaId);
}

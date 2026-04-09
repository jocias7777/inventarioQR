using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IReservaService
{
    Task<IPagedList<ReservaListaDto>> GetPagedAsync(
        string? estado, string? busqueda, int pagina, int tamano);

    Task<ReservaDetalleDto?> GetDetalleAsync(int id);
    Task<ReservaResumenDto> GetResumenAsync();
    Task<ReservaCrearViewModel> PrepararFormularioAsync();

    Task<(bool ok, string msg)> CrearAsync(
        ReservaCrearViewModel vm, string userId, string userName);

    Task<(bool ok, string msg)> CompletarAsync(
        int id, string userId, string userName);

    Task<(bool ok, string msg)> CancelarAsync(
        int id, string userId, string userName);

    Task ProcesarExpiradas();
}

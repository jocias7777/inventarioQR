using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IOperacionService
{
    Task<IPagedList<OperacionListaDto>> GetPagedAsync(
        string? tipo, string? estado, int pagina, int tamano);

    Task<OperacionDetalleDto?> GetDetalleAsync(int id);
    Task<OperacionResumenDto> GetResumenAsync();
    Task<OperacionCrearViewModel> PrepararFormularioAsync();

    Task<(bool ok, string msg)> CrearAsync(
        OperacionCrearViewModel vm, string userId, string userName);

    Task<(bool ok, string msg)> IniciarAsync(
        int id, string userId, string userName);

    Task<(bool ok, string msg)> CompletarAsync(
        int id, string payloadQR, string userId, string userName);

    Task<(bool ok, string msg)> CancelarAsync(
        int id, string userId, string userName);
}
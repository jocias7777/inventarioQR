using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IMovimientoService
{
    Task<IPagedList<MovimientoListaDto>> GetPagedAsync(
        string? tipo, string? busqueda, int pagina, int tamano);

    Task<MovimientoCrearViewModel> PrepararFormularioAsync();

    Task<(bool ok, string mensaje)> RegistrarAsync(
        MovimientoCrearViewModel vm, string userId, string userName);
}
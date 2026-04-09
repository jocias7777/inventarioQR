using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IInventarioService
{
    Task<IPagedList<InventarioListaDto>> GetPagedAsync(
        string? busqueda, string? bodega, int pagina, int tamano);

    Task<InventarioDetalleDto?> GetDetalleAsync(int id);
    Task<InventarioResumenGlobalDto> GetResumenAsync();
    Task<List<string>> GetBodegasAsync();
}
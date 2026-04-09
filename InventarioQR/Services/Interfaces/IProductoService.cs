using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IProductoService
{
    Task<IPagedList<ProductoListaDto>> GetPagedAsync(
        string? busqueda, string? estado, int pagina, int tamano);

    Task<ProductoDetalleDto?> GetDetalleAsync(int id);
    Task<ProductoViewModel?> GetParaEditarAsync(int id);
    Task<(bool ok, string mensaje)> CrearAsync(ProductoViewModel vm, string userId, string userName);
    Task<(bool ok, string mensaje)> EditarAsync(ProductoViewModel vm, string userId, string userName);
    Task<(bool ok, string mensaje)> EliminarAsync(int id, string userId, string userName);
    Task<List<ProductoListaDto>> BuscarAjaxAsync(string q);
}
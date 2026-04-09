using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IQRService
{
    Task<IPagedList<QRListaDto>> GetPagedAsync(
        int? productoId, int pagina, int tamano);

    Task<QRDetalleDto?> GetDetalleAsync(int id);
    Task<string> GenerarImagenBase64Async(string payload);

    Task<(bool ok, string msg, int qrId)> GenerarAsync(
        int productoId, int posicionId,
        string userId, string userName);

    Task<(bool ok, string msg)> GenerarMasivoAsync(
        int productoId, List<int> posicionIds,
        string userId, string userName);

    Task<(bool ok, string msg)> EliminarAsync(
        int id, string userId, string userName);

    Task<QRGenerarViewModel> PrepararFormularioAsync();

    // Portal escaneo
    Task<EscaneoResultadoDto> EscanearAsync(string payload);
}
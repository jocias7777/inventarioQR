using InventarioQR.ViewModels;
using X.PagedList;

namespace InventarioQR.Services.Interfaces;

public interface IReporteService
{
    Task<ReporteResumenGlobalDto> GetResumenAsync();
    Task<List<ReporteStockBajoDto>> GetStockBajoAsync(int limite = 50);
    Task<IPagedList<ReporteMovimientoDto>> GetMovimientosAsync(
        string? tipo, string? fechaDesde,
        string? fechaHasta, int pagina, int tamano);
    Task<List<ReporteRotacionDto>> GetRotacionAsync(int dias = 30);
    Task<byte[]> ExportarStockBajoExcelAsync();
    Task<byte[]> ExportarMovimientosExcelAsync(
        string? tipo, string? fechaDesde, string? fechaHasta);
}
using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class MovimientoService : IMovimientoService
{
    private readonly ApplicationDbContext _db;

    public MovimientoService(ApplicationDbContext db) => _db = db;

    public async Task<IPagedList<MovimientoListaDto>> GetPagedAsync(
        string? tipo, string? busqueda, int pagina, int tamano)
    {
        var q = _db.Movimientos
            .Include(m => m.Producto)
            .Include(m => m.PosicionOrigen)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .Include(m => m.PosicionDestino)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(m => m.Tipo == tipo);

        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(m =>
                m.Producto.Nombre.Contains(busqueda) ||
                m.Producto.SKU.Contains(busqueda) ||
                m.NombreUsuario.Contains(busqueda));

        var lista = q.OrderByDescending(m => m.FechaHora)
            .Select(m => new MovimientoListaDto
            {
                Id = m.Id,
                Tipo = m.Tipo,
                ProductoNombre = m.Producto.Nombre,
                ProductoSKU = m.Producto.SKU,
                Cantidad = m.Cantidad,
                Motivo = m.Motivo,
                NombreUsuario = m.NombreUsuario,
                FechaHora = m.FechaHora,
                UbicacionOrigen = m.PosicionOrigen == null ? null :
                    m.PosicionOrigen.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    m.PosicionOrigen.Nombre,
                UbicacionDestino = m.PosicionDestino == null ? null :
                    m.PosicionDestino.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    m.PosicionDestino.Nombre
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<MovimientoCrearViewModel> PrepararFormularioAsync()
    {
        var productos = await _db.Productos
            .Where(p => p.Estado == "Activo")
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoSelectDto
            {
                Id = p.Id,
                Texto = $"{p.SKU} — {p.Nombre}"
            })
            .AsNoTracking()
            .ToListAsync();

        var posiciones = await _db.Posiciones
            .Include(p => p.Nivel)
                .ThenInclude(n => n.Estanteria)
                    .ThenInclude(e => e.Zona)
                        .ThenInclude(z => z.Bodega)
            .OrderBy(p => p.Nivel.Estanteria.Zona.Bodega.Nombre)
            .Select(p => new PosicionSelectDto
            {
                Id = p.Id,
                Texto = p.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                        p.Nivel.Estanteria.Zona.Nombre + " › " +
                        p.Nivel.Estanteria.Nombre + " › " +
                        p.Nombre
            })
            .AsNoTracking()
            .ToListAsync();

        return new MovimientoCrearViewModel
        {
            Productos = productos,
            Posiciones = posiciones
        };
    }

    public async Task<(bool ok, string mensaje)> RegistrarAsync(
        MovimientoCrearViewModel vm, string userId, string userName)
    {
        // ── Validaciones de negocio ──
        switch (vm.Tipo)
        {
            case "Entrada":
                if (vm.PosicionDestinoId == null)
                    return (false, "Entrada requiere una posición destino.");
                break;

            case "Salida":
                if (vm.PosicionOrigenId == null)
                    return (false, "Salida requiere una posición origen.");

                var invSalida = await _db.Inventarios
                    .FirstOrDefaultAsync(i =>
                        i.ProductoId == vm.ProductoId &&
                        i.PosicionId == vm.PosicionOrigenId);

                if (invSalida == null || invSalida.CantidadDisponible < vm.Cantidad)
                    return (false,
                        $"Stock insuficiente. Disponible: {invSalida?.CantidadDisponible ?? 0}");
                break;

            case "Transferencia":
                if (vm.PosicionOrigenId == null || vm.PosicionDestinoId == null)
                    return (false, "Transferencia requiere origen y destino.");

                if (vm.PosicionOrigenId == vm.PosicionDestinoId)
                    return (false, "Origen y destino no pueden ser iguales.");

                var invTrans = await _db.Inventarios
                    .FirstOrDefaultAsync(i =>
                        i.ProductoId == vm.ProductoId &&
                        i.PosicionId == vm.PosicionOrigenId);

                if (invTrans == null || invTrans.CantidadDisponible < vm.Cantidad)
                    return (false,
                        $"Stock insuficiente para transferir. Disponible: {invTrans?.CantidadDisponible ?? 0}");
                break;

            case "Ajuste":
                if (vm.PosicionOrigenId == null && vm.PosicionDestinoId == null)
                    return (false, "Ajuste requiere al menos una posición.");
                break;

            default:
                return (false, "Tipo de movimiento no válido.");
        }

        // ── Ejecutar en transacción ──
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1. Crear movimiento (inmutable)
            var movimiento = new Movimiento
            {
                Tipo = vm.Tipo,
                ProductoId = vm.ProductoId,
                PosicionOrigenId = vm.PosicionOrigenId,
                PosicionDestinoId = vm.PosicionDestinoId,
                Cantidad = vm.Cantidad,
                Motivo = vm.Motivo?.Trim(),
                UsuarioId = userId,
                NombreUsuario = userName,
                FechaHora = DateTime.UtcNow
            };
            _db.Movimientos.Add(movimiento);

            // 2. Afectar inventario según tipo
            await AplicarImpactoAsync(vm);

            // 3. Auditoría
            _db.AuditoriaLogs.Add(new AuditoriaLog
            {
                UsuarioId = userId,
                NombreUsuario = userName,
                Accion = "CREATE",
                Modulo = "Movimientos",
                Detalle = $"{vm.Tipo} de {vm.Cantidad} uds — ProductoId:{vm.ProductoId}",
                FechaHora = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, $"{vm.Tipo} registrado correctamente.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error al registrar movimiento: {ex.Message}");
        }
    }

    // ── Lógica de impacto en inventario ──
    private async Task AplicarImpactoAsync(MovimientoCrearViewModel vm)
    {
        switch (vm.Tipo)
        {
            case "Entrada":
                var invEntrada = await ObtenerOCrearInventarioAsync(
                    vm.ProductoId, vm.PosicionDestinoId!.Value);
                invEntrada.CantidadDisponible += vm.Cantidad;
                invEntrada.UltimaActualizacion = DateTime.UtcNow;
                break;

            case "Salida":
                var invSalida = await _db.Inventarios.FirstAsync(i =>
                    i.ProductoId == vm.ProductoId &&
                    i.PosicionId == vm.PosicionOrigenId);
                invSalida.CantidadDisponible -= vm.Cantidad;
                invSalida.UltimaActualizacion = DateTime.UtcNow;
                break;

            case "Transferencia":
                var origen = await _db.Inventarios.FirstAsync(i =>
                    i.ProductoId == vm.ProductoId &&
                    i.PosicionId == vm.PosicionOrigenId);
                origen.CantidadDisponible -= vm.Cantidad;
                origen.UltimaActualizacion = DateTime.UtcNow;

                var destino = await ObtenerOCrearInventarioAsync(
                    vm.ProductoId, vm.PosicionDestinoId!.Value);
                destino.CantidadDisponible += vm.Cantidad;
                destino.UltimaActualizacion = DateTime.UtcNow;
                break;

            case "Ajuste":
                var posId = vm.PosicionOrigenId ?? vm.PosicionDestinoId!.Value;
                var invAjuste = await ObtenerOCrearInventarioAsync(vm.ProductoId, posId);
                // Ajuste: la cantidad puede ser positiva o negativa
                // Se suma directamente (el operador indica el sentido)
                invAjuste.CantidadDisponible = Math.Max(0,
                    invAjuste.CantidadDisponible + vm.Cantidad);
                invAjuste.UltimaActualizacion = DateTime.UtcNow;
                break;
        }
    }

    private async Task<Inventario> ObtenerOCrearInventarioAsync(
        int productoId, int posicionId)
    {
        var inv = await _db.Inventarios
            .FirstOrDefaultAsync(i =>
                i.ProductoId == productoId && i.PosicionId == posicionId);

        if (inv != null) return inv;

        inv = new Inventario
        {
            ProductoId = productoId,
            PosicionId = posicionId
        };
        _db.Inventarios.Add(inv);
        return inv;
    }
}
using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class ReservaService : IReservaService
{
    private readonly ApplicationDbContext _db;
    public ReservaService(ApplicationDbContext db) => _db = db;

    public async Task<IPagedList<ReservaListaDto>> GetPagedAsync(
        string? estado, string? busqueda, int pagina, int tamano)
    {
        var q = _db.Reservas
            .Include(r => r.Producto)
            .Include(r => r.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(r => r.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
            q = q.Where(r =>
                r.Producto.Nombre.Contains(busqueda) ||
                r.Producto.SKU.Contains(busqueda) ||
                (r.Referencia != null && r.Referencia.Contains(busqueda)));

        var lista = q.OrderByDescending(r => r.FechaCreacion)
            .Select(r => new ReservaListaDto
            {
                Id = r.Id,
                ProductoNombre = r.Producto.Nombre,
                ProductoSKU = r.Producto.SKU,
                Cantidad = r.Cantidad,
                Estado = r.Estado,
                Referencia = r.Referencia,
                NombreUsuario = r.UsuarioId,
                FechaCreacion = r.FechaCreacion,
                FechaExpiracion = r.FechaExpiracion,
                Ubicacion =
                    r.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                    r.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                    r.Posicion.Nivel.Estanteria.Nombre + " › " +
                    r.Posicion.Nombre
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<ReservaDetalleDto?> GetDetalleAsync(int id)
    {
        var r = await _db.Reservas
            .Include(x => x.Producto)
            .Include(x => x.Posicion)
                .ThenInclude(p => p.Nivel)
                    .ThenInclude(n => n.Estanteria)
                        .ThenInclude(e => e.Zona)
                            .ThenInclude(z => z.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return null;

        var stock = await _db.Inventarios
            .Where(i => i.ProductoId == r.ProductoId &&
                        i.PosicionId == r.PosicionId)
            .Select(i => i.CantidadDisponible)
            .FirstOrDefaultAsync();

        return new ReservaDetalleDto
        {
            Id = r.Id,
            ProductoId = r.ProductoId,
            PosicionId = r.PosicionId,
            ProductoNombre = r.Producto.Nombre,
            ProductoSKU = r.Producto.SKU,
            ProductoImagenUrl = r.Producto.ImagenUrl,
            Cantidad = r.Cantidad,
            Estado = r.Estado,
            Referencia = r.Referencia,
            NombreUsuario = r.UsuarioId,
            FechaCreacion = r.FechaCreacion,
            FechaExpiracion = r.FechaExpiracion,
            StockActualUbicacion = stock,
            Ubicacion =
                r.Posicion.Nivel.Estanteria.Zona.Bodega.Nombre + " › " +
                r.Posicion.Nivel.Estanteria.Zona.Nombre + " › " +
                r.Posicion.Nivel.Estanteria.Nombre + " › " +
                r.Posicion.Nombre
        };
    }

    public async Task<ReservaResumenDto> GetResumenAsync()
    {
        var ahora = DateTime.UtcNow;

        var activas = await _db.Reservas
            .CountAsync(r => r.Estado == "Activa" && r.FechaExpiracion > ahora);

        var expiradas = await _db.Reservas
            .CountAsync(r => r.Estado == "Expirada" ||
                            (r.Estado == "Activa" && r.FechaExpiracion <= ahora));

        var completadas = await _db.Reservas
            .CountAsync(r => r.Estado == "Completada");

        var unidades = await _db.Reservas
            .Where(r => r.Estado == "Activa" && r.FechaExpiracion > ahora)
            .SumAsync(r => (int?)r.Cantidad) ?? 0;

        return new ReservaResumenDto
        {
            TotalActivas = activas,
            TotalExpiradas = expiradas,
            TotalCompletadas = completadas,
            UnidadesReservadas = unidades
        };
    }

    public async Task<ReservaCrearViewModel> PrepararFormularioAsync()
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

        return new ReservaCrearViewModel
        {
            Productos = productos,
            Posiciones = posiciones
        };
    }

    public async Task<(bool ok, string msg)> CrearAsync(
        ReservaCrearViewModel vm, string userId, string userName)
    {
        // Verificar stock disponible
        var inv = await _db.Inventarios
            .FirstOrDefaultAsync(i =>
                i.ProductoId == vm.ProductoId &&
                i.PosicionId == vm.PosicionId);

        if (inv == null || inv.CantidadDisponible < vm.Cantidad)
            return (false,
                $"Stock insuficiente. Disponible: {inv?.CantidadDisponible ?? 0}");

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Reducir disponible, aumentar reservado
            inv.CantidadDisponible -= vm.Cantidad;
            inv.CantidadReservada += vm.Cantidad;
            inv.UltimaActualizacion = DateTime.UtcNow;

            var reserva = new Reserva
            {
                ProductoId = vm.ProductoId,
                PosicionId = vm.PosicionId,
                Cantidad = vm.Cantidad,
                Estado = "Activa",
                UsuarioId = userId,
                Referencia = vm.Referencia?.Trim(),
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(vm.HorasExpiracion)
            };

            _db.Reservas.Add(reserva);

            // Movimiento automático
            _db.Movimientos.Add(new Movimiento
            {
                Tipo = "Ajuste",
                ProductoId = vm.ProductoId,
                PosicionOrigenId = vm.PosicionId,
                Cantidad = vm.Cantidad,
                Motivo = $"Reserva creada — {vm.Referencia ?? "sin ref."}",
                UsuarioId = userId,
                NombreUsuario = userName,
                FechaHora = DateTime.UtcNow
            });

            await Auditar("CREATE", "Reservas",
                $"Reserva creada: {vm.Cantidad} uds ProductoId:{vm.ProductoId}",
                userId, userName);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Reserva creada correctamente.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error al crear reserva: {ex.Message}");
        }
    }

    public async Task<(bool ok, string msg)> CompletarAsync(
        int id, string userId, string userName)
    {
        var reserva = await _db.Reservas
            .Include(r => r.Producto)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva == null) return (false, "Reserva no encontrada.");
        if (reserva.Estado != "Activa")
            return (false, $"No se puede completar: estado actual es '{reserva.Estado}'.");

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            reserva.Estado = "Completada";

            // Reducir cantidad reservada del inventario
            var inv = await _db.Inventarios
                .FirstOrDefaultAsync(i =>
                    i.ProductoId == reserva.ProductoId &&
                    i.PosicionId == reserva.PosicionId);

            if (inv != null)
            {
                inv.CantidadReservada = Math.Max(0,
                    inv.CantidadReservada - reserva.Cantidad);
                inv.UltimaActualizacion = DateTime.UtcNow;
            }

            // Movimiento de salida
            _db.Movimientos.Add(new Movimiento
            {
                Tipo = "Salida",
                ProductoId = reserva.ProductoId,
                PosicionOrigenId = reserva.PosicionId,
                Cantidad = reserva.Cantidad,
                Motivo = $"Completar reserva #{reserva.Id}",
                UsuarioId = userId,
                NombreUsuario = userName,
                FechaHora = DateTime.UtcNow
            });

            await Auditar("UPDATE", "Reservas",
                $"Reserva #{id} completada",
                userId, userName);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Reserva completada y salida registrada.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool ok, string msg)> CancelarAsync(
        int id, string userId, string userName)
    {
        var reserva = await _db.Reservas
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva == null) return (false, "Reserva no encontrada.");
        if (reserva.Estado == "Completada")
            return (false, "No se puede cancelar una reserva completada.");

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            reserva.Estado = "Expirada";

            // Devolver al disponible
            var inv = await _db.Inventarios
                .FirstOrDefaultAsync(i =>
                    i.ProductoId == reserva.ProductoId &&
                    i.PosicionId == reserva.PosicionId);

            if (inv != null)
            {
                inv.CantidadDisponible += reserva.Cantidad;
                inv.CantidadReservada = Math.Max(0,
                    inv.CantidadReservada - reserva.Cantidad);
                inv.UltimaActualizacion = DateTime.UtcNow;
            }

            await Auditar("UPDATE", "Reservas",
                $"Reserva #{id} cancelada/expirada",
                userId, userName);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, "Reserva cancelada. Stock devuelto al inventario.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task ProcesarExpiradas()
    {
        var vencidas = await _db.Reservas
            .Include(r => r.Producto)
            .Where(r => r.Estado == "Activa" &&
                        r.FechaExpiracion <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var r in vencidas)
        {
            r.Estado = "Expirada";

            var inv = await _db.Inventarios
                .FirstOrDefaultAsync(i =>
                    i.ProductoId == r.ProductoId &&
                    i.PosicionId == r.PosicionId);

            if (inv != null)
            {
                inv.CantidadDisponible += r.Cantidad;
                inv.CantidadReservada = Math.Max(0,
                    inv.CantidadReservada - r.Cantidad);
                inv.UltimaActualizacion = DateTime.UtcNow;
            }
        }

        if (vencidas.Any())
            await _db.SaveChangesAsync();
    }

    private async Task Auditar(
        string accion, string modulo, string detalle,
        string userId, string userName)
    {
        _db.AuditoriaLogs.Add(new AuditoriaLog
        {
            UsuarioId = userId,
            NombreUsuario = userName,
            Accion = accion,
            Modulo = modulo,
            Detalle = detalle,
            FechaHora = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}

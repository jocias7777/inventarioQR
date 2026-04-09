using InventarioQR.Data;
using InventarioQR.Models.Entities;
using InventarioQR.Services.Interfaces;
using InventarioQR.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace InventarioQR.Services;

public class OperacionService : IOperacionService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public OperacionService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }

    public async Task<IPagedList<OperacionListaDto>> GetPagedAsync(
        string? tipo, string? estado, int pagina, int tamano)
    {
        var q = _db.Operaciones
            .Include(o => o.Producto)
            .Include(o => o.PosicionOrigen)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .Include(o => o.PosicionDestino)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(o => o.Tipo == tipo);

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(o => o.Estado == estado);

        var lista = q.OrderByDescending(o => o.FechaCreacion)
            .Select(o => new OperacionListaDto
            {
                Id = o.Id,
                Tipo = o.Tipo,
                Estado = o.Estado,
                ProductoNombre = o.Producto.Nombre,
                ProductoSKU = o.Producto.SKU,
                Cantidad = o.Cantidad,
                AsignadoA = o.AsignadoA,
                CreadoPor = o.CreadoPor,
                Notas = o.Notas,
                FechaCreacion = o.FechaCreacion,
                FechaCompletado = o.FechaCompletado,
                UbicacionOrigen = o.PosicionOrigen == null ? null :
                    o.PosicionOrigen.Nivel.Estanteria.Zona.Bodega.Nombre +
                    " › " + o.PosicionOrigen.Nombre,
                UbicacionDestino = o.PosicionDestino == null ? null :
                    o.PosicionDestino.Nivel.Estanteria.Zona.Bodega.Nombre +
                    " › " + o.PosicionDestino.Nombre
            });

        return lista.ToPagedList(pagina, tamano);
    }

    public async Task<OperacionDetalleDto?> GetDetalleAsync(int id)
    {
        var o = await _db.Operaciones
            .Include(x => x.Producto)
            .Include(x => x.PosicionOrigen)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .Include(x => x.PosicionDestino)
                .ThenInclude(p => p!.Nivel.Estanteria.Zona.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (o == null) return null;

        var stockOrigen = o.PosicionOrigenId.HasValue
            ? await _db.Inventarios
                .Where(i => i.ProductoId == o.ProductoId &&
                            i.PosicionId == o.PosicionOrigenId)
                .Select(i => i.CantidadDisponible)
                .FirstOrDefaultAsync()
            : 0;

        // Pasos según tipo
        var pasos = o.Tipo switch
        {
            "Picking" => new List<PasoOperacionDto>
            {
                new() { Numero=1, Descripcion="Ir a la ubicación origen",
                    Completado = o.Estado != "Pendiente" },
                new() { Numero=2, Descripcion=$"Tomar {o.Cantidad} unidades de {o.Producto.Nombre}",
                    Completado = o.Estado == "Completada" },
                new() { Numero=3, Descripcion="Escanear QR de confirmación",
                    Completado = o.Estado == "Completada" },
                new() { Numero=4, Descripcion="Llevar a zona de despacho",
                    Completado = o.Estado == "Completada" }
            },
            "Traslado" => new List<PasoOperacionDto>
            {
                new() { Numero=1, Descripcion="Ir a la posición origen",
                    Completado = o.Estado != "Pendiente" },
                new() { Numero=2, Descripcion=$"Retirar {o.Cantidad} unidades",
                    Completado = o.Estado == "Completada" },
                new() { Numero=3, Descripcion="Trasladar a posición destino",
                    Completado = o.Estado == "Completada" },
                new() { Numero=4, Descripcion="Escanear QR destino para confirmar",
                    Completado = o.Estado == "Completada" }
            },
            _ => new List<PasoOperacionDto>
            {
                new() { Numero=1, Descripcion="Verificar inventario físico",
                    Completado = o.Estado != "Pendiente" },
                new() { Numero=2, Descripcion="Confirmar cantidades",
                    Completado = o.Estado == "Completada" },
                new() { Numero=3, Descripcion="Escanear QR y confirmar",
                    Completado = o.Estado == "Completada" }
            }
        };

        return new OperacionDetalleDto
        {
            Id = o.Id,
            Tipo = o.Tipo,
            Estado = o.Estado,
            ProductoId = o.ProductoId,
            ProductoNombre = o.Producto.Nombre,
            ProductoSKU = o.Producto.SKU,
            ProductoImagenUrl = o.Producto.ImagenUrl,
            PosicionOrigenId = o.PosicionOrigenId,
            PosicionDestinoId = o.PosicionDestinoId,
            Cantidad = o.Cantidad,
            AsignadoA = o.AsignadoA,
            CreadoPor = o.CreadoPor,
            Notas = o.Notas,
            FechaCreacion = o.FechaCreacion,
            FechaCompletado = o.FechaCompletado,
            StockOrigen = stockOrigen,
            Pasos = pasos,
            UbicacionOrigen = o.PosicionOrigen == null ? null :
                o.PosicionOrigen.Nivel.Estanteria.Zona.Bodega.Nombre +
                " › " + o.PosicionOrigen.Nombre,
            UbicacionDestino = o.PosicionDestino == null ? null :
                o.PosicionDestino.Nivel.Estanteria.Zona.Bodega.Nombre +
                " › " + o.PosicionDestino.Nombre
        };
    }

    public async Task<OperacionResumenDto> GetResumenAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var semana = DateTime.UtcNow.AddDays(-7);

        return new OperacionResumenDto
        {
            Pendientes = await _db.Operaciones
                .CountAsync(o => o.Estado == "Pendiente"),
            EnProceso = await _db.Operaciones
                .CountAsync(o => o.Estado == "EnProceso"),
            CompletadasHoy = await _db.Operaciones
                .CountAsync(o => o.Estado == "Completada" &&
                                 o.FechaCompletado >= hoy),
            TotalSemana = await _db.Operaciones
                .CountAsync(o => o.FechaCreacion >= semana)
        };
    }

    public async Task<OperacionCrearViewModel> PrepararFormularioAsync()
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
            .Include(p => p.Nivel.Estanteria.Zona.Bodega)
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

        var usuarios = await _um.Users
            .Where(u => u.Activo && !u.Eliminado &&
                       (u.Rol == "Bodega" || u.Rol == "Administrador"))
            .Select(u => new UsuarioSelectDto
            {
                Id = u.Id,
                Nombre = u.NombreCompleto
            })
            .ToListAsync();

        return new OperacionCrearViewModel
        {
            Productos = productos,
            Posiciones = posiciones,
            Usuarios = usuarios
        };
    }

    public async Task<(bool ok, string msg)> CrearAsync(
        OperacionCrearViewModel vm, string userId, string userName)
    {
        if (vm.Tipo == "Traslado" &&
           (vm.PosicionOrigenId == null || vm.PosicionDestinoId == null))
            return (false, "Traslado requiere origen y destino.");

        if (vm.Tipo == "Picking" && vm.PosicionOrigenId == null)
            return (false, "Picking requiere posición origen.");

        var op = new Operacion
        {
            Tipo = vm.Tipo,
            Estado = "Pendiente",
            ProductoId = vm.ProductoId,
            PosicionOrigenId = vm.PosicionOrigenId,
            PosicionDestinoId = vm.PosicionDestinoId,
            Cantidad = vm.Cantidad,
            AsignadoA = vm.AsignadoA ?? userName,
            CreadoPor = userName,
            Notas = vm.Notas?.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _db.Operaciones.Add(op);
        await _db.SaveChangesAsync();

        await Auditar("CREATE", "Operaciones",
            $"Op.{vm.Tipo} creada #{op.Id} prod:{vm.ProductoId}",
            userId, userName);

        return (true, $"Operación #{op.Id} creada y asignada.");
    }

    public async Task<(bool ok, string msg)> IniciarAsync(
        int id, string userId, string userName)
    {
        var op = await _db.Operaciones.FindAsync(id);
        if (op == null) return (false, "Operación no encontrada.");
        if (op.Estado != "Pendiente")
            return (false, $"Estado actual: {op.Estado}. Solo Pendiente puede iniciarse.");

        op.Estado = "EnProceso";
        op.FechaInicio = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await Auditar("UPDATE", "Operaciones",
            $"Op #{id} iniciada", userId, userName);

        return (true, "Operación iniciada. Sigue los pasos en pantalla.");
    }

    public async Task<(bool ok, string msg)> CompletarAsync(
        int id, string payloadQR, string userId, string userName)
    {
        var op = await _db.Operaciones
            .Include(o => o.Producto)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (op == null) return (false, "Operación no encontrada.");
        if (op.Estado == "Completada")
            return (false, "Ya está completada.");

        // Validar QR si se proveyó
        if (!string.IsNullOrWhiteSpace(payloadQR))
        {
            var posId = op.Tipo == "Traslado"
                ? op.PosicionDestinoId
                : op.PosicionOrigenId;

            var qrValido = await _db.CodigosQR
                .AnyAsync(q => q.Payload == payloadQR &&
                               q.ProductoId == op.ProductoId &&
                               q.PosicionId == posId);

            if (!qrValido)
                return (false,
                    "QR inválido. Escanea el QR correcto para esta ubicación.");
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            op.Estado = "Completada";
            op.FechaCompletado = DateTime.UtcNow;

            // Generar movimiento automático
            if (op.Tipo == "Picking" || op.Tipo == "Confirmacion")
            {
                var inv = await _db.Inventarios
                    .FirstOrDefaultAsync(i =>
                        i.ProductoId == op.ProductoId &&
                        i.PosicionId == op.PosicionOrigenId);

                if (inv != null && inv.CantidadDisponible >= op.Cantidad)
                {
                    inv.CantidadDisponible -= op.Cantidad;
                    inv.UltimaActualizacion = DateTime.UtcNow;
                }

                _db.Movimientos.Add(new Movimiento
                {
                    Tipo = "Salida",
                    ProductoId = op.ProductoId,
                    PosicionOrigenId = op.PosicionOrigenId,
                    Cantidad = op.Cantidad,
                    Motivo = $"Op.{op.Tipo} #{op.Id}",
                    UsuarioId = userId,
                    NombreUsuario = userName,
                    FechaHora = DateTime.UtcNow
                });
            }
            else if (op.Tipo == "Traslado")
            {
                // Quitar de origen
                var invOrigen = await _db.Inventarios
                    .FirstOrDefaultAsync(i =>
                        i.ProductoId == op.ProductoId &&
                        i.PosicionId == op.PosicionOrigenId);

                if (invOrigen != null)
                {
                    invOrigen.CantidadDisponible = Math.Max(0,
                        invOrigen.CantidadDisponible - op.Cantidad);
                    invOrigen.UltimaActualizacion = DateTime.UtcNow;
                }

                // Agregar a destino
                var invDestino = await _db.Inventarios
                    .FirstOrDefaultAsync(i =>
                        i.ProductoId == op.ProductoId &&
                        i.PosicionId == op.PosicionDestinoId)
                    ?? new Inventario
                    {
                        ProductoId = op.ProductoId,
                        PosicionId = op.PosicionDestinoId!.Value
                    };

                if (invDestino.Id == 0)
                    _db.Inventarios.Add(invDestino);

                invDestino.CantidadDisponible += op.Cantidad;
                invDestino.UltimaActualizacion = DateTime.UtcNow;

                _db.Movimientos.Add(new Movimiento
                {
                    Tipo = "Transferencia",
                    ProductoId = op.ProductoId,
                    PosicionOrigenId = op.PosicionOrigenId,
                    PosicionDestinoId = op.PosicionDestinoId,
                    Cantidad = op.Cantidad,
                    Motivo = $"Op.Traslado #{op.Id}",
                    UsuarioId = userId,
                    NombreUsuario = userName,
                    FechaHora = DateTime.UtcNow
                });
            }

            await Auditar("UPDATE", "Operaciones",
                $"Op #{id} completada", userId, userName);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return (true, $"Operación #{id} completada. Movimiento registrado.");
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
        var op = await _db.Operaciones.FindAsync(id);
        if (op == null) return (false, "No encontrada.");
        if (op.Estado == "Completada")
            return (false, "No se puede cancelar una operación completada.");

        op.Eliminado = true;
        await _db.SaveChangesAsync();

        await Auditar("DELETE", "Operaciones",
            $"Op #{id} cancelada", userId, userName);

        return (true, "Operación cancelada.");
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

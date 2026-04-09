using InventarioQR.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventarioQR.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Zona> Zonas => Set<Zona>();
    public DbSet<Estanteria> Estanterias => Set<Estanteria>();
    public DbSet<Nivel> Niveles => Set<Nivel>();
    public DbSet<Posicion> Posiciones => Set<Posicion>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Variante> Variantes => Set<Variante>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<CodigoQR> CodigosQR => Set<CodigoQR>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<Operacion> Operaciones => Set<Operacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Índices para rendimiento
        builder.Entity<Producto>()
            .HasIndex(p => p.SKU).IsUnique();

        builder.Entity<Inventario>()
            .HasIndex(i => new { i.ProductoId, i.PosicionId }).IsUnique();

        builder.Entity<Bodega>()
            .HasIndex(b => b.CodigoInterno).IsUnique();

        // Filtros globales soft delete
        builder.Entity<Producto>().HasQueryFilter(p => !p.Eliminado);
        builder.Entity<Bodega>().HasQueryFilter(b => !b.Eliminado);
        builder.Entity<Zona>().HasQueryFilter(z => !z.Eliminado);
        builder.Entity<Estanteria>().HasQueryFilter(e => !e.Eliminado);
        builder.Entity<Nivel>().HasQueryFilter(n => !n.Eliminado);
        builder.Entity<Posicion>().HasQueryFilter(p => !p.Eliminado);
        builder.Entity<Inventario>().HasQueryFilter(i => !i.Eliminado);
        builder.Entity<Variante>().HasQueryFilter(v => !v.Eliminado);
        builder.Entity<Operacion>().HasQueryFilter(o => !o.Eliminado);
    }
}
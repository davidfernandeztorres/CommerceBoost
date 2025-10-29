using CommerceBoost.Models;
using Microsoft.EntityFrameworkCore;

namespace CommerceBoost.Data;

public class ContextoComercio : DbContext
{
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<LineaVenta> LineasVenta { get; set; }
    public DbSet<Cliente> Clientes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=commerce;Username=postgres;Password=password");
    }
}
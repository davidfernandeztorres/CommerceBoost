using CommerceBoost.Data;
using CommerceBoost.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CommerceBoost.Services;

public class ServicioComercio
{
    private readonly ContextoComercio _contexto;

    public ServicioComercio(ContextoComercio contexto)
    {
        _contexto = contexto;
    }

    public async Task<IEnumerable<Producto>> ObtenerProductosAsync()
    {
        return await _contexto.Productos.ToListAsync();
    }

    public async Task AgregarProductoAsync(string nombre, string codigo, decimal precio, int stock)
    {
        var producto = new Producto { Nombre = nombre, Codigo = codigo, Precio = precio, Stock = stock };
        _contexto.Productos.Add(producto);
        await _contexto.SaveChangesAsync();
    }

    public async Task<Producto?> ObtenerProductoPorCodigoAsync(string codigo)
    {
        return await _contexto.Productos.FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    public async Task ActualizarProductoAsync(int id, string nombre, string codigo, decimal precio, int stock)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.Nombre = nombre;
            producto.Codigo = codigo;
            producto.Precio = precio;
            producto.Stock = stock;
            await _contexto.SaveChangesAsync();
        }
    }

    public async Task EliminarProductoAsync(int id)
    {
        var producto = await _contexto.Productos.FindAsync(id);
        if (producto != null)
        {
            _contexto.Productos.Remove(producto);
            await _contexto.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Venta>> ObtenerVentasAsync()
    {
        return await _contexto.Ventas.Include(v => v.Lineas).ThenInclude(l => l.Producto).ToListAsync();
    }

    public async Task AgregarVentaAsync(List<LineaVenta> lineas)
    {
        var venta = new Venta { Fecha = DateTime.Now, Lineas = lineas };
        venta.Total = lineas.Sum(l => l.Cantidad * l.Precio);
        _contexto.Ventas.Add(venta);
        await _contexto.SaveChangesAsync();
    }

    public async Task<IEnumerable<Cliente>> ObtenerClientesAsync()
    {
        return await _contexto.Clientes.ToListAsync();
    }

    public async Task AgregarClienteAsync(Cliente cliente)
    {
        _contexto.Clientes.Add(cliente);
        await _contexto.SaveChangesAsync();
    }
}
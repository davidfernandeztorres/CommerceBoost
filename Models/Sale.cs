using System.Collections.Generic;

namespace CommerceBoost.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public List<LineaVenta> Lineas { get; set; } = new();
    public decimal Total { get; set; }
}
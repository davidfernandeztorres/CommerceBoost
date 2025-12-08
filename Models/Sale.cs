using System;
using System.Collections.Generic;   

namespace CommerceBoost.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public string MetodoPago { get; set; } = "Efectivo";
        public bool ZClosed { get; set; } = false;
        public List<SaleItem> Items { get; set; } = new();
    }
}

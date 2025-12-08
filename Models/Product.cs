namespace CommerceBoost.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}

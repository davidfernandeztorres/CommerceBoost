namespace CommerceBoost.Models
{
    public class SaleItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    Recalculate();
                }
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value)
                {
                    _unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(DiscountedPrice));
                    Recalculate();
                }
            }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                if (_discount != value)
                {
                    _discount = value;
                    OnPropertyChanged(nameof(Discount));
                    OnPropertyChanged(nameof(DiscountedPrice));
                    Recalculate();
                }
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public decimal DiscountedPrice => UnitPrice - Discount;

        private void Recalculate()
        {
            TotalPrice = Quantity * (UnitPrice - Discount);
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}

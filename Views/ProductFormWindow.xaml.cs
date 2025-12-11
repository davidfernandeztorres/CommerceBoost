using System;
using System.Windows;
using CommerceBoost.Models;

namespace CommerceBoost.Views
{
    public partial class ProductFormWindow : Window
    {
        public Product ResultProduct { get; private set; }

        public ProductFormWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreBox.Text) || 
                string.IsNullOrWhiteSpace(PrecioBox.Text) || 
                string.IsNullOrWhiteSpace(StockBox.Text))
            {
                MessageBox.Show("Por favor, rellena todos los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PrecioBox.Text, out decimal precio))
            {
                MessageBox.Show("El precio debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockBox.Text, out int stock))
            {
                MessageBox.Show("El stock debe ser un número entero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var random = new Random();
            ResultProduct = new Product
            {
                Codigo = random.Next(100000, 999999).ToString(), // Generamos el código automáticamente
                Nombre = NombreBox.Text,
                Precio = precio,
                Stock = stock
            };

            DialogResult = true;
            Close();
        }
    }
}

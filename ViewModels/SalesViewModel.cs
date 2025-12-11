using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommerceBoost.Models;
using CommerceBoost.Services;
using CommerceBoost.Helpers;
using System.Text.Json;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace CommerceBoost.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private readonly CommerceService _service;
        private Sale _currentSale;
        private SaleItem? _selectedItem;
        public SaleItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public SalesViewModel(CommerceService service)
        {
            _service = service;
            _service.EnsureSchema(); // Arreglamos el esquema de la BD si hace falta
            // Empezamos con una venta vacía para la sesión del TPV
            _currentSale = new Sale { Fecha = DateTime.UtcNow, Total = 0, Items = new List<SaleItem>() };
            
            // Cargamos los productos para poder buscarlos
            var productsList = _service.GetProducts();
            Products = new ObservableCollection<Product>(productsList);
            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            UpdateSaleList();

            // Configuramos los comandos
            InitializeCommands();
        }

        public ObservableCollection<SaleItem> CurrentSaleItems { get; private set; } = new ObservableCollection<SaleItem>();
        public ObservableCollection<Product> Products { get; }
        public ICollectionView ProductsView { get; }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                ProductsView.Refresh();
            }
        }

        private string _editingProperty = "Quantity";
        public string EditingProperty
        {
            get => _editingProperty;
            set
            {
                _editingProperty = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingProperty)));
            }
        }

        private string _scanQuantity = "1";
        public string ScanQuantity
        {
            get => _scanQuantity;
            set
            {
                _scanQuantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanQuantity)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<List<SaleItem>>? OnSaleUpdated;

        // Comandos para las acciones del TPV
        public ICommand? NavigateInventoryCommand { get; private set; }
        public ICommand? NavigateSalesCommand { get; private set; }
        public ICommand? NavigateSettingsCommand { get; private set; }
        public ICommand? NumPadCommand { get; private set; }
        public ICommand? CobrarDirectoCommand { get; private set; }
        public ICommand? CobrarSinTicketCommand { get; private set; }
        public ICommand? CobrarTarjetaCommand { get; private set; }
        public ICommand? ReimprimirTicketCommand { get; private set; }
        public ICommand? AplicarDescuentoCommand { get; private set; }
        public ICommand? BorrarArticuloCommand { get; private set; }
        public ICommand? AgregarArticuloCommand { get; private set; }
        public ICommand? MasOpcionesCommand { get; private set; }
        public ICommand? AbrirCajonCommand { get; private set; }
        public ICommand? ScanProductCommand { get; private set; }
        public ICommand? ScanNewProductCommand { get; private set; }
        public ICommand? AddManualProductCommand { get; private set; }
        public ICommand? DeleteProductCommand { get; private set; }
        public ICommand? EditProductCommand { get; private set; }
        public ICommand? ImprimirCierreZCommand { get; private set; }

        private void InitializeCommands()
        {
            NavigateInventoryCommand = new RelayCommand(_ => AbrirInventario());
            NavigateSalesCommand = new RelayCommand(_ => AbrirVentas());
            NavigateSettingsCommand = new RelayCommand(_ => AbrirAjustes());
            NumPadCommand = new RelayCommand(param => UsarTeclado(param?.ToString() ?? ""));
            CobrarDirectoCommand = new RelayCommand(_ => CobrarDirecto());
            CobrarSinTicketCommand = new RelayCommand(_ => CobrarSinTicket());
            CobrarTarjetaCommand = new RelayCommand(_ => CobrarTarjeta());
            ReimprimirTicketCommand = new RelayCommand(_ => ReimprimirTicket());
            AplicarDescuentoCommand = new RelayCommand(_ => AplicarDescuento());
            BorrarArticuloCommand = new RelayCommand(_ => BorrarArticulo());
            AgregarArticuloCommand = new RelayCommand(_ => AgregarArticulo(""));
            MasOpcionesCommand = new RelayCommand(_ => MasOpciones());
            AbrirCajonCommand = new RelayCommand(_ => AbrirCajon());
            ScanProductCommand = new RelayCommand(_ => ScanRandomProduct());
            ScanNewProductCommand = new RelayCommand(_ => ScanNewInventoryProduct());
            AddManualProductCommand = new RelayCommand(_ => AddManualProduct());
            DeleteProductCommand = new RelayCommand(param => DeleteProduct(param as Product));
            EditProductCommand = new RelayCommand(param => EditProduct(param as Product));
            ImprimirCierreZCommand = new RelayCommand(_ => ImprimirCierreZ());
        }

        private void AddManualProduct()
        {
            var form = new Views.ProductFormWindow();
            form.Owner = Application.Current.MainWindow; // Set owner to main window (or find active window)
            if (form.ShowDialog() == true)
            {
                var newProduct = form.ResultProduct;
                try
                {
                    _service.AddProduct(newProduct);
                    Products.Add(newProduct);
                    MessageBox.Show($"Producto añadido:\n{newProduct.Nombre}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Acciones del inventario
        private bool FilterProducts(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            if (item is Product product)
            {
                return (product.Nombre?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                       (product.Codigo?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            return false;
        }

        private void DeleteProduct(Product? product)
        {
            if (product == null) return;
            if (MessageBox.Show($"¿Estás seguro de eliminar '{product.Nombre}'?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // Quitamos el producto de la lista para actualizar la interfaz
                Products.Remove(product);
            }
        }

        private void EditProduct(Product? product)
        {
            if (product == null) return;
            MessageBox.Show($"Editar producto: {product.Nombre}\n(Funcionalidad completa pendiente de implementación)", "Editar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ScanNewInventoryProduct()
        {
            // Simulamos escanear un producto nuevo
            var random = new Random();
            var newProduct = new Product
            {
                // El ID lo genera la BD automáticamente
                Codigo = random.Next(100000, 999999).ToString(),
                Nombre = $"Producto Nuevo {random.Next(1, 100)}",
                Precio = (decimal)(random.Next(100, 5000) / 100.0),
                Stock = random.Next(10, 100)
            };

            // Lo guardamos en la base de datos
            try 
            {
                _service.AddProduct(newProduct);
                
                // Lo añadimos a la lista de la interfaz
                Products.Add(newProduct);
                
                MessageBox.Show($"Nuevo producto añadido al inventario y BD:\n\nCódigo: {newProduct.Codigo}\nNombre: {newProduct.Nombre}\nPrecio: {newProduct.Precio:C2}\nStock: {newProduct.Stock}", "Inventario Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en BD: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Simulación del lector de códigos de barras
        private void ScanRandomProduct()
        {
            // Cogemos todos los productos de la BD
            var allProducts = _service.GetProducts();
            if (allProducts == null || allProducts.Count == 0)
            {
                MessageBox.Show("No hay productos en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Elegimos uno al azar
            var random = new Random();
            var randomProduct = allProducts[random.Next(allProducts.Count)];

            // Parseamos la cantidad
            if (!int.TryParse(ScanQuantity, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor que 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Añadimos el producto a la venta
            var newItem = new SaleItem
            {
                ProductId = randomProduct.Id,
                Product = randomProduct,
                Quantity = quantity,
                UnitPrice = randomProduct.Precio,
                Discount = 0,
                TotalPrice = randomProduct.Precio * quantity
            };

            _currentSale.Items.Add(newItem);
            _selectedItem = newItem; // Lo seleccionamos automáticamente
            
            // Actualizamos la interfaz
            Application.Current.Dispatcher.Invoke(() => 
            {
                UpdateSaleList();
            });

            // Reseteamos la cantidad a 1 para el siguiente escaneo
            ScanQuantity = "1";
        }

        // Navegación entre vistas
        // Usamos eventos para que MainWindow cambie de vista
        public event Action<string>? RequestNavigation;

        public void AbrirInventario() => RequestNavigation?.Invoke("Inventory");
        public void AbrirVentas() => RequestNavigation?.Invoke("Sales");
        public void AbrirAjustes() => RequestNavigation?.Invoke("Settings");

        // Acciones del TPV

        public void AgregarArticulo(string codigo)
        {
            // Buscamos el producto por código y lo añadimos a la venta
            var product = _service.GetProductByCode(codigo);
            if (product != null)
            {
                var newItem = new SaleItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Precio,
                    TotalPrice = product.Precio
                };
                _currentSale.Items.Add(newItem);
                _selectedItem = newItem; // Lo seleccionamos automáticamente
                UpdateSaleList();
            }
        }

        public void BorrarArticulo(string codigo)
        {
            // En una app real, 'codigo' podría ser el ID del artículo
            // Por ahora, simplemente quitamos el último
            var itemToRemove = _currentSale.Items.LastOrDefault(); // Simplificación
            if (itemToRemove != null)
            {
                _currentSale.Items.Remove(itemToRemove);
                UpdateSaleList();
            }
        }

        public void SeleccionarArticulo(string codigo)
        {
            // Buscamos el artículo por código de producto
            _selectedItem = _currentSale.Items.FirstOrDefault(i => i.Product.Codigo == codigo);
        }

        public void UsarTeclado(string input)
        {
            if (input == "⌫")
            {
                if (_selectedItem != null)
                {
                   if (EditingProperty == "UnitPrice")
                   {
                       string sPrice = _selectedItem.UnitPrice.ToString("0.00");
                       // Mejor tratar el precio como texto para poder editarlo
                       // Pero como es decimal, lo más simple es resetearlo
                       _selectedItem.UnitPrice = 0;
                   }
                   else // Quantity
                   {
                       string sQty = _selectedItem.Quantity.ToString();
                       if (sQty.Length > 1) 
                           _selectedItem.Quantity = int.Parse(sQty.Substring(0, sQty.Length - 1));
                       else 
                           _selectedItem.Quantity = 0;
                       
                       ScanQuantity = _selectedItem.Quantity.ToString();
                   }
                }
                else
                {
                    if (ScanQuantity.Length > 1) ScanQuantity = ScanQuantity.Substring(0, ScanQuantity.Length - 1);
                    else ScanQuantity = "0";
                }
                return;
            }

            if (_selectedItem != null)
            {
                if (EditingProperty == "UnitPrice")
                {
                    string sPrice = _selectedItem.UnitPrice.ToString();
                    if (sPrice == "0") sPrice = "";
                    
                    if (input == ".")
                    {
                        if (!sPrice.Contains(".")) sPrice += ".";
                    }
                    else
                    {
                        sPrice += input;
                    }

                    if (decimal.TryParse(sPrice, out decimal newPrice))
                    {
                        _selectedItem.UnitPrice = newPrice;
                    }
                }
                else // Quantity
                {
                    string sQty = _selectedItem.Quantity.ToString();
                    if (sQty == "0" || sQty == "1") sQty = ""; 
                    
                    if (input != ".")
                    {
                        sQty += input;
                        if (int.TryParse(sQty, out int newQty))
                        {
                            _selectedItem.Quantity = newQty;
                            ScanQuantity = sQty; 
                        }
                    }
                }
            }
            else
            {
                // Modificamos el buffer de escaneo
                if ((ScanQuantity == "1" || ScanQuantity == "0") && input != ".")
                {
                    ScanQuantity = input;
                }
                else
                {
                    ScanQuantity += input;
                }
            }
        }

        private void RecalculateItem(SaleItem item)
        {
            item.TotalPrice = item.Quantity * (item.UnitPrice - item.Discount);
        }

        // Acciones del inventario
        public List<Product> GetInventory() => _service.GetProducts();
        
        public void AddProduct(Product p)
        {
            _service.AddProduct(p);
        }

        public void DeleteProduct(int id)
        {
             _service.DeleteProduct(id);
        }

        private void UpdateSaleList()
        {
            CurrentSaleItems.Clear();
            foreach (var item in _currentSale.Items)
            {
                CurrentSaleItems.Add(item);
            }
            OnSaleUpdated?.Invoke(_currentSale.Items.ToList());
        }

        // Acciones del pie de página (botones de cobro)
        public void CobrarDirecto() => FinalizarVenta("Efectivo");
        public void CobrarSinTicket() => FinalizarVenta("Sin Ticket");
        public void CobrarTarjeta() => FinalizarVenta("Tarjeta");

        private void FinalizarVenta(string metodoPago)
        {
            if (!_currentSale.Items.Any())
            {
                MessageBox.Show("No hay artículos en la venta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Nos aseguramos de que existe el cliente (para evitar errores de FK)
                var customers = _service.GetCustomers();
                if (!customers.Any(c => c.Id == 1))
                {
                    _service.AddCustomer(new Customer { Id = 1, Name = "Cliente General", Email = "general@tienda.com" });
                }

                // 2. Preparamos la venta
                _currentSale.Fecha = DateTime.UtcNow;
                _currentSale.CustomerId = 1;
                _currentSale.Total = _currentSale.Items.Sum(item => item.TotalPrice);
                _currentSale.MetodoPago = metodoPago;
                
                // 3. Guardamos en la BD
                _service.AddSale(_currentSale);

                // 4. Actualizamos el stock
                foreach (var item in _currentSale.Items)
                {
                    _service.UpdateStock(item.ProductId, item.Quantity);
                }

                // 5. Generamos el ticket en JSON
                GenerateTicket(_currentSale, metodoPago);

                // 6. Todo OK, reseteamos la venta
                var ticketId = _currentSale.Id;
                // Venta completada en silencio - ticket impreso

                _currentSale = new Sale { Fecha = DateTime.UtcNow, Total = 0, Items = new List<SaleItem>() };
                _selectedItem = null;
                UpdateSaleList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al finalizar la venta:\n{ex.Message}\n\n{ex.InnerException?.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateTicket(Sale sale, string metodoPago)
        {
            var sb = new System.Text.StringBuilder();
            
            // Cabecera
            sb.AppendLine("========================================");
            sb.AppendLine("     O ALMACEN DO CARNAVAL");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine($"Ticket #: {sale.Id:D6}");
            sb.AppendLine($"Fecha: {sale.Fecha.ToLocalTime():dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Método de Pago: {metodoPago.ToUpper()}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();
            
            // Cabecera de artículos
            sb.AppendLine("CANT  PRODUCTO              P.UNIT  TOTAL");
            sb.AppendLine("----------------------------------------");
            
            // Artículos
            foreach (var item in sale.Items)
            {
                string nombre = item.Product.Nombre.Length > 20 
                    ? item.Product.Nombre.Substring(0, 20) 
                    : item.Product.Nombre.PadRight(20);
                
                decimal precioFinal = item.UnitPrice - item.Discount;
                
                sb.AppendLine($"{item.Quantity,4}  {nombre}  {precioFinal,6:F2}  {item.TotalPrice,6:F2}");
                
                // Mostramos el descuento si se aplicó
                if (item.Discount > 0)
                {
                    sb.AppendLine($"      (Descuento: -{item.Discount:F2})");
                }
            }
            
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();
            
            // Totales
            decimal subtotal = sale.Items.Sum(i => i.Quantity * i.UnitPrice);
            decimal totalDescuentos = sale.Items.Sum(i => i.Quantity * i.Discount);
            
            if (totalDescuentos > 0)
            {
                sb.AppendLine($"Subtotal:                    {subtotal,8:F2}€");
                sb.AppendLine($"Descuentos:                 -{totalDescuentos,8:F2}€");
                sb.AppendLine("----------------------------------------");
            }
            
            sb.AppendLine($"TOTAL:                       {sale.Total,8:F2}€");
            sb.AppendLine();
            
            // Mensaje según el método de pago
            if (metodoPago == "Efectivo")
            {
                sb.AppendLine("*** PAGADO EN EFECTIVO ***");
            }
            else if (metodoPago == "Tarjeta")
            {
                sb.AppendLine("*** PAGADO CON TARJETA ***");
            }
            
            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("   ¡Gracias por su compra!");
            sb.AppendLine("   Vuelva pronto");
            sb.AppendLine("========================================");
            
            // Guardamos en archivo
            string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
            System.IO.Directory.CreateDirectory(folder);
            string filename = $"Ticket_{sale.Id:D6}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            System.IO.File.WriteAllText(System.IO.Path.Combine(folder, filename), sb.ToString());
        }

        public void ReimprimirTicket() 
        {
            // Simulamos reimprimir el último ticket
            var lastSale = _service.GetSales().OrderByDescending(s => s.Fecha).FirstOrDefault();
            if (lastSale != null)
            {
                // Reimprimimos en silencio - en producción iría a la impresora
                GenerateTicket(lastSale, "Reimpresión");
            }
        }

        public void AbrirCajon()
        {
            // Simulamos abrir el cajón de efectivo
            // En producción, esto enviaría una señal al cajón para abrirlo
            System.Diagnostics.Debug.WriteLine("Cash drawer opened");
        }
        
        public void AplicarDescuento() 
        {
            // Verificamos que hay artículos en la venta
            if (!_currentSale.Items.Any())
            {
                MessageBox.Show("No hay artículos en la venta para aplicar descuento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Determinamos el título y mensaje según si hay producto seleccionado
                string title = _selectedItem != null ? "Descuento Artículo" : "Descuento Global";
                string message = _selectedItem != null 
                    ? "Introduce porcentaje de descuento para el artículo (%):" 
                    : "Introduce porcentaje de descuento para TODOS los artículos (%):";

                var inputWin = new Views.InputWindow(title, message);
                inputWin.Owner = Application.Current.MainWindow;
                
                if (inputWin.ShowDialog() == true)
                {
                    if (decimal.TryParse(inputWin.ResultText, out decimal discountPercent))
                    {
                        // Validamos que el porcentaje esté entre 0 y 100
                        if (discountPercent < 0 || discountPercent > 100)
                        {
                            MessageBox.Show("El porcentaje debe estar entre 0 y 100.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (_selectedItem != null)
                        {
                            // Descuento individual: solo al producto seleccionado
                            decimal discountAmount = _selectedItem.UnitPrice * (discountPercent / 100m);
                            _selectedItem.Discount = discountAmount;
                        }
                        else
                        {
                            // Descuento global: a todos los productos del ticket
                            foreach (var item in _currentSale.Items)
                            {
                                decimal discountAmount = item.UnitPrice * (discountPercent / 100m);
                                item.Discount = discountAmount;
                            }
                        }
                        
                        // No hace falta llamar a UpdateSaleList() - SaleItem implementa INotifyPropertyChanged
                    }
                    else
                    {
                        MessageBox.Show("Porcentaje inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar descuento:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BorrarArticulo()
        {
            if (_selectedItem != null)
            {
                _currentSale.Items.Remove(_selectedItem);
                _selectedItem = null;
                UpdateSaleList();
            }
            else if (_currentSale.Items.Any())
            {
                // Si no hay nada seleccionado, quitamos el último
                _currentSale.Items.RemoveAt(_currentSale.Items.Count - 1);
                UpdateSaleList();
            }
        }

        public void AgregarArticulo()
        {
            var inputWin = new Views.InputWindow("Añadir por Código", "Introduce código o nombre del producto:");
            inputWin.Owner = Application.Current.MainWindow;
            if (inputWin.ShowDialog() == true)
            {
                string code = inputWin.ResultText;
                var product = Products.FirstOrDefault(p => p.Codigo == code || p.Nombre.Contains(code, StringComparison.OrdinalIgnoreCase));
                
                if (product != null)
                {
                    int qty = int.Parse(ScanQuantity); // Usamos la cantidad actual del teclado
                    var newItem = new SaleItem
                    {
                        ProductId = product.Id,
                        Product = product,
                        Quantity = qty,
                        UnitPrice = product.Precio,
                        Discount = 0,
                        TotalPrice = product.Precio * qty
                    };
                    _currentSale.Items.Add(newItem);
                    _selectedItem = newItem;
                    UpdateSaleList();
                    ScanQuantity = "1"; // Reseteamos
                }
                // Producto no encontrado - lo ignoramos para ir rápido
            }
        }

        public void MasOpciones() => MessageBox.Show("Más opciones...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        public void ImprimirCierreZ()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var sales = _service.GetSales().Where(s => s.Fecha.Date == today && !s.ZClosed).ToList();
                
                if (!sales.Any())
                {
                    MessageBox.Show("No hay ventas pendientes de cierre.", "Cierre Z", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Calculamos totales por método de pago
                var efectivoSales = sales.Where(s => s.MetodoPago == "Efectivo").ToList();
                var tarjetaSales = sales.Where(s => s.MetodoPago == "Tarjeta").ToList();
                var otrosSales = sales.Where(s => s.MetodoPago != "Efectivo" && s.MetodoPago != "Tarjeta").ToList();

                decimal totalEfectivo = efectivoSales.Sum(s => s.Total);
                decimal totalTarjeta = tarjetaSales.Sum(s => s.Total);
                decimal totalOtros = otrosSales.Sum(s => s.Total);
                decimal totalGeneral = sales.Sum(s => s.Total);

                // Generamos el ticket del cierre Z
                var sb = new System.Text.StringBuilder();
                
                sb.AppendLine("========================================");
                sb.AppendLine("        O ALMACEN DO CARNAVAL");
                sb.AppendLine("========================================");
                sb.AppendLine("          CIERRE Z - ARQUEO");
                sb.AppendLine("========================================");
                sb.AppendLine();
                sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Período: {today:dd/MM/yyyy}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine();
                
                // Ventas por método de pago
                sb.AppendLine("DETALLE DE VENTAS:");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine();
                
                if (efectivoSales.Any())
                {
                    sb.AppendLine($"EFECTIVO ({efectivoSales.Count} tickets):");
                    sb.AppendLine();
                    foreach (var sale in efectivoSales.OrderBy(s => s.Fecha))
                    {
                        sb.AppendLine($"  #{sale.Id:D6}  {sale.Fecha.ToLocalTime():HH:mm}  {sale.Total,8:F2}€");
                    }
                    sb.AppendLine($"                    --------");
                    sb.AppendLine($"  Subtotal:         {totalEfectivo,8:F2}€");
                    sb.AppendLine();
                }
                
                if (tarjetaSales.Any())
                {
                    sb.AppendLine($"TARJETA ({tarjetaSales.Count} tickets):");
                    sb.AppendLine();
                    foreach (var sale in tarjetaSales.OrderBy(s => s.Fecha))
                    {
                        sb.AppendLine($"  #{sale.Id:D6}  {sale.Fecha.ToLocalTime():HH:mm}  {sale.Total,8:F2}€");
                    }
                    sb.AppendLine($"                    --------");
                    sb.AppendLine($"  Subtotal:         {totalTarjeta,8:F2}€");
                    sb.AppendLine();
                }
                
                if (otrosSales.Any())
                {
                    sb.AppendLine($"OTROS ({otrosSales.Count} tickets):");
                    sb.AppendLine();
                    foreach (var sale in otrosSales.OrderBy(s => s.Fecha))
                    {
                        sb.AppendLine($"  #{sale.Id:D6}  {sale.Fecha.ToLocalTime():HH:mm}  {sale.Total,8:F2}€");
                    }
                    sb.AppendLine($"                    --------");
                    sb.AppendLine($"  Subtotal:         {totalOtros,8:F2}€");
                    sb.AppendLine();
                }
                
                sb.AppendLine("========================================");
                sb.AppendLine($"TOTAL TICKETS:           {sales.Count,4}");
                sb.AppendLine($"TOTAL GENERAL:      {totalGeneral,10:F2}€");
                sb.AppendLine("========================================");
                sb.AppendLine();
                sb.AppendLine("   Caja cerrada correctamente");
                sb.AppendLine("========================================");
                
                // Guardamos el informe Z
                string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
                System.IO.Directory.CreateDirectory(folder);
                string filename = $"CierreZ_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                System.IO.File.WriteAllText(System.IO.Path.Combine(folder, filename), sb.ToString());
                
                // Marcamos las ventas como cerradas en Z
                foreach (var sale in sales)
                {
                    sale.ZClosed = true;
                }
                _service.UpdateSales(sales);
                
                // Actualizamos las estadísticas para que muestren cero
                RefreshSalesStats();
                
                MessageBox.Show($"Cierre Z completado.\n\nTotal: {totalGeneral:C}\nTickets: {sales.Count}\n\nTicket guardado en carpeta Tickets", "Cierre Z", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar Cierre Z:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Informes y estadísticas
        // Estadísticas de ventas e historial
        private decimal _dailyTotal;
        public decimal DailyTotal
        {
            get => _dailyTotal;
            set
            {
                _dailyTotal = value;
                OnPropertyChanged(nameof(DailyTotal));
            }
        }

        private int _dailySalesCount;
        public int DailySalesCount
        {
            get => _dailySalesCount;
            set
            {
                _dailySalesCount = value;
                OnPropertyChanged(nameof(DailySalesCount));
            }
        }

        private List<Sale> _salesHistory = new();
        public List<Sale> SalesHistory
        {
            get => _salesHistory;
            set
            {
                _salesHistory = value;
                OnPropertyChanged(nameof(SalesHistory));
            }
        }

        public void RefreshSalesStats()
        {
            var today = DateTime.UtcNow.Date;
            var sales = _service.GetSales();
            
            // Solo contamos las ventas que no se han cerrado en Z
            var openSales = sales.Where(s => !s.ZClosed).ToList();
            
            DailyTotal = openSales.Where(s => s.Fecha.Date == today).Sum(s => s.Total);
            DailySalesCount = openSales.Count(s => s.Fecha.Date == today);
            SalesHistory = sales.OrderByDescending(s => s.Fecha).ToList();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

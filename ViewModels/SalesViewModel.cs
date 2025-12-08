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
            _service.EnsureSchema(); // Fix DB Schema
            // Initialize with a new empty sale for the TPV session
            _currentSale = new Sale { Fecha = DateTime.UtcNow, Total = 0, Items = new List<SaleItem>() };
            
            // Load initial data if needed (e.g. products for lookup)
            var productsList = _service.GetProducts();
            Products = new ObservableCollection<Product>(productsList);
            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;

            UpdateSaleList();

            // Initialize Commands
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

        // Commands
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

        // --- Inventory Actions ---
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
                // _service.DeleteProduct(product); // Assuming service has this method, if not we need to add it. 
                // For now just remove from list to update UI
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
            // Simulate scanning a new product
            var random = new Random();
            var newProduct = new Product
            {
                // Id = Products.Count + 1, // Let DB handle ID if possible, or generate unique
                Codigo = random.Next(100000, 999999).ToString(),
                Nombre = $"Producto Nuevo {random.Next(1, 100)}",
                Precio = (decimal)(random.Next(100, 5000) / 100.0),
                Stock = random.Next(10, 100)
            };

            // Add to Database
            try 
            {
                _service.AddProduct(newProduct);
                
                // Add to UI List
                Products.Add(newProduct);
                
                MessageBox.Show($"Nuevo producto añadido al inventario y BD:\n\nCódigo: {newProduct.Codigo}\nNombre: {newProduct.Nombre}\nPrecio: {newProduct.Precio:C2}\nStock: {newProduct.Stock}", "Inventario Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en BD: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Barcode Scanner Simulation ---
        private void ScanRandomProduct()
        {
            // Get all products from database
            var allProducts = _service.GetProducts();
            if (allProducts == null || allProducts.Count == 0)
            {
                MessageBox.Show("No hay productos en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Select random product
            var random = new Random();
            var randomProduct = allProducts[random.Next(allProducts.Count)];

            // Parse quantity
            if (!int.TryParse(ScanQuantity, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor que 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add product to sale
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
            _selectedItem = newItem; // Auto-select the scanned item
            
            // Force UI update
            Application.Current.Dispatcher.Invoke(() => 
            {
                UpdateSaleList();
            });

            // Reset quantity to 1 for next scan
            ScanQuantity = "1";
        }

        // --- Navigation / Views ---
        // Simple navigation via events or properties. For now, we'll use an event to tell MainWindow to switch views.
        public event Action<string>? RequestNavigation;

        public void AbrirInventario() => RequestNavigation?.Invoke("Inventory");
        public void AbrirVentas() => RequestNavigation?.Invoke("Sales");
        public void AbrirAjustes() => RequestNavigation?.Invoke("Settings");

        // --- TPV Actions ---

        public void AgregarArticulo(string codigo)
        {
            // Logic to find product by code and add to current sale
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
                _selectedItem = newItem; // Auto-select last added
                UpdateSaleList();
            }
        }

        public void BorrarArticulo(string codigo)
        {
            // In a real app, 'codigo' might be the SaleItemId or ProductId
            // For this demo, let's remove the selected item or find by code
            var itemToRemove = _currentSale.Items.LastOrDefault(); // Simplification
            if (itemToRemove != null)
            {
                _currentSale.Items.Remove(itemToRemove);
                UpdateSaleList();
            }
        }

        public void SeleccionarArticulo(string codigo)
        {
            // Find item by Product Code
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
                       string sPrice = _selectedItem.UnitPrice.ToString("0.00"); // Format to keep decimals? Or just raw string?
                       // Better approach: treat as string input logic
                       // But UnitPrice is decimal. Let's assume user types like a calculator.
                       // Actually, simpler: just manipulate the value as string if possible, or reset.
                       // Let's try to just remove last char from string representation, but that's tricky with formatting.
                       // Simpler: Reset to 0 if backspace on full value, or implement string buffer.
                       // For now, let's just set to 0 if backspace.
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
                // Modify scan buffer
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

        // --- Inventory Actions ---
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

        // --- Footer Actions ---
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
                // 1. Ensure Customer exists (Prevent FK Crash)
                var customers = _service.GetCustomers();
                if (!customers.Any(c => c.Id == 1))
                {
                    _service.AddCustomer(new Customer { Id = 1, Name = "Cliente General", Email = "general@tienda.com" });
                }

                // 2. Prepare Sale
                _currentSale.Fecha = DateTime.UtcNow;
                _currentSale.CustomerId = 1;
                _currentSale.Total = _currentSale.Items.Sum(item => item.TotalPrice);
                _currentSale.MetodoPago = metodoPago;
                
                // 3. Save to DB
                _service.AddSale(_currentSale);

                // 4. Update Stock
                foreach (var item in _currentSale.Items)
                {
                    _service.UpdateStock(item.ProductId, item.Quantity);
                }

                // 5. Generate JSON Ticket
                GenerateTicket(_currentSale, metodoPago);

                // 6. Success & Reset
                var ticketId = _currentSale.Id;
                // Sale completed silently - ticket printed

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
            
            // Header
            sb.AppendLine("========================================");
            sb.AppendLine("     O ALMACEN DO CARNAVAL");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine($"Ticket #: {sale.Id:D6}");
            sb.AppendLine($"Fecha: {sale.Fecha.ToLocalTime():dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Método de Pago: {metodoPago.ToUpper()}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();
            
            // Items header
            sb.AppendLine("CANT  PRODUCTO              P.UNIT  TOTAL");
            sb.AppendLine("----------------------------------------");
            
            // Items
            foreach (var item in sale.Items)
            {
                string nombre = item.Product.Nombre.Length > 20 
                    ? item.Product.Nombre.Substring(0, 20) 
                    : item.Product.Nombre.PadRight(20);
                
                decimal precioFinal = item.UnitPrice - item.Discount;
                
                sb.AppendLine($"{item.Quantity,4}  {nombre}  {precioFinal,6:F2}  {item.TotalPrice,6:F2}");
                
                // Show discount if applied
                if (item.Discount > 0)
                {
                    sb.AppendLine($"      (Descuento: -{item.Discount:F2})");
                }
            }
            
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();
            
            // Totals
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
            
            // Payment method specific message
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
            
            // Save to file
            string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
            System.IO.Directory.CreateDirectory(folder);
            string filename = $"Ticket_{sale.Id:D6}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            System.IO.File.WriteAllText(System.IO.Path.Combine(folder, filename), sb.ToString());
        }

        public void ReimprimirTicket() 
        {
            // Simulate reprinting last ticket
            var lastSale = _service.GetSales().OrderByDescending(s => s.Fecha).FirstOrDefault();
            if (lastSale != null)
            {
                // Reprint silently - would send to printer in production
                GenerateTicket(lastSale, "Reimpresión");
            }
        }

        public void AbrirCajon()
        {
            // Simulate opening cash drawer
            // In production, this would send a signal to the cash drawer to open
            System.Diagnostics.Debug.WriteLine("Cash drawer opened");
        }
        
        public void AplicarDescuento() 
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Selecciona un artículo para aplicar descuento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var inputWin = new Views.InputWindow("Descuento", "Introduce porcentaje de descuento (%):");
                inputWin.Owner = Application.Current.MainWindow;
                if (inputWin.ShowDialog() == true)
                {
                    if (decimal.TryParse(inputWin.ResultText, out decimal discountPercent))
                    {
                        decimal discountAmount = _selectedItem.UnitPrice * (discountPercent / 100m);
                        _selectedItem.Discount = discountAmount;
                        
                        // No need to call UpdateSaleList() - SaleItem implements INotifyPropertyChanged
                        // Discount applied silently for speed
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
                // Remove last if none selected
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
                    int qty = int.Parse(ScanQuantity); // Use current numpad quantity
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
                    ScanQuantity = "1"; // Reset
                }
                // Product not found - silently ignore for speed
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

                // Calculate totals by payment method
                var efectivoSales = sales.Where(s => s.MetodoPago == "Efectivo").ToList();
                var tarjetaSales = sales.Where(s => s.MetodoPago == "Tarjeta").ToList();
                var otrosSales = sales.Where(s => s.MetodoPago != "Efectivo" && s.MetodoPago != "Tarjeta").ToList();

                decimal totalEfectivo = efectivoSales.Sum(s => s.Total);
                decimal totalTarjeta = tarjetaSales.Sum(s => s.Total);
                decimal totalOtros = otrosSales.Sum(s => s.Total);
                decimal totalGeneral = sales.Sum(s => s.Total);

                // Generate Z Report Ticket
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
                
                // Sales by payment method
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
                
                // Save Z Report
                string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
                System.IO.Directory.CreateDirectory(folder);
                string filename = $"CierreZ_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                System.IO.File.WriteAllText(System.IO.Path.Combine(folder, filename), sb.ToString());
                
                // Mark sales as Z-closed
                foreach (var sale in sales)
                {
                    sale.ZClosed = true;
                }
                _service.UpdateSales(sales);
                
                // Refresh stats to show zero
                RefreshSalesStats();
                
                MessageBox.Show($"Cierre Z completado.\n\nTotal: {totalGeneral:C}\nTickets: {sales.Count}\n\nTicket guardado en carpeta Tickets", "Cierre Z", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar Cierre Z:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Reports ---
        // --- Sales Stats & History ---
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
            
            // Only count sales that haven't been Z-closed
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

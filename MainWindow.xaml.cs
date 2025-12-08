
using System;
using System.Windows;
using CommerceBoost.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceBoost
{
    public partial class MainWindow : Window
    {
        private readonly SalesViewModel _vm;

        public MainWindow(SalesViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            DataContext = _vm;

            // Subscribe to navigation requests
            // Subscribe to navigation requests
            _vm.RequestNavigation += OnNavigate;
            
            // Subscribe to DataGrid cell changes
            SalesGrid.CurrentCellChanged += SalesGrid_CurrentCellChanged;
        }

        private void SalesGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (SalesGrid.CurrentCell.Column != null)
            {
                string? header = SalesGrid.CurrentCell.Column.Header as string;
                if (header == "PRECIO UNIT.")
                {
                    _vm.EditingProperty = "UnitPrice";
                }
                else
                {
                    _vm.EditingProperty = "Quantity";
                }
            }
        }

        private void OnNavigate(string viewName)
        {
            switch (viewName)
            {
                case "Sales":
                    // Open Sales Management Window
                    _vm.RefreshSalesStats();
                    var salesWin = new Views.SalesManagementWindow(_vm);
                    salesWin.Owner = this;
                    salesWin.ShowDialog();
                    break;
                case "Inventory":
                    // Open Inventory Window
                    var invWin = new Views.InventoryWindow(_vm);
                    invWin.Owner = this;
                    invWin.ShowDialog();
                    break;
                case "Settings":
                    MessageBox.Show("Vista de Ajustes no implementada aún.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}

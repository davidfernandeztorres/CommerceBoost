using System.Windows;
using CommerceBoost.ViewModels;

namespace CommerceBoost.Views
{
    public partial class InventoryWindow : Window
    {
        public InventoryWindow(SalesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}

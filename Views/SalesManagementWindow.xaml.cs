using System.Windows;
using CommerceBoost.ViewModels;

namespace CommerceBoost.Views
{
    public partial class SalesManagementWindow : Window
    {
        public SalesManagementWindow(SalesViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}

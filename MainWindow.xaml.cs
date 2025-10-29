using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using CommerceBoost.Services;
using CommerceBoost.Data;
using System.Windows.Controls;

namespace CommerceBoost;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // DataContext = ViewModel;
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "index.html"));
            
            // Exponer servicio C# al JavaScript
            webView.CoreWebView2.AddHostObjectToScript("ServicioComercio", new ServicioComercio(new ContextoComercio()));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is string htmlFile)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", htmlFile);
            webView.CoreWebView2.Navigate(path);
        }
    }
}
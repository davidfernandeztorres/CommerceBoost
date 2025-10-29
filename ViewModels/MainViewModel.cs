using CommerceBoost.Data;
using CommerceBoost.Models;
using CommerceBoost.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CommerceBoost.ViewModels;

public class ModeloVistaPrincipal : INotifyPropertyChanged
{
    private readonly ServicioComercio _servicioComercio;

    public ObservableCollection<Producto> Productos { get; } = new();
    public ObservableCollection<Venta> Ventas { get; } = new();
    public ObservableCollection<Cliente> Clientes { get; } = new();

    public ModeloVistaPrincipal()
    {
        try
        {
            var contexto = new ContextoComercio();
            _servicioComercio = new ServicioComercio(contexto);
            CargarDatosAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error en ModeloVista: {ex.Message}");
            _servicioComercio = null!;
        }
    }

    private async void CargarDatosAsync()
    {
        var productos = await _servicioComercio.ObtenerProductosAsync();
        foreach (var p in productos) Productos.Add(p);

        var ventas = await _servicioComercio.ObtenerVentasAsync();
        foreach (var v in ventas) Ventas.Add(v);

        var clientes = await _servicioComercio.ObtenerClientesAsync();
        foreach (var c in clientes) Clientes.Add(c);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
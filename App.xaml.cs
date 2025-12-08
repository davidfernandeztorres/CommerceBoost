using System.Windows;
using CommerceBoost.ViewModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CommerceBoost.Data;
using CommerceBoost.Services;
using System;

namespace CommerceBoost
{
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("CommerceDb");
                    services.AddDbContext<CommerceDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    services.AddTransient<CommerceService>();
                    services.AddTransient<SalesViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host!.StartAsync();

            // Ensure DB is created
            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
                // db.Database.EnsureCreated(); // Uncomment if you want auto-creation, but usually migrations are better. 
                // For this project/user request, let's assume they might need it created if it doesn't exist.
                try 
                {
                   db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to DB: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host!.StopAsync();
            }
            base.OnExit(e);
        }
    }
}

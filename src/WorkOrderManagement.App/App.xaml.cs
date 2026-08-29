using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;
using WorkOrderManagement.Application.Interfaces;
using WorkOrderManagement.Application.Services;
using WorkOrderManagement.App.ViewModels;
using WorkOrderManagement.App.Views;
using WorkOrderManagement.Infrastructure.Data;
using WorkOrderManagement.Infrastructure.Repositories;

namespace WorkOrderManagement.App;

/// <summary>
/// Application entry point.
/// Configures the DI container, logging, EF Core, and launches the main window.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = CreateHost();
        await _host.StartAsync();

        // Apply pending migrations and seed data
        await InitializeDatabaseAsync();

        // Launch main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Load initial data after window is shown
        if (mainWindow.DataContext is MainWindowViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=localhost;Database=WorkOrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

                // Configure EF Core with SQL Server or SQLite based on connection string
                bool isSqlite = connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
                                (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) &&
                                 !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                                 !connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase));

                if (isSqlite)
                {
                    services.AddDbContext<WorkOrderDbContext>(options =>
                        options.UseSqlite(connectionString));
                }
                else
                {
                    services.AddDbContext<WorkOrderDbContext>(options =>
                        options.UseSqlServer(connectionString));
                }

                // Infrastructure
                services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
                services.AddScoped<ITechnicianRepository, TechnicianRepository>();
                services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
                services.AddScoped<DatabaseSeeder>();

                // Application services
                services.AddScoped<ITechnicianService, TechnicianService>();
                services.AddScoped<IWorkOrderService, WorkOrderService>();

                // ViewModels
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<WorkOrdersViewModel>();
                services.AddSingleton<TechniciansViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    private async Task InitializeDatabaseAsync()
    {
        var logger = _host!.Services.GetRequiredService<ILogger<App>>();
        try
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WorkOrderDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

            if (context.Database.IsSqlServer())
            {
                // Ensure SQL Server database and tables are created
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                // Apply any pending SQLite migrations
                await context.Database.MigrateAsync();
            }

            // Seed only if the database is empty
            await seeder.SeedAsync();

            logger.LogInformation("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
            MessageBox.Show(
                $"Failed to initialize the database.\n\nError: {ex.Message}\n\n" +
                "Please verify your SQL Server connection string in appsettings.json or ensure SQL Server is running.",
                "Database Connection / Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}

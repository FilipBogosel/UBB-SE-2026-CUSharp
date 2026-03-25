#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieApp.Core.Data;
using MovieApp.Core.Interfaces;
using MovieApp.Core.Services;
using MovieApp.UI.Services;
using MovieApp.UI.ViewModels;
using System.Net.Http;
using System.Windows;

namespace MovieApp.UI;

/// <summary>
/// Bootstraps the WPF application and dependency injection container.
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    private IServiceScope? _applicationScope;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            await InitializeApplicationAsync();

            var scope = _applicationScope ?? throw new InvalidOperationException("Application scope was not initialized.");
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.ToString(),
                "MovieApp Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        _applicationScope?.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }

    private async Task InitializeApplicationAsync()
    {
        try
        {
            BuildServiceProvider(useInMemoryFallback: false);
            var dbContext = _applicationScope!.ServiceProvider.GetRequiredService<MovieAppDbContext>();
            await dbContext.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(dbContext);
        }
        catch
        {
            _applicationScope?.Dispose();
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            BuildServiceProvider(useInMemoryFallback: true);
            var dbContext = _applicationScope!.ServiceProvider.GetRequiredService<MovieAppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(dbContext);

            MessageBox.Show(
                "LocalDB was not available, so MovieApp started with an in-memory demo database. Data will reset when the app closes.",
                "MovieApp Demo Mode",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void BuildServiceProvider(bool useInMemoryFallback)
    {
        var services = new ServiceCollection();
        ConfigureServices(services, useInMemoryFallback);
        _serviceProvider = services.BuildServiceProvider();
        _applicationScope = _serviceProvider.CreateScope();
    }

    private static void ConfigureServices(IServiceCollection services, bool useInMemoryFallback)
    {
        services.AddDbContext<MovieAppDbContext>(options =>
        {
            if (useInMemoryFallback)
            {
                options.UseInMemoryDatabase("MovieAppDemoDb");
            }
            else
            {
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MovieAppDb;Trusted_Connection=True;");
            }
        });

        services.AddSingleton(new HttpClient());
        services.AddSingleton<ExternalReviewService>();

        services.AddScoped<MovieSelectionService>();
        services.AddScoped<GamificationRefreshService>();

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPointService, PointService>();
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<ICommentService, CommentService>();

        services.AddScoped<CatalogViewModel>();
        services.AddScoped<MovieDetailViewModel>();
        services.AddScoped<BattleViewModel>();
        services.AddScoped<ForumViewModel>();
        services.AddScoped<ProfileViewModel>();
        services.AddScoped<MainViewModel>();
        services.AddScoped<MainWindow>();
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            e.Exception.ToString(),
            "MovieApp Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
        Shutdown();
    }
}

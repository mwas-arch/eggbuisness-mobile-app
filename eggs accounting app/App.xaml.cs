using eggs_accounting_app.Data;
using Microsoft.EntityFrameworkCore;

namespace eggs_accounting_app;

public partial class App : Application
{
    private readonly IDbContextFactory<EggBusinessDbContext> _dbFactory;

    public App(
        IDbContextFactory<EggBusinessDbContext> dbFactory)
    {
        InitializeComponent();

        _dbFactory = dbFactory;

        MainPage = new AppShell();

        InitializeDatabaseAsync();
    }

    private async void InitializeDatabaseAsync()
    {
        try
        {
            await using var db =
                await _dbFactory.CreateDbContextAsync();

            await db.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Database initialization failed: {ex}");
        }
    }
}
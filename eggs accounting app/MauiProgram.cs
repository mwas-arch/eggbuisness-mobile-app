using eggs_accounting_app.Data;
using Microsoft.EntityFrameworkCore;
using eggs_accounting_app.Services;

namespace eggs_accounting_app;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });

        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "eggbusiness.db3");

        builder.Services.AddDbContextFactory<EggBusinessDbContext>(
            options =>
                options.UseSqlite(
                    $"Data Source={databasePath}"));

        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<LocalDatabaseService>();

        return builder.Build();
    }
}

using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class SalesHistoryPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public SalesHistoryPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadSalesAsync();
        };
    }

    private async Task LoadSalesAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RefreshButton.IsEnabled = false;

            var sales =
                await _databaseService.GetSalesAsync();

            var history =
                sales.Select(s => new SaleHistoryItem
                {
                    SaleNumber =
                        $"Sale #{s.Id}",

                    CustomerName =
                        string.IsNullOrWhiteSpace(
                            s.CustomerName)
                            ? "Walk-in Customer"
                            : s.CustomerName,

                    SaleDate =
                        $"Date: " +
                        $"{s.SaleDate.ToLocalTime():dd/MM/yyyy HH:mm}",

                    PaymentMethod =
                        $"Payment: {s.PaymentMethod}",

                    TotalAmount =
                        $"Total: Ksh {s.TotalAmount:N2}",

                    AmountPaid =
                        $"Paid: Ksh {s.AmountPaid:N2}",

                    BalanceDue =
                        $"Balance: Ksh {s.BalanceDue:N2}"
                }).ToList();

            SalesList.ItemsSource = history;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load sales.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RefreshButton.IsEnabled = true;
        }
    }

    private async void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        await LoadSalesAsync();
    }
}

public class SaleHistoryItem
{
    public string SaleNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string SaleDate { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public string TotalAmount { get; set; } = string.Empty;

    public string AmountPaid { get; set; } = string.Empty;

    public string BalanceDue { get; set; } = string.Empty;
}


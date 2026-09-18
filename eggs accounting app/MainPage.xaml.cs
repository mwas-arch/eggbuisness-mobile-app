using eggs_accounting_app.Pages;
using eggs_accounting_app.Services;
using eggs_accounting_app.Models;

namespace eggs_accounting_app;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadDashboard();
    }

    private async void LoadDashboard()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var dashboard =
                await _apiService.GetDashboardAsync();

            if (dashboard == null)
            {
                await DisplayAlertAsync(
                    "Error",
                    "No dashboard data was returned.",
                    "OK");

                return;
            }

            StockLabel.Text =
                $"Total Eggs: {dashboard.Inventory.TotalEggs}";

            CratesLabel.Text =
                $"Crates: {dashboard.Inventory.Crates}";

            LooseEggsLabel.Text =
                $"Loose Eggs: {dashboard.Inventory.LooseEggs}";

            TodaySalesLabel.Text =
                $"Today's Sales: Ksh {dashboard.Sales.TodaySales:N2}";

            TotalSalesLabel.Text =
                $"Total Sales: Ksh {dashboard.Sales.TotalSales:N2}";

            CreditLabel.Text =
                $"Outstanding Credit: Ksh {dashboard.Sales.OutstandingCredit:N2}";

            TodayExpensesLabel.Text =
                $"Today's Expenses: Ksh {dashboard.Expenses.TodayExpenses:N2}";

            TotalExpensesLabel.Text =
                $"Total Expenses: Ksh {dashboard.Expenses.TotalExpenses:N2}";

            TodayProfitLabel.Text =
                $"Today's Profit: Ksh {dashboard.Profit.Today:N2}";

            OverallProfitLabel.Text =
                $"Overall Profit: Ksh {dashboard.Profit.Overall:N2}";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Dashboard Error",
                $"Could not load dashboard.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        LoadDashboard();
    }

    private async void OnSuppliersClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(SuppliersPage));
    }

    private async void OnInventoryClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(InventoryPage));
    }

    private async void OnSalesClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(SalesPage));
    }

    private async void OnExpensesClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ExpensesPage));
    }
    private async void OnCustomersClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(CustomersPage));
    }
    private async void OnCustomerAccountsClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(CustomerAccountPage));
    }
    private async void OnStockAdjustmentsClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(StockAdjustmentsPage));
    }
    private async void OnProductPricesClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ProductPricesPage));
    }
   
}   
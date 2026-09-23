
using eggs_accounting_app.Models;
using eggs_accounting_app.Pages;
using eggs_accounting_app.Services;

namespace eggs_accounting_app;

public partial class MainPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public MainPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadDashboardAsync();
        };
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var inventory =
                await _databaseService.GetInventoryAsync();

            var sales =
                await _databaseService.GetSalesAsync();

            var expenses =
                await _databaseService.GetExpensesAsync();

            var today =
                DateTime.Today;

            // -----------------------------
            // INVENTORY
            // -----------------------------

            int totalEggs =
                inventory.Sum(b => b.EggsRemaining);

            int crates =
                totalEggs / 30;

            int looseEggs =
                totalEggs % 30;

            // -----------------------------
            // SALES
            // -----------------------------

            decimal totalSales =
                sales.Sum(s => s.TotalAmount);

            decimal outstandingCredit =
                sales.Sum(s => s.BalanceDue);

            decimal todaySales =
                sales
                    .Where(s =>
                        s.SaleDate.ToLocalTime().Date ==
                        today)
                    .Sum(s => s.TotalAmount);

            // -----------------------------
            // EXPENSES
            // -----------------------------

            decimal totalExpenses =
                expenses.Sum(e => e.Amount);

            decimal todayExpenses =
                expenses
                    .Where(e =>
                        e.ExpenseDate.ToLocalTime().Date ==
                        today)
                    .Sum(e => e.Amount);

            // -----------------------------
            // PROFIT
            // -----------------------------

            decimal todayProfit =
                todaySales - todayExpenses;

            decimal overallProfit =
                totalSales - totalExpenses;

            // -----------------------------
            // UPDATE DASHBOARD
            // -----------------------------

            StockLabel.Text =
                $"Total Eggs: {totalEggs}";

            CratesLabel.Text =
                $"Crates: {crates}";

            LooseEggsLabel.Text =
                $"Loose Eggs: {looseEggs}";

            TodaySalesLabel.Text =
                $"Today's Sales: Ksh {todaySales:N2}";

            TotalSalesLabel.Text =
                $"Total Sales: Ksh {totalSales:N2}";

            CreditLabel.Text =
                $"Outstanding Credit: Ksh {outstandingCredit:N2}";

            TodayExpensesLabel.Text =
                $"Today's Expenses: Ksh {todayExpenses:N2}";

            TotalExpensesLabel.Text =
                $"Total Expenses: Ksh {totalExpenses:N2}";

            TodayProfitLabel.Text =
                $"Today's Profit: Ksh {todayProfit:N2}";

            OverallProfitLabel.Text =
                $"Overall Profit: Ksh {overallProfit:N2}";
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

    private async void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        await LoadDashboardAsync();
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



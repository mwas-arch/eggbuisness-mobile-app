
using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class ExpenseHistoryPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public ExpenseHistoryPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadExpensesAsync();
        };
    }

    private async Task LoadExpensesAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RefreshButton.IsEnabled = false;

            var expenses =
                await _databaseService.GetExpensesAsync();

            var history = expenses
                .Select(expense => new ExpenseHistoryItem
                {
                    ExpenseNumber =
                        $"Expense #{expense.Id}",

                    Category =
                        $"Category: {expense.Category}",

                    Amount =
                        $"Amount: Ksh {expense.Amount:N2}",

                    Description =
                        string.IsNullOrWhiteSpace(
                            expense.Description)
                            ? "No description"
                            : expense.Description,

                    ExpenseDate =
                        $"Date: {expense.ExpenseDate.ToLocalTime():dd/MM/yyyy HH:mm}"
                })
                .ToList();

            ExpensesList.ItemsSource = history;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load expenses.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RefreshButton.IsEnabled = true;
        }
    }

    private void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        LoadExpensesAsync();
    }
}

public class ExpenseHistoryItem
{
    public string ExpenseNumber { get; set; } =
        string.Empty;

    public string Category { get; set; } =
        string.Empty;

    public string Amount { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string ExpenseDate { get; set; } =
        string.Empty;
}


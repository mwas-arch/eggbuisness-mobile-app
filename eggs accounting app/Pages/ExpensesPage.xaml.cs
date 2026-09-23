
using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class ExpensesPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public ExpensesPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        CategoryPicker.SelectedIndex = 0;
    }

    private async void OnAddExpenseClicked(
        object? sender,
        EventArgs e)
    {
        try
        {
            if (CategoryPicker.SelectedItem == null)
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Please select an expense category.",
                    "OK");

                return;
            }

            if (!decimal.TryParse(
                    AmountEntry.Text,
                    out decimal amount) ||
                amount <= 0)
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Enter a valid expense amount.",
                    "OK");

                return;
            }

            var expense = new Expense
            {
                Category =
                    CategoryPicker.SelectedItem.ToString()
                    ?? "Other",

                Amount = amount,

                Description =
                    DescriptionEditor.Text?.Trim(),

                ExpenseDate =
                    DateTime.UtcNow,

                CreatedBy =
                    "Mobile App"
            };

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            AddExpenseButton.IsEnabled = false;

            await _databaseService.AddExpenseAsync(
                expense);

            await DisplayAlertAsync(
                "Expense Added",
                $"Expense added successfully.\n\n" +
                $"Category: {expense.Category}\n" +
                $"Amount: Ksh {expense.Amount:N2}\n" +
                $"Description: {expense.Description}",
                "OK");

            AmountEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not add expense.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            AddExpenseButton.IsEnabled = true;
        }
    }

    private async void OnExpenseHistoryClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(ExpenseHistoryPage));
    }
}


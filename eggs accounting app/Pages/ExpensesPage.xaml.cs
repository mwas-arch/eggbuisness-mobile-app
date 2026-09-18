using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class ExpensesPage : ContentPage
{
    private readonly ApiService _apiService;

    public ExpensesPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

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

                Description = DescriptionEditor.Text,

                ExpenseDate = DateTime.UtcNow,

                CreatedBy = "Mobile App"
            };

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            AddExpenseButton.IsEnabled = false;

            var response =
                await _apiService.CreateExpenseAsync(expense);

            var message =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync(
                    "Expense Added",
                    $"Expense added successfully.\n\n" +
                    $"Category: {expense.Category}\n" +
                    $"Amount: Ksh {expense.Amount:N2}\n" +
                    $"Description: {expense.Description}",
                    "OK");

                AmountEntry.Text = "";
                DescriptionEditor.Text = "";
            }
            else
            {
                await DisplayAlertAsync(
                    "Expense Failed",
                    message,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Connection Error",
                ex.Message,
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
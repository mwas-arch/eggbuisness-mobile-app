using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class CustomerAccountPage : ContentPage
{
    private readonly ApiService _apiService;

    public CustomerAccountPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadCustomers();
    }

    private async void LoadCustomers()
    {
        try
        {
            var customers =
                await _apiService.GetCustomersAsync();

            CustomerPicker.ItemsSource = customers;
            CustomerPicker.ItemDisplayBinding =
                new Binding("Name");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load customers.\n\n{ex.Message}",
                "OK");
        }
    }

    private async void OnCustomerSelected(
        object? sender,
        EventArgs e)
    {
        if (CustomerPicker.SelectedItem is not Customer customer)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var account =
                await _apiService.GetCustomerAccountAsync(
                    customer.Id);

            if (account == null)
            {
                await DisplayAlertAsync(
                    "Error",
                    "Customer account could not be loaded.",
                    "OK");

                return;
            }

            CustomerNameLabel.Text =
                $"Customer: {account.Name}";

            PhoneLabel.Text =
                $"Phone: {account.Phone ?? "-"}";

            CreditLimitLabel.Text =
                $"Credit Limit: Ksh {account.CreditLimit:N2}";

            TotalPurchasesLabel.Text =
                $"Total Purchases: Ksh {account.TotalPurchases:N2}";

            TotalPaidLabel.Text =
                $"Total Paid: Ksh {account.TotalPaid:N2}";

            BalanceLabel.Text =
                $"Outstanding Balance: Ksh {account.OutstandingBalance:N2}";

            AvailableCreditLabel.Text =
                $"Available Credit: Ksh {account.AvailableCredit:N2}";

            SalesCollection.ItemsSource =
                account.Sales;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load customer account.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
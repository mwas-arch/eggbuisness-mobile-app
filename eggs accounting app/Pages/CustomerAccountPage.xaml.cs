using eggs_accounting_app.Models;
using eggs_accounting_app.Services;
using Microsoft.EntityFrameworkCore;

namespace eggs_accounting_app.Pages;

public partial class CustomerAccountPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public CustomerAccountPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadCustomersAsync();
        };
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers =
                await _databaseService.GetCustomersAsync();

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
        if (CustomerPicker.SelectedItem
            is not Customer customer)
        {
            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var customers =
                await _databaseService.GetCustomersAsync();

            var selectedCustomer =
                customers.FirstOrDefault(
                    c => c.Id == customer.Id);

            if (selectedCustomer == null)
            {
                await DisplayAlertAsync(
                    "Error",
                    "Customer could not be found.",
                    "OK");

                return;
            }

            var sales =
                await _databaseService.GetSalesAsync();

            var customerSales =
                sales
                    .Where(s =>
                        s.CustomerId == selectedCustomer.Id)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();

            decimal totalPurchases =
                customerSales.Sum(s => s.TotalAmount);

            decimal totalPaid =
                customerSales.Sum(s => s.AmountPaid);

            decimal outstandingBalance =
                customerSales.Sum(s => s.BalanceDue);

            decimal availableCredit =
                selectedCustomer.CreditLimit
                - outstandingBalance;

            if (availableCredit < 0)
            {
                availableCredit = 0;
            }

            CustomerNameLabel.Text =
                $"Customer: {selectedCustomer.Name}";

            PhoneLabel.Text =
                $"Phone: {selectedCustomer.Phone ?? "-"}";

            CreditLimitLabel.Text =
                $"Credit Limit: Ksh {selectedCustomer.CreditLimit:N2}";

            TotalPurchasesLabel.Text =
                $"Total Purchases: Ksh {totalPurchases:N2}";

            TotalPaidLabel.Text =
                $"Total Paid: Ksh {totalPaid:N2}";

            BalanceLabel.Text =
                $"Outstanding Balance: Ksh {outstandingBalance:N2}";

            AvailableCreditLabel.Text =
                $"Available Credit: Ksh {availableCredit:N2}";

            SalesCollection.ItemsSource =
                customerSales;
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
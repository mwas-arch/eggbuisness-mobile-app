using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class CustomersPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public CustomersPage(
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
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var customers =
                await _databaseService.GetCustomersAsync();

            CustomersCollection.ItemsSource = customers;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load customers.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddCustomerClicked(
        object? sender,
        EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync(
                "Missing Name",
                "Please enter the customer name.",
                "OK");

            return;
        }

        decimal creditLimit = 0;

        if (!string.IsNullOrWhiteSpace(CreditLimitEntry.Text) &&
            !decimal.TryParse(
                CreditLimitEntry.Text,
                out creditLimit))
        {
            await DisplayAlertAsync(
                "Invalid Credit Limit",
                "Please enter a valid credit limit.",
                "OK");

            return;
        }

        if (creditLimit < 0)
        {
            await DisplayAlertAsync(
                "Invalid Credit Limit",
                "Credit limit cannot be negative.",
                "OK");

            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var customer = new Customer
            {
                Name = NameEntry.Text.Trim(),

                Phone = PhoneEntry.Text?.Trim(),

                Address = AddressEntry.Text?.Trim(),

                CreditLimit = creditLimit
            };

            await _databaseService.AddCustomerAsync(
                customer);

            await DisplayAlertAsync(
                "Success",
                "Customer added successfully.",
                "OK");

            NameEntry.Text = string.Empty;
            PhoneEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
            CreditLimitEntry.Text = string.Empty;

            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not add customer.\n\n{ex.Message}",
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
        LoadCustomersAsync();
    }
}
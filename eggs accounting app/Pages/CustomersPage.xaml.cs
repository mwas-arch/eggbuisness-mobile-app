using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class CustomersPage : ContentPage
{
    private readonly ApiService _apiService;

    public CustomersPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadCustomers();
    }

    private async void LoadCustomers()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var customers = await _apiService.GetCustomersAsync();

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

            var response =
                await _apiService.CreateCustomerAsync(customer);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync(
                    "Success",
                    "Customer added successfully.",
                    "OK");

                NameEntry.Text = string.Empty;
                PhoneEntry.Text = string.Empty;
                AddressEntry.Text = string.Empty;
                CreditLimitEntry.Text = string.Empty;

                LoadCustomers();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                await DisplayAlertAsync(
                    "Error",
                    $"Customer could not be added.\n\n{error}",
                    "OK");
            }
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
        LoadCustomers();
    }
}
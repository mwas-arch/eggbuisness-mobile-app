
using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class SuppliersPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public SuppliersPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadSuppliersAsync();
        };
    }

    private async Task LoadSuppliersAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RefreshButton.IsEnabled = false;

            List<Supplier> suppliers =
                await _databaseService.GetSuppliersAsync();

            SuppliersList.ItemsSource = suppliers;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load suppliers.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RefreshButton.IsEnabled = true;
        }
    }

    private async void OnAddSupplierClicked(
        object? sender,
        EventArgs e)
    {
        try
        {
            string name =
                SupplierNameEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Please enter the supplier name.",
                    "OK");

                return;
            }

            AddSupplierButton.IsEnabled = false;

            var supplier = new Supplier
            {
                Name = name,

                Phone =
                    string.IsNullOrWhiteSpace(
                        SupplierPhoneEntry.Text)
                        ? null
                        : SupplierPhoneEntry.Text.Trim(),

                Email =
                    string.IsNullOrWhiteSpace(
                        SupplierEmailEntry.Text)
                        ? null
                        : SupplierEmailEntry.Text.Trim(),

                Address =
                    string.IsNullOrWhiteSpace(
                        SupplierAddressEntry.Text)
                        ? null
                        : SupplierAddressEntry.Text.Trim(),

                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.AddSupplierAsync(
                supplier);

            await DisplayAlertAsync(
                "Success",
                $"Supplier '{supplier.Name}' was added successfully.",
                "OK");

            // Clear form
            SupplierNameEntry.Text = string.Empty;
            SupplierPhoneEntry.Text = string.Empty;
            SupplierEmailEntry.Text = string.Empty;
            SupplierAddressEntry.Text = string.Empty;

            // Reload local SQLite data
            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not add supplier.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            AddSupplierButton.IsEnabled = true;
        }
    }

    private async void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        await LoadSuppliersAsync();
    }
}

using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class SuppliersPage : ContentPage
{
    private readonly ApiService _apiService;

    public SuppliersPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadSuppliersAsync();
    }

    private async void LoadSuppliersAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RefreshButton.IsEnabled = false;

            List<Supplier> suppliers =
                await _apiService.GetSuppliersAsync();

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

    private void OnRefreshClicked(
        object? sender,
        EventArgs e)
    {
        LoadSuppliersAsync();
    }
}
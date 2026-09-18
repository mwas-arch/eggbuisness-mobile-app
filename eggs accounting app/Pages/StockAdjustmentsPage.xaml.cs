using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class StockAdjustmentsPage : ContentPage
{
    private readonly ApiService _apiService;

    public StockAdjustmentsPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadInventory();
    }

    private async void LoadInventory()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var inventory =
                await _apiService.GetInventoryAsync();

            var batches = inventory
                .Select(batch => new InventoryBatchDisplay
                {
                    Id = batch.Id,
                    Grade = batch.Grade,
                    EggsRemaining = batch.EggsRemaining,
                    DisplayName =
                        $"Batch #{batch.Id} - {batch.Grade} - " +
                        $"{batch.EggsRemaining} eggs remaining"
                })
                .ToList();

            BatchPicker.ItemsSource = batches;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load inventory.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRecordAdjustmentClicked(
        object? sender,
        EventArgs e)
    {
        if (BatchPicker.SelectedItem is not InventoryBatchDisplay batch)
        {
            await DisplayAlertAsync(
                "Missing Batch",
                "Please select an inventory batch.",
                "OK");

            return;
        }

        if (!int.TryParse(
                QuantityEntry.Text,
                out int quantity) ||
            quantity <= 0)
        {
            await DisplayAlertAsync(
                "Invalid Quantity",
                "Enter a valid number of eggs.",
                "OK");

            return;
        }

        if (quantity > batch.EggsRemaining)
        {
            await DisplayAlertAsync(
                "Insufficient Stock",
                $"Only {batch.EggsRemaining} eggs remain in this batch.",
                "OK");

            return;
        }

        var reason =
            ReasonPicker.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(reason))
        {
            await DisplayAlertAsync(
                "Missing Reason",
                "Please select a reason for the adjustment.",
                "OK");

            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var adjustment = new StockAdjustment
            {
                InventoryBatchId = batch.Id,
                QuantityEggs = quantity,
                Reason = reason,
                LoggedBy = "Mobile App",
                Timestamp = DateTime.UtcNow
            };

            var response =
                await _apiService.CreateStockAdjustmentAsync(
                    adjustment);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync(
                    "Success",
                    $"{quantity} eggs have been deducted from Batch #{batch.Id}.",
                    "OK");

                QuantityEntry.Text = string.Empty;
                ReasonPicker.SelectedItem = null;
                BatchPicker.SelectedItem = null;

                LoadInventory();
            }
            else
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlertAsync(
                    "Error",
                    $"Could not record adjustment.\n\n{error}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not record adjustment.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}

public class InventoryBatchDisplay
{
    public int Id { get; set; }

    public string Grade { get; set; } = string.Empty;

    public int EggsRemaining { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

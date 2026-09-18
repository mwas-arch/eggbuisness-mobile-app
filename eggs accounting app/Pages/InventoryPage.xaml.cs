using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class InventoryPage : ContentPage
{
private readonly ApiService _apiService;


public InventoryPage()
{
    InitializeComponent();

    _apiService = new ApiService();

    ReceivedDatePicker.Date = DateTime.Today;

    LoadSuppliersAsync();
    LoadInventoryAsync();
}

private async Task LoadInventoryAsync()
{
    try
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        RefreshButton.IsEnabled = false;

        List<InventoryBatch> inventory =
            await _apiService.GetInventoryAsync();

        InventoryList.ItemsSource = inventory;
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
        RefreshButton.IsEnabled = true;
    }
}

private async void LoadSuppliersAsync()
{
    try
    {
        List<Supplier> suppliers =
            await _apiService.GetSuppliersAsync();

        SupplierPicker.ItemsSource = suppliers;
        SupplierPicker.ItemDisplayBinding =
            new Binding("Name");
    }
    catch (Exception ex)
    {
        await DisplayAlertAsync(
            "Error",
            $"Could not load suppliers.\n\n{ex.Message}",
            "OK");
    }
}

private void OnRefreshClicked(
    object? sender,
    EventArgs e)
{
    LoadInventoryAsync();
}

private void OnAddBatchClicked(
    object? sender,
    EventArgs e)
{
    AddBatchForm.IsVisible = true;
    AddBatchButton.IsVisible = false;
    BatchMessageLabel.Text = string.Empty;
}

private void OnCancelBatchClicked(
    object? sender,
    EventArgs e)
{
    AddBatchForm.IsVisible = false;
    AddBatchButton.IsVisible = true;

    ClearBatchForm();
}

private async void OnSaveBatchClicked(
    object? sender,
    EventArgs e)
{
    try
    {
        SaveBatchIndicator.IsVisible = true;
        SaveBatchIndicator.IsRunning = true;

        BatchMessageLabel.Text = string.Empty;

        if (SupplierPicker.SelectedItem is not Supplier supplier)
        {
            await DisplayAlertAsync(
                "Validation",
                "Please select a supplier.",
                "OK");

            return;
        }

        if (GradePicker.SelectedItem is not string grade ||
            string.IsNullOrWhiteSpace(grade))
        {
            await DisplayAlertAsync(
                "Validation",
                "Please select an egg grade.",
                "OK");

            return;
        }

        if (!int.TryParse(
                CratesEntry.Text,
                out int crates) ||
            crates <= 0)
        {
            await DisplayAlertAsync(
                "Validation",
                "Enter a valid number of crates.",
                "OK");

            return;
        }

        if (!int.TryParse(
                EggsPerCrateEntry.Text,
                out int eggsPerCrate) ||
            eggsPerCrate <= 0)
        {
            await DisplayAlertAsync(
                "Validation",
                "Enter a valid number of eggs per crate.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                CostPerCrateEntry.Text,
                out decimal costPerCrate) ||
            costPerCrate < 0)
        {
            await DisplayAlertAsync(
                "Validation",
                "Enter a valid cost per crate.",
                "OK");

            return;
        }

        int eggsReceived =
            crates * eggsPerCrate;

        var request =
            new CreateInventoryBatchRequest
            {
                SupplierId = supplier.Id,
                Grade = grade,
                EggsReceived = eggsReceived,
                EggsPerCrate = eggsPerCrate,
                CostPerCrate = costPerCrate,
                ReceivedDate =
                    ReceivedDatePicker.Date ?? DateTime.Today
            };

        HttpResponseMessage response =
            await _apiService.CreateInventoryBatchAsync(
                request);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlertAsync(
                "Success",
                $"Inventory batch added successfully.\n\n" +
                $"Grade: {grade}\n" +
                $"Crates: {crates}\n" +
                $"Eggs: {eggsReceived}",
                "OK");

            ClearBatchForm();

            AddBatchForm.IsVisible = false;
            AddBatchButton.IsVisible = true;

            await LoadInventoryAsync();
        }
        else
        {
            string error =
                await response.Content.ReadAsStringAsync();

            await DisplayAlertAsync(
                "Error",
                $"Could not save inventory batch.\n\n{error}",
                "OK");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlertAsync(
            "Error",
            $"Could not save inventory batch.\n\n{ex.Message}",
            "OK");
    }
    finally
    {
        SaveBatchIndicator.IsRunning = false;
        SaveBatchIndicator.IsVisible = false;
    }
}

private void ClearBatchForm()
{
    SupplierPicker.SelectedItem = null;
    GradePicker.SelectedItem = null;

    CratesEntry.Text = string.Empty;

    EggsPerCrateEntry.Text = "30";

    CostPerCrateEntry.Text = string.Empty;

    ReceivedDatePicker.Date = DateTime.Today;

    BatchMessageLabel.Text = string.Empty;
}

}

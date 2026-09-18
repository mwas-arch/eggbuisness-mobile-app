using System.Text.Json;
using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class SalesPage : ContentPage
{
private readonly ApiService _apiService;


public SalesPage()
{
    InitializeComponent();

    _apiService = new ApiService();

    PriceTypePicker.SelectedIndex = 0;
    PaymentMethodPicker.SelectedIndex = 0;

    LoadCustomers();
    LoadProducts();
}

private async void LoadCustomers()
{
    try
    {
        var customers = await _apiService.GetCustomersAsync();

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

private async void LoadProducts()
{
    try
    {
        var products = await _apiService.GetProductsAsync();

        ProductPicker.ItemsSource = products;
    }
    catch (Exception ex)
    {
        await DisplayAlertAsync(
            "Error",
            $"Could not load products.\n\n{ex.Message}",
            "OK");
    }
}

private async void OnCreateSaleClicked(
    object? sender,
    EventArgs e)
{
    try
    {
        if (ProductPicker.SelectedItem is not Product product)
        {
            await DisplayAlertAsync(
                "Validation",
                "Please select a product.",
                "OK");

            return;
        }

        if (!int.TryParse(
                CratesEntry.Text,
                out int crates))
        {
            crates = 0;
        }

        if (!int.TryParse(
                LooseEggsEntry.Text,
                out int looseEggs))
        {
            looseEggs = 0;
        }

        int quantityEggs =
            (crates * product.EggsPerCrate)
            + looseEggs;

        if (quantityEggs <= 0)
        {
            await DisplayAlertAsync(
                "Validation",
                "Enter a valid quantity.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                AmountPaidEntry.Text,
                out decimal amountPaid))
        {
            amountPaid = 0;
        }

        var selectedCustomer =
            CustomerPicker.SelectedItem as Customer;

        var request = new CreateSaleRequest
        {
            CustomerId = selectedCustomer?.Id,

            PaymentMethod =
                PaymentMethodPicker.SelectedItem?.ToString()
                ?? "Cash",

            AmountPaid = amountPaid,

            Items = new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    QuantityEggs = quantityEggs,
                    PriceType =
                        PriceTypePicker.SelectedItem?.ToString()
                        ?? "Retail"
                }
            }
        };

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        CreateSaleButton.IsEnabled = false;

        var response =
            await _apiService.CreateSaleAsync(request);

        var message =
            await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var sale =
                    JsonSerializer.Deserialize<SaleResponse>(
                        message,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (sale != null)
                {
                    string customerName =
                        selectedCustomer?.Name
                        ?? "Walk-in Customer";

                    string receipt =
                        $"Sale #: {sale.Id}\n\n" +
                        $"Customer: {customerName}\n" +
                        $"Product: {product.Name}\n" +
                        $"Grade: {product.Grade}\n" +
                        $"Quantity: {crates} crate(s) + {looseEggs} egg(s)\n\n" +
                        $"Total: Ksh {sale.TotalAmount:N2}\n" +
                        $"Paid: Ksh {sale.AmountPaid:N2}\n" +
                        $"Balance: Ksh {sale.BalanceDue:N2}\n\n" +
                        $"Payment: {sale.PaymentMethod}";

                    await DisplayAlertAsync(
                        "Sale Created Successfully",
                        receipt,
                        "OK");
                }
                else
                {
                    await DisplayAlertAsync(
                        "Sale Created",
                        "The sale was created successfully.",
                        "OK");
                }
            }
            catch
            {
                await DisplayAlertAsync(
                    "Sale Created",
                    "The sale was created successfully.",
                    "OK");
            }

            CratesEntry.Text = "";
            LooseEggsEntry.Text = "";
            AmountPaidEntry.Text = "";
            CustomerPicker.SelectedItem = null;
        }
        else
        {
            await DisplayAlertAsync(
                "Sale Failed",
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
        CreateSaleButton.IsEnabled = true;
    }
}

private async void OnPriceTypeChanged(
object? sender,
EventArgs e)
{
if (ProductPicker.SelectedItem is not Product product)
{
CurrentPriceLabel.Text = "Current Price: -";
EstimatedTotalLabel.Text = "Estimated Total: Ksh 0.00";
return;
}


try
{
    var price =
        await _apiService.GetProductPriceAsync(product.Id);

    if (price == null)
    {
        CurrentPriceLabel.Text =
            "Current Price: No pricing available";

        EstimatedTotalLabel.Text =
            "Estimated Total: Ksh 0.00";

        return;
    }

    var priceType =
        PriceTypePicker.SelectedItem?.ToString()
        ?? "Retail";

    decimal currentPrice;
    int crates = 0;
    int looseEggs = 0;

    int.TryParse(CratesEntry.Text, out crates);
    int.TryParse(LooseEggsEntry.Text, out looseEggs);

    decimal estimatedTotal;

    if (priceType == "Wholesale")
    {
        currentPrice = price.WholesalePricePerCrate;

        estimatedTotal =
            (crates * currentPrice)
            + (looseEggs * price.RetailPricePerEgg);

        CurrentPriceLabel.Text =
            $"Current Price: Ksh {currentPrice:N2} / crate";
    }
    else if (priceType == "Bulk")
    {
        currentPrice = price.BulkPricePerCrate;

        estimatedTotal =
            (crates * currentPrice)
            + (looseEggs * price.RetailPricePerEgg);

        CurrentPriceLabel.Text =
            $"Current Price: Ksh {currentPrice:N2} / crate";
    }
    else
    {
        currentPrice = price.RetailPricePerEgg;

        int totalEggs =
            (crates * product.EggsPerCrate)
            + looseEggs;

        estimatedTotal =
            totalEggs * currentPrice;

        CurrentPriceLabel.Text =
            $"Current Price: Ksh {currentPrice:N2} / egg";
    }

    EstimatedTotalLabel.Text =
        $"Estimated Total: Ksh {estimatedTotal:N2}";
}
catch (Exception ex)
{
    CurrentPriceLabel.Text =
        $"Could not load price: {ex.Message}";

    EstimatedTotalLabel.Text =
        "Estimated Total: Ksh 0.00";
}


}


private void OnProductChanged(
    object? sender,
    EventArgs e)
{
    OnPriceTypeChanged(sender, e);
}
private void OnQuantityChanged(
    object? sender,
    TextChangedEventArgs e)
{
    _ = UpdateEstimatedTotal();
}
private async Task UpdateEstimatedTotal()
{
    if (ProductPicker.SelectedItem is not Product product)
    {
        EstimatedTotalLabel.Text =
            "Estimated Total: Ksh 0.00";

        return;
    }

    var price =
        await _apiService.GetProductPriceAsync(product.Id);

    if (price == null)
    {
        EstimatedTotalLabel.Text =
            "Estimated Total: Ksh 0.00";

        return;
    }

    int.TryParse(CratesEntry.Text, out int crates);
    int.TryParse(LooseEggsEntry.Text, out int looseEggs);

    var priceType =
        PriceTypePicker.SelectedItem?.ToString()
        ?? "Retail";

    decimal estimatedTotal;

    if (priceType == "Wholesale")
    {
        estimatedTotal =
            (crates * price.WholesalePricePerCrate)
            + (looseEggs * price.RetailPricePerEgg);
    }
    else if (priceType == "Bulk")
    {
        estimatedTotal =
            (crates * price.BulkPricePerCrate)
            + (looseEggs * price.RetailPricePerEgg);
    }
    else
    {
        int totalEggs =
            (crates * product.EggsPerCrate)
            + looseEggs;

        estimatedTotal =
            totalEggs * price.RetailPricePerEgg;
    }

    EstimatedTotalLabel.Text =
        $"Estimated Total: Ksh {estimatedTotal:N2}";
}

private async void OnSalesHistoryClicked(
    object? sender,
    EventArgs e)
{
    await Shell.Current.GoToAsync(
        nameof(SalesHistoryPage));
}


}

public class SaleResponse
{
public int Id { get; set; }


public int? CustomerId { get; set; }

public decimal TotalAmount { get; set; }

public decimal AmountPaid { get; set; }

public decimal BalanceDue { get; set; }

public string PaymentMethod { get; set; } = string.Empty;

public DateTime SaleDate { get; set; }

}

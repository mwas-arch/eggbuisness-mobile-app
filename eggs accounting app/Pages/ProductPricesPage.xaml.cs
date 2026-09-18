using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class ProductPricesPage : ContentPage
{
    private readonly ApiService _apiService;

    public ProductPricesPage()
    {
        InitializeComponent();

        _apiService = new ApiService();

        LoadProducts();
        LoadPrices();
    }

    private async void LoadProducts()
    {
        try
        {
            var products =
                await _apiService.GetProductsAsync();

            var productItems = products
                .Select(product => new ProductDisplay
                {
                    Id = product.Id,
                    Grade = product.Grade,
                    DisplayName =
                        $"{product.Name} - {product.Grade}"
                })
                .ToList();

            ProductPicker.ItemsSource = productItems;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load products.\n\n{ex.Message}",
                "OK");
        }
    }

    private async void LoadPrices()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var prices =
                await _apiService.GetProductPricesAsync();

            var products =
                await _apiService.GetProductsAsync();

            var priceItems = prices
                .Select(price =>
                {
                    var product =
                        products.FirstOrDefault(
                            p => p.Id == price.ProductId);

                    return new PriceDisplay
                    {
                        Id = price.Id,
                        Grade = product?.Grade ?? "Unknown",
                        RetailPricePerEgg =
                            price.RetailPricePerEgg,
                        WholesalePricePerCrate =
                            price.WholesalePricePerCrate,
                        BulkPricePerCrate =
                            price.BulkPricePerCrate,
                        EffectiveFrom =
                            price.EffectiveFrom
                    };
                })
                .OrderByDescending(p => p.EffectiveFrom)
                .ToList();

            PricesCollection.ItemsSource = priceItems;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load pricing.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnSavePricingClicked(
        object? sender,
        EventArgs e)
    {
        if (ProductPicker.SelectedItem is not ProductDisplay product)
        {
            await DisplayAlertAsync(
                "Missing Product",
                "Please select a product.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                RetailPriceEntry.Text,
                out decimal retailPrice) ||
            retailPrice < 0)
        {
            await DisplayAlertAsync(
                "Invalid Retail Price",
                "Enter a valid retail price.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                WholesalePriceEntry.Text,
                out decimal wholesalePrice) ||
            wholesalePrice < 0)
        {
            await DisplayAlertAsync(
                "Invalid Wholesale Price",
                "Enter a valid wholesale crate price.",
                "OK");

            return;
        }

        if (!decimal.TryParse(
                BulkPriceEntry.Text,
                out decimal bulkPrice) ||
            bulkPrice < 0)
        {
            await DisplayAlertAsync(
                "Invalid Bulk Price",
                "Enter a valid bulk crate price.",
                "OK");

            return;
        }

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var price = new ProductPrice
            {
                ProductId = product.Id,
                RetailPricePerEgg = retailPrice,
                WholesalePricePerCrate = wholesalePrice,
                BulkPricePerCrate = bulkPrice,
                EffectiveFrom = DateTime.UtcNow,
                Grade = product.Grade
            };

            var response =
                await _apiService.CreateProductPriceAsync(price);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync(
                    "Success",
                    $"Pricing saved for {product.Grade} eggs.",
                    "OK");

                RetailPriceEntry.Text = string.Empty;
                WholesalePriceEntry.Text = string.Empty;
                BulkPriceEntry.Text = string.Empty;
                ProductPicker.SelectedItem = null;

                LoadPrices();
            }
            else
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlertAsync(
                    "Error",
                    $"Could not save pricing.\n\n{error}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not save pricing.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}

public class ProductDisplay
{
    public int Id { get; set; }

    public string Grade { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}

public class PriceDisplay
{
    public int Id { get; set; }

    public string Grade { get; set; } = string.Empty;

    public decimal RetailPricePerEgg { get; set; }

    public decimal WholesalePricePerCrate { get; set; }

    public decimal BulkPricePerCrate { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public string RetailDisplay =>
        $"Retail: Ksh {RetailPricePerEgg:N2} / egg";

    public string WholesaleDisplay =>
        $"Wholesale: Ksh {WholesalePricePerCrate:N2} / crate";

    public string BulkDisplay =>
        $"Bulk: Ksh {BulkPricePerCrate:N2} / crate";

    public string EffectiveDisplay =>
        $"Effective: {EffectiveFrom:dd MMM yyyy HH:mm}";
}

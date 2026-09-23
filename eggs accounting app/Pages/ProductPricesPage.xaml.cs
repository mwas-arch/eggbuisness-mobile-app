using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class ProductPricesPage : ContentPage
{
    private readonly LocalDatabaseService _databaseService;

    public ProductPricesPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        Loaded += async (_, _) =>
        {
            await LoadProductsAsync();
            await LoadPricesAsync();
        };
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products =
                await _databaseService.GetProductsAsync();

            var productItems =
                products
                    .Select(product => new ProductDisplay
                    {
                        Id = product.Id,
                        Grade = product.Grade,
                        DisplayName =
                            $"{product.Name} - {product.Grade}"
                    })
                    .ToList();

            ProductPicker.ItemsSource =
                productItems;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load products.\n\n{ex.Message}",
                "OK");
        }
    }

    private async Task LoadPricesAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var prices =
                await _databaseService.GetProductPricesAsync();

            var products =
                await _databaseService.GetProductsAsync();

            var priceItems =
                prices
                    .Select(price =>
                    {
                        var product =
                            products.FirstOrDefault(
                                p => p.Id == price.ProductId);

                        return new PriceDisplay
                        {
                            Id = price.Id,

                            Grade =
                                product?.Grade
                                ?? price.Grade
                                ?? "Unknown",

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
                    .OrderByDescending(
                        p => p.EffectiveFrom)
                    .ToList();

            PricesCollection.ItemsSource =
                priceItems;
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

    private async void OnAddProductClicked(
        object? sender,
        EventArgs e)
    {
        try
        {
            string name =
                ProductNameEntry.Text?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Please enter a product name.",
                    "OK");

                return;
            }

            string grade =
                ProductGradePicker.SelectedItem
                    ?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(grade))
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Please select an egg grade.",
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
                    "Enter a valid eggs-per-crate value.",
                    "OK");

                return;
            }

            AddProductButton.IsEnabled = false;

            var product = new Product
            {
                Name = name,
                Grade = grade,
                EggsPerCrate = eggsPerCrate
            };

            await _databaseService.AddProductAsync(
                product);

            await DisplayAlertAsync(
                "Success",
                $"{product.Name} - {product.Grade} was added successfully.",
                "OK");

            ProductNameEntry.Text =
                string.Empty;

            ProductGradePicker.SelectedItem =
                null;

            EggsPerCrateEntry.Text =
                "30";

            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not add product.\n\n{ex.Message}",
                "OK");
        }
        finally
        {
            AddProductButton.IsEnabled = true;
        }
    }

    private async void OnSavePricingClicked(
        object? sender,
        EventArgs e)
    {
        if (ProductPicker.SelectedItem
            is not ProductDisplay product)
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
                ProductId =
                    product.Id,

                RetailPricePerEgg =
                    retailPrice,

                WholesalePricePerCrate =
                    wholesalePrice,

                BulkPricePerCrate =
                    bulkPrice,

                EffectiveFrom =
                    DateTime.UtcNow,

                Grade =
                    product.Grade
            };

            await _databaseService.AddProductPriceAsync(
                price);

            await DisplayAlertAsync(
                "Success",
                $"Pricing saved for {product.Grade} eggs.",
                "OK");

            RetailPriceEntry.Text =
                string.Empty;

            WholesalePriceEntry.Text =
                string.Empty;

            BulkPriceEntry.Text =
                string.Empty;

            ProductPicker.SelectedItem =
                null;

            await LoadPricesAsync();
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

    public string Grade { get; set; } =
        string.Empty;

    public string DisplayName { get; set; } =
        string.Empty;
}

public class PriceDisplay
{
    public int Id { get; set; }

    public string Grade { get; set; } =
        string.Empty;

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

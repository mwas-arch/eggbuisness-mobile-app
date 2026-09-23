using eggs_accounting_app.Models;
using eggs_accounting_app.Services;

namespace eggs_accounting_app.Pages;

public partial class SalesPage : ContentPage
{
    public class ProductDisplay
    {
        public int Id { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public string Grade { get; set; } =
            string.Empty;

        public int EggsPerCrate { get; set; }

        public string DisplayName =>
            $"{Name} - {Grade}";
    }

    private readonly LocalDatabaseService _databaseService;

    public SalesPage(
        LocalDatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;

        PriceTypePicker.SelectedIndex = 0;
        PaymentMethodPicker.SelectedIndex = 0;

        Loaded += async (_, _) =>
        {
            await LoadCustomersAsync();
            await LoadProductsAsync();
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
                        Name = product.Name,
                        Grade = product.Grade,
                        EggsPerCrate = product.EggsPerCrate
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

    private async void OnCreateSaleClicked(
        object? sender,
        EventArgs e)
    {
        try
        {
            if (ProductPicker.SelectedItem
                is not ProductDisplay product)
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Please select a product / egg grade.",
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

            if (crates < 0)
            {
                crates = 0;
            }

            if (looseEggs < 0)
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

            if (amountPaid < 0)
            {
                await DisplayAlertAsync(
                    "Validation",
                    "Amount paid cannot be negative.",
                    "OK");

                return;
            }

            var selectedCustomer =
                CustomerPicker.SelectedItem as Customer;

            var priceType =
                PriceTypePicker.SelectedItem
                    ?.ToString()
                ?? "Retail";

            var request =
                new CreateSaleRequest
                {
                    CustomerId =
                        selectedCustomer?.Id,

                    PaymentMethod =
                        PaymentMethodPicker.SelectedItem
                            ?.ToString()
                        ?? "Cash",

                    AmountPaid =
                        amountPaid,

                    CreatedBy =
                        "Offline User",

                    Items =
                        new List<CreateSaleItemRequest>
                        {
                            new CreateSaleItemRequest
                            {
                                ProductId =
                                    product.Id,

                                QuantityEggs =
                                    quantityEggs,

                                PriceType =
                                    priceType
                            }
                        }
                };

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            CreateSaleButton.IsEnabled = false;

            var sale =
                await _databaseService.CreateSaleAsync(
                    request);

            string customerName =
                selectedCustomer?.Name
                ?? "Walk-in Customer";

            string receipt =
                $"Sale #: {sale.Id}\n\n" +
                $"Customer: {customerName}\n" +
                $"Product: {product.Name}\n" +
                $"Grade: {product.Grade}\n" +
                $"Quantity: {crates} crate(s) + " +
                $"{looseEggs} egg(s)\n" +
                $"Price Type: {priceType}\n\n" +
                $"Total: Ksh {sale.TotalAmount:N2}\n" +
                $"Paid: Ksh {sale.AmountPaid:N2}\n" +
                $"Balance: Ksh {sale.BalanceDue:N2}\n\n" +
                $"Payment: {sale.PaymentMethod}";

            await DisplayAlertAsync(
                "Sale Created Successfully",
                receipt,
                "OK");

            CratesEntry.Text =
                string.Empty;

            LooseEggsEntry.Text =
                string.Empty;

            AmountPaidEntry.Text =
                string.Empty;

            CustomerPicker.SelectedItem =
                null;

            ProductPicker.SelectedItem =
                null;

            CurrentPriceLabel.Text =
                "Current Price: -";

            EstimatedTotalLabel.Text =
                "Estimated Total: Ksh 0.00";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Sale Failed",
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
        await UpdateEstimatedTotal();
    }

    private void OnProductChanged(
        object? sender,
        EventArgs e)
    {
        _ = UpdateEstimatedTotal();
    }

    private void OnQuantityChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        _ = UpdateEstimatedTotal();
    }

    private async Task UpdateEstimatedTotal()
    {
        if (ProductPicker.SelectedItem
            is not ProductDisplay product)
        {
            CurrentPriceLabel.Text =
                "Current Price: -";

            EstimatedTotalLabel.Text =
                "Estimated Total: Ksh 0.00";

            return;
        }

        try
        {
            var price =
                await _databaseService
                    .GetCurrentProductPriceAsync(
                        product.Id);

            if (price == null)
            {
                CurrentPriceLabel.Text =
                    "Current Price: No pricing available";

                EstimatedTotalLabel.Text =
                    "Estimated Total: Ksh 0.00";

                return;
            }

            int.TryParse(
                CratesEntry.Text,
                out int crates);

            int.TryParse(
                LooseEggsEntry.Text,
                out int looseEggs);

            if (crates < 0)
            {
                crates = 0;
            }

            if (looseEggs < 0)
            {
                looseEggs = 0;
            }

            string priceType =
                PriceTypePicker.SelectedItem
                    ?.ToString()
                ?? "Retail";

            decimal estimatedTotal;

            if (priceType == "Wholesale")
            {
                estimatedTotal =
                    (crates *
                        price.WholesalePricePerCrate)
                    +
                    (looseEggs *
                        price.RetailPricePerEgg);

                CurrentPriceLabel.Text =
                    $"Current Price: Ksh " +
                    $"{price.WholesalePricePerCrate:N2} / crate";
            }
            else if (priceType == "Bulk")
            {
                estimatedTotal =
                    (crates *
                        price.BulkPricePerCrate)
                    +
                    (looseEggs *
                        price.RetailPricePerEgg);

                CurrentPriceLabel.Text =
                    $"Current Price: Ksh " +
                    $"{price.BulkPricePerCrate:N2} / crate";
            }
            else
            {
                int totalEggs =
                    (crates *
                        product.EggsPerCrate)
                    +
                    looseEggs;

                estimatedTotal =
                    totalEggs *
                    price.RetailPricePerEgg;

                CurrentPriceLabel.Text =
                    $"Current Price: Ksh " +
                    $"{price.RetailPricePerEgg:N2} / egg";
            }

            EstimatedTotalLabel.Text =
                $"Estimated Total: Ksh " +
                $"{estimatedTotal:N2}";
        }
        catch (Exception ex)
        {
            CurrentPriceLabel.Text =
                $"Could not load price: {ex.Message}";

            EstimatedTotalLabel.Text =
                "Estimated Total: Ksh 0.00";
        }
    }

    private async void OnSalesHistoryClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(SalesHistoryPage));
    }
}


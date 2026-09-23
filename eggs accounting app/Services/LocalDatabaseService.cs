
using eggs_accounting_app.Data;
using eggs_accounting_app.Models;
using Microsoft.EntityFrameworkCore;

namespace eggs_accounting_app.Services;

public class LocalDatabaseService
{
    private readonly IDbContextFactory<EggBusinessDbContext> _dbFactory;

    public LocalDatabaseService(
        IDbContextFactory<EggBusinessDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Suppliers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<InventoryBatch>> GetInventoryAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.InventoryBatches
            .Include(b => b.Supplier)
            .OrderByDescending(b => b.ReceivedDate)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Products
            .Include(p => p.Prices)
            .ToListAsync();
    }

    public async Task<List<ProductPrice>> GetProductPricesAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.ProductPrices
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<ProductPrice?> GetCurrentProductPriceAsync(
        int productId)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.ProductPrices
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Expense>> GetExpensesAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Expenses
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetSalesAsync()
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        return await db.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<Supplier> AddSupplierAsync(
        Supplier supplier)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        db.Suppliers.Add(supplier);

        await db.SaveChangesAsync();

        return supplier;
    }

    public async Task<InventoryBatch> AddInventoryBatchAsync(
        InventoryBatch batch)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        if (batch.ReceivedDate == default)
            batch.ReceivedDate = DateTime.UtcNow;

        batch.EggsRemaining = batch.EggsReceived;

        db.InventoryBatches.Add(batch);

        await db.SaveChangesAsync();

        return batch;
    }

    public async Task<Expense> AddExpenseAsync(
        Expense expense)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        db.Expenses.Add(expense);

        await db.SaveChangesAsync();

        return expense;
    }

    public async Task<Customer> AddCustomerAsync(
        Customer customer)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        db.Customers.Add(customer);

        await db.SaveChangesAsync();

        return customer;
    }

    public async Task<StockAdjustment> AddStockAdjustmentAsync(
        StockAdjustment adjustment)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        var batch =
            await db.InventoryBatches
                .FirstOrDefaultAsync(
                    b => b.Id == adjustment.InventoryBatchId);

        if (batch == null)
        {
            throw new InvalidOperationException(
                "Inventory batch was not found.");
        }

        if (adjustment.QuantityEggs <= 0)
        {
            throw new InvalidOperationException(
                "Adjustment quantity must be greater than zero.");
        }

        if (adjustment.QuantityEggs > batch.EggsRemaining)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. " +
                $"Only {batch.EggsRemaining} eggs remain.");
        }

        batch.EggsRemaining -=
            adjustment.QuantityEggs;

        db.StockAdjustments.Add(adjustment);

        await db.SaveChangesAsync();

        return adjustment;
    }

    public async Task<ProductPrice> AddProductPriceAsync(
        ProductPrice price)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        db.ProductPrices.Add(price);

        await db.SaveChangesAsync();

        return price;
    }

    public async Task<Sale> CreateSaleAsync(
        CreateSaleRequest request)
    {
        if (request.Items == null ||
            request.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "A sale must contain at least one item.");
        }

        await using var db =
            await _dbFactory.CreateDbContextAsync();

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            Customer? customer = null;

            if (request.CustomerId.HasValue)
            {
                customer =
                    await db.Customers
                        .FirstOrDefaultAsync(
                            c => c.Id ==
                                 request.CustomerId.Value);

                if (customer == null)
                {
                    throw new InvalidOperationException(
                        "Selected customer was not found.");
                }
            }

            var sale = new Sale
            {
                CustomerId = request.CustomerId,

                CustomerName =
                    customer?.Name ??
                    "Walk-in Customer",

                PaymentMethod =
                    string.IsNullOrWhiteSpace(
                        request.PaymentMethod)
                        ? "Cash"
                        : request.PaymentMethod,

                AmountPaid =
                    request.AmountPaid,

                SaleDate =
                    DateTime.UtcNow,

                CreatedBy =
                    string.IsNullOrWhiteSpace(
                        request.CreatedBy)
                        ? "Mobile App"
                        : request.CreatedBy
            };

            decimal totalAmount = 0;

            foreach (var requestItem in request.Items)
            {
                if (requestItem.QuantityEggs <= 0)
                {
                    throw new InvalidOperationException(
                        "Sale quantity must be greater than zero.");
                }

                var product =
                    await db.Products
                        .FirstOrDefaultAsync(
                            p => p.Id ==
                                 requestItem.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException(
                        "Selected product was not found.");
                }

                if (product.EggsPerCrate <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid eggs-per-crate setting " +
                        $"for {product.Name}.");
                }

                var price =
                    await db.ProductPrices
                        .Where(p =>
                            p.ProductId == product.Id)
                        .OrderByDescending(
                            p => p.EffectiveFrom)
                        .FirstOrDefaultAsync();

                if (price == null)
                {
                    throw new InvalidOperationException(
                        $"No pricing has been configured " +
                        $"for {product.Name} - " +
                        $"{product.Grade}.");
                }

                int quantityEggs =
                    requestItem.QuantityEggs;

                int crates =
                    quantityEggs /
                    product.EggsPerCrate;

                int looseEggs =
                    quantityEggs %
                    product.EggsPerCrate;

                string priceType =
                    requestItem.PriceType ?? "Retail";

                decimal totalPrice;

                if (priceType == "Wholesale")
                {
                    totalPrice =
                        (crates *
                         price.WholesalePricePerCrate)
                        +
                        (looseEggs *
                         price.RetailPricePerEgg);
                }
                else if (priceType == "Bulk")
                {
                    totalPrice =
                        (crates *
                         price.BulkPricePerCrate)
                        +
                        (looseEggs *
                         price.RetailPricePerEgg);
                }
                else
                {
                    totalPrice =
                        quantityEggs *
                        price.RetailPricePerEgg;
                }

                var batches =
                    await db.InventoryBatches
                        .Where(b =>
                            b.Grade == product.Grade &&
                            b.EggsRemaining > 0)
                        .OrderBy(b => b.ReceivedDate)
                        .ThenBy(b => b.Id)
                        .ToListAsync();

                int availableEggs =
                    batches.Sum(
                        b => b.EggsRemaining);

                if (availableEggs < quantityEggs)
                {
                    throw new InvalidOperationException(
                        $"Insufficient {product.Grade} " +
                        $"egg stock.\n\n" +
                        $"Available: {availableEggs}\n" +
                        $"Required: {quantityEggs}");
                }

                int remainingToDeduct =
                    quantityEggs;

                foreach (var batch in batches)
                {
                    if (remainingToDeduct <= 0)
                        break;

                    int deduction =
                        Math.Min(
                            batch.EggsRemaining,
                            remainingToDeduct);

                    batch.EggsRemaining -=
                        deduction;

                    remainingToDeduct -=
                        deduction;

                    decimal itemTotal;

                    if (priceType == "Retail")
                    {
                        itemTotal =
                            deduction *
                            price.RetailPricePerEgg;
                    }
                    else
                    {
                        itemTotal =
                            CalculateBatchSaleAmount(
                                deduction,
                                product.EggsPerCrate,
                                price,
                                priceType);
                    }

                    var saleItem = new SaleItem
                    {
                        Sale = sale,

                        ProductId =
                            product.Id,

                        InventoryBatchId =
                            batch.Id,

                        QuantityEggs =
                            deduction,

                        UnitPrice =
                            priceType == "Retail"
                                ? price.RetailPricePerEgg
                                : priceType == "Bulk"
                                    ? price.BulkPricePerCrate
                                    : price.WholesalePricePerCrate,

                        TotalPrice =
                            itemTotal
                    };

                    sale.Items.Add(saleItem);
                }

                totalAmount +=
                    totalPrice;
            }

            if (request.AmountPaid < 0)
            {
                throw new InvalidOperationException(
                    "Amount paid cannot be negative.");
            }

            if (request.AmountPaid > totalAmount)
            {
                throw new InvalidOperationException(
                    "Amount paid cannot be greater " +
                    "than the sale total.");
            }

            sale.TotalAmount =
                totalAmount;

            sale.BalanceDue =
                totalAmount -
                request.AmountPaid;

            db.Sales.Add(sale);

            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    private static decimal CalculateBatchSaleAmount(
        int quantityEggs,
        int eggsPerCrate,
        ProductPrice price,
        string priceType)
    {
        int crates =
            quantityEggs /
            eggsPerCrate;

        int looseEggs =
            quantityEggs %
            eggsPerCrate;

        decimal cratePrice =
            priceType == "Bulk"
                ? price.BulkPricePerCrate
                : price.WholesalePricePerCrate;

        return
            (crates * cratePrice)
            +
            (looseEggs *
             price.RetailPricePerEgg);
    }
    public async Task<Product> AddProductAsync(
        Product product)
    {
        await using var db =
            await _dbFactory.CreateDbContextAsync();

        db.Products.Add(product);

        await db.SaveChangesAsync();

        return product;
    }
}

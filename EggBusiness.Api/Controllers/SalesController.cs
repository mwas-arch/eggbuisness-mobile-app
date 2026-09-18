using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public SalesController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Sales
    [HttpGet]
    public async Task<IActionResult> GetSales()
    {
        var sales = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Product)
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new
            {
                s.Id,
                s.CustomerId,
                CustomerName = s.Customer != null
                    ? s.Customer.Name
                    : "Walk-in Customer",

                s.TotalAmount,
                s.AmountPaid,
                s.BalanceDue,
                s.PaymentMethod,
                s.SaleDate,
                s.CreatedBy,

                Items = s.SaleItems.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product!.Name,
                    Grade = i.Product.Grade,
                    i.InventoryBatchId,
                    i.QuantityEggs,
                    Crates = i.QuantityEggs / i.Product.EggsPerCrate,
                    LooseEggs = i.QuantityEggs % i.Product.EggsPerCrate,
                    i.UnitPrice,
                    i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(sales);
    }

    // GET: api/Sales/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSale(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.Product)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.CustomerId,
                CustomerName = s.Customer != null
                    ? s.Customer.Name
                    : "Walk-in Customer",

                s.TotalAmount,
                s.AmountPaid,
                s.BalanceDue,
                s.PaymentMethod,
                s.SaleDate,
                s.CreatedBy,

                Items = s.SaleItems.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product!.Name,
                    Grade = i.Product.Grade,
                    i.InventoryBatchId,
                    i.QuantityEggs,
                    Crates = i.QuantityEggs / i.Product.EggsPerCrate,
                    LooseEggs = i.QuantityEggs % i.Product.EggsPerCrate,
                    i.UnitPrice,
                    i.TotalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (sale == null)
        {
            return NotFound();
        }

        return Ok(sale);
    }

    // POST: api/Sales
    [HttpPost]
    public async Task<IActionResult> CreateSale(
        CreateSaleRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest("A sale must contain at least one item.");
        }

        var validPaymentMethods = new[]
        {
            "Cash",
            "M-Pesa",
            "Bank Transfer",
            "Credit"
        };

        if (!validPaymentMethods.Contains(request.PaymentMethod))
        {
            return BadRequest("Invalid payment method.");
        }

        if (request.AmountPaid < 0)
        {
            return BadRequest("Amount paid cannot be negative.");
        }

        // Validate customer if supplied
        Customer? customer = null;

        if (request.CustomerId.HasValue)
        {
            customer = await _context.Customers
                .FindAsync(request.CustomerId.Value);

            if (customer == null)
            {
                return BadRequest("Customer does not exist.");
            }
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var sale = new Sale
            {
                CustomerId = request.CustomerId,
                PaymentMethod = request.PaymentMethod,
                AmountPaid = request.AmountPaid,
                SaleDate = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            };

            decimal totalAmount = 0;

            foreach (var requestedItem in request.Items)
            {
                if (requestedItem.QuantityEggs <= 0)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        "Sale quantity must be greater than zero.");
                }

                var product = await _context.Products
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(
                        p => p.Id == requestedItem.ProductId);

                if (product == null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        $"Product {requestedItem.ProductId} does not exist.");
                }

                var price = product.Prices
                    .OrderByDescending(p => p.EffectiveFrom)
                    .FirstOrDefault();

                if (price == null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        $"No price has been configured for {product.Grade} eggs.");
                }

                // Find batches using FIFO.
                var batches = await _context.InventoryBatches
                    .Where(b =>
                        b.Grade == product.Grade &&
                        b.EggsRemaining > 0)
                    .OrderBy(b => b.ReceivedDate)
                    .ToListAsync();

                var totalAvailable = batches
                    .Sum(b => b.EggsRemaining);

                if (totalAvailable < requestedItem.QuantityEggs)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        $"Insufficient stock for {product.Grade} eggs. " +
                        $"Available: {totalAvailable}, " +
                        $"Requested: {requestedItem.QuantityEggs}.");
                }

                int remainingToSell =
                    requestedItem.QuantityEggs;

                foreach (var batch in batches)
                {
                    if (remainingToSell <= 0)
                    {
                        break;
                    }

                    var quantityFromBatch =
                        Math.Min(
                            batch.EggsRemaining,
                            remainingToSell);

                    batch.EggsRemaining -= quantityFromBatch;

                    remainingToSell -= quantityFromBatch;

                    // Determine unit price.
                    decimal unitPrice;

                    if (requestedItem.PriceType == "Retail")
                    {
                        unitPrice = price.RetailPricePerEgg;
                    }
                    else if (requestedItem.PriceType == "Wholesale")
                    {
                        unitPrice =
                            price.WholesalePricePerCrate
                            / product.EggsPerCrate;
                    }
                    else if (requestedItem.PriceType == "Bulk")
                    {
                        unitPrice =
                            price.BulkPricePerCrate
                            / product.EggsPerCrate;
                    }
                    else
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(
                            "Invalid price type.");
                    }

                    var itemTotal =
                        unitPrice * quantityFromBatch;

                    var saleItem = new SaleItem
                    {
                        Sale = sale,
                        ProductId = product.Id,
                        InventoryBatchId = batch.Id,
                        QuantityEggs = quantityFromBatch,
                        UnitPrice = unitPrice,
                        TotalPrice = itemTotal
                    };

                    _context.SaleItems.Add(saleItem);

                    totalAmount += itemTotal;
                }
            }

            sale.TotalAmount = totalAmount;
            sale.BalanceDue =
                sale.TotalAmount - sale.AmountPaid;

            if (sale.BalanceDue < 0)
            {
                await transaction.RollbackAsync();

                return BadRequest(
                    "Amount paid cannot exceed the total sale amount.");
            }

            if (request.PaymentMethod == "Credit")
            {
                if (customer == null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        "A customer is required for a credit sale.");
                }

                var existingBalance = await _context.Sales
                    .Where(s =>
                        s.CustomerId == customer.Id &&
                        s.BalanceDue > 0)
                    .SumAsync(s => s.BalanceDue);

                var newBalance =
                    existingBalance + sale.BalanceDue;

                if (newBalance > customer.CreditLimit)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(
                        "This sale would exceed the customer's credit limit.");
                }
            }

            _context.Sales.Add(sale);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetSale),
                new { id = sale.Id },
                new
                {
                    sale.Id,
                    sale.CustomerId,
                    sale.TotalAmount,
                    sale.AmountPaid,
                    sale.BalanceDue,
                    sale.PaymentMethod,
                    sale.SaleDate
                });
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(
                500,
                "An error occurred while processing the sale.");
        }
    }
}


// Request models used by the sales endpoint.

public class CreateSaleRequest
{
    public int? CustomerId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal AmountPaid { get; set; }

    public string? CreatedBy { get; set; }

    public List<CreateSaleItemRequest> Items { get; set; }
        = new();
}


public class CreateSaleItemRequest
{
    public int ProductId { get; set; }

    public int QuantityEggs { get; set; }

    public string PriceType { get; set; } = "Retail";
}
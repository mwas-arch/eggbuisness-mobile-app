using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPricesController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public ProductPricesController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/ProductPrices
    [HttpGet]
    public async Task<IActionResult> GetPrices()
    {
        var prices = await _context.ProductPrices
            .Include(p => p.Product)
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new
            {
                p.Id,
                p.ProductId,
                ProductName = p.Product!.Name,
                Grade = p.Product.Grade,
                p.RetailPricePerEgg,
                p.WholesalePricePerCrate,
                p.BulkPricePerCrate,
                p.EffectiveFrom
            })
            .ToListAsync();

        return Ok(prices);
    }

    // GET: api/ProductPrices/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPrice(int id)
    {
        var price = await _context.ProductPrices
            .Include(p => p.Product)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.ProductId,
                ProductName = p.Product!.Name,
                Grade = p.Product.Grade,
                p.RetailPricePerEgg,
                p.WholesalePricePerCrate,
                p.BulkPricePerCrate,
                p.EffectiveFrom
            })
            .FirstOrDefaultAsync();

        if (price == null)
        {
            return NotFound();
        }

        return Ok(price);
    }

    // POST: api/ProductPrices
    [HttpPost]
    public async Task<IActionResult> CreatePrice(
        ProductPrice price)
    {
        var product = await _context.Products
            .FindAsync(price.ProductId);

        if (product == null)
        {
            return BadRequest("Product does not exist.");
        }

        if (price.RetailPricePerEgg < 0)
        {
            return BadRequest(
                "Retail price cannot be negative.");
        }

        if (price.WholesalePricePerCrate < 0)
        {
            return BadRequest(
                "Wholesale price cannot be negative.");
        }

        if (price.BulkPricePerCrate < 0)
        {
            return BadRequest(
                "Bulk price cannot be negative.");
        }

        price.Id = 0;
        price.EffectiveFrom = DateTime.UtcNow;

        _context.ProductPrices.Add(price);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPrice),
            new { id = price.Id },
            new
            {
                price.Id,
                price.ProductId,
                ProductName = product.Name,
                Grade = product.Grade,
                price.RetailPricePerEgg,
                price.WholesalePricePerCrate,
                price.BulkPricePerCrate,
                price.EffectiveFrom
            });
    }

    // DELETE: api/ProductPrices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrice(int id)
    {
        var price = await _context.ProductPrices
            .FindAsync(id);

        if (price == null)
        {
            return NotFound();
        }

        _context.ProductPrices.Remove(price);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
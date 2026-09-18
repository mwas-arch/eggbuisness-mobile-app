using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public ProductsController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Products
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Prices)
            .OrderBy(p => p.Grade)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Grade,
                p.EggsPerCrate,

                Prices = p.Prices
                    .OrderByDescending(price => price.EffectiveFrom)
                    .Select(price => new
                    {
                        price.Id,
                        price.RetailPricePerEgg,
                        price.WholesalePricePerCrate,
                        price.BulkPricePerCrate,
                        price.EffectiveFrom
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Prices)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Grade,
                p.EggsPerCrate,

                Prices = p.Prices
                    .OrderByDescending(price => price.EffectiveFrom)
                    .Select(price => new
                    {
                        price.Id,
                        price.RetailPricePerEgg,
                        price.WholesalePricePerCrate,
                        price.BulkPricePerCrate,
                        price.EffectiveFrom
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // POST: api/Products
    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Grade))
        {
            return BadRequest("Egg grade is required.");
        }

        if (product.EggsPerCrate <= 0)
        {
            return BadRequest(
                "Eggs per crate must be greater than zero.");
        }

        var existingProduct = await _context.Products
            .AnyAsync(p => p.Grade == product.Grade);

        if (existingProduct)
        {
            return BadRequest(
                $"A product for grade '{product.Grade}' already exists.");
        }

        product.Id = 0;

        // Prices are managed separately.
        product.Prices = new List<ProductPrice>();

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            new
            {
                product.Id,
                product.Name,
                product.Grade,
                product.EggsPerCrate
            });
    }

    // PUT: api/Products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(
                "The ID in the URL does not match the product ID.");
        }

        var existingProduct = await _context.Products
            .FindAsync(id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Grade))
        {
            return BadRequest("Egg grade is required.");
        }

        if (product.EggsPerCrate <= 0)
        {
            return BadRequest(
                "Eggs per crate must be greater than zero.");
        }

        var duplicateGrade = await _context.Products
            .AnyAsync(p =>
                p.Id != id &&
                p.Grade == product.Grade);

        if (duplicateGrade)
        {
            return BadRequest(
                $"A product for grade '{product.Grade}' already exists.");
        }

        existingProduct.Name = product.Name;
        existingProduct.Grade = product.Grade;
        existingProduct.EggsPerCrate = product.EggsPerCrate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
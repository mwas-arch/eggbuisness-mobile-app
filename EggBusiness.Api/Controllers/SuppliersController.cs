using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public SuppliersController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Suppliers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
    {
        return await _context.Suppliers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    // GET: api/Suppliers/5 
    [HttpGet("{id}")]
    public async Task<ActionResult<Supplier>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.InventoryBatches)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            return NotFound();
        }

        return supplier;
    }

    // POST: api/Suppliers
    [HttpPost]
    public async Task<ActionResult<Supplier>> CreateSupplier(Supplier supplier)
    {
        supplier.Id = 0;
        supplier.CreatedAt = DateTime.UtcNow;

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.Id },
            supplier);
    }

    // PUT: api/Suppliers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(
        int id,
        Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return BadRequest();
        }

        var existingSupplier = await _context.Suppliers
            .FindAsync(id);

        if (existingSupplier == null)
        {
            return NotFound();
        }

        existingSupplier.Name = supplier.Name;
        existingSupplier.Phone = supplier.Phone;
        existingSupplier.Email = supplier.Email;
        existingSupplier.Address = supplier.Address;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Suppliers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

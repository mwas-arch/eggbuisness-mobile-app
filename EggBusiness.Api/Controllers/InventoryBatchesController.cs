
using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryBatchesController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public InventoryBatchesController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/InventoryBatches
    [HttpGet]
    public async Task<IActionResult> GetBatches()
    {
        var batches = await _context.InventoryBatches
            .Include(b => b.Supplier)
            .OrderByDescending(b => b.ReceivedDate)
            .Select(b => new
            {
                b.Id,
                b.SupplierId,
                SupplierName = b.Supplier!.Name,
                b.Grade,
                b.EggsReceived,
                b.EggsRemaining,
                b.EggsPerCrate,
                b.CostPerCrate,
                b.ReceivedDate,

                RemainingCrates =
                    b.EggsRemaining / b.EggsPerCrate,

                RemainingLooseEggs =
                    b.EggsRemaining % b.EggsPerCrate
            })
            .ToListAsync();

        return Ok(batches);
    }

    // GET: api/InventoryBatches/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBatch(int id)
    {
        var batch = await _context.InventoryBatches
            .Include(b => b.Supplier)
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.SupplierId,
                SupplierName = b.Supplier!.Name,
                b.Grade,
                b.EggsReceived,
                b.EggsRemaining,
                b.EggsPerCrate,
                b.CostPerCrate,
                b.ReceivedDate,

                RemainingCrates =
                    b.EggsRemaining / b.EggsPerCrate,

                RemainingLooseEggs =
                    b.EggsRemaining % b.EggsPerCrate
            })
            .FirstOrDefaultAsync();

        if (batch == null)
        {
            return NotFound();
        }

        return Ok(batch);
    }

    // POST: api/InventoryBatches
    [HttpPost]
    public async Task<IActionResult> CreateBatch(
        InventoryBatch batch)
    {
        // Check that the supplier exists
        var supplierExists = await _context.Suppliers
            .AnyAsync(s => s.Id == batch.SupplierId);

        if (!supplierExists)
        {
            return BadRequest("Supplier does not exist.");
        }

        // Validate eggs received
        if (batch.EggsReceived <= 0)
        {
            return BadRequest(
                "Eggs received must be greater than zero.");
        }

        // Validate eggs per crate
        if (batch.EggsPerCrate <= 0)
        {
            return BadRequest(
                "Eggs per crate must be greater than zero.");
        }

        // Validate cost
        if (batch.CostPerCrate < 0)
        {
            return BadRequest(
                "Cost per crate cannot be negative.");
        }

        // Let SQL Server generate the ID
        batch.Id = 0;

        // Store individual eggs as the atomic inventory unit
        batch.EggsRemaining = batch.EggsReceived;

        // Set the receiving date automatically
        if (batch.ReceivedDate == default)
        {
            batch.ReceivedDate = DateTime.UtcNow;
        }

        _context.InventoryBatches.Add(batch);

        await _context.SaveChangesAsync();

        // Return a clean response instead of the complete
        // EF entity, preventing circular JSON references.
        return CreatedAtAction(
            nameof(GetBatch),
            new { id = batch.Id },
            new
            {
                batch.Id,
                batch.SupplierId,
                batch.Grade,
                batch.EggsReceived,
                batch.EggsRemaining,
                batch.EggsPerCrate,
                batch.CostPerCrate,
                batch.ReceivedDate,

                RemainingCrates =
                    batch.EggsRemaining / batch.EggsPerCrate,

                RemainingLooseEggs =
                    batch.EggsRemaining % batch.EggsPerCrate
            });
    }

    // PUT: api/InventoryBatches/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBatch(
        int id,
        InventoryBatch batch)
    {
        if (id != batch.Id)
        {
            return BadRequest(
                "The ID in the URL does not match the batch ID.");
        }

        var existingBatch = await _context.InventoryBatches
            .FindAsync(id);

        if (existingBatch == null)
        {
            return NotFound();
        }

        // Validate supplier
        var supplierExists = await _context.Suppliers
            .AnyAsync(s => s.Id == batch.SupplierId);

        if (!supplierExists)
        {
            return BadRequest("Supplier does not exist.");
        }

        // Validate values
        if (batch.EggsReceived <= 0)
        {
            return BadRequest(
                "Eggs received must be greater than zero.");
        }

        if (batch.EggsPerCrate <= 0)
        {
            return BadRequest(
                "Eggs per crate must be greater than zero.");
        }

        if (batch.CostPerCrate < 0)
        {
            return BadRequest(
                "Cost per crate cannot be negative.");
        }

        existingBatch.SupplierId = batch.SupplierId;
        existingBatch.Grade = batch.Grade;
        existingBatch.EggsReceived = batch.EggsReceived;
        existingBatch.EggsPerCrate = batch.EggsPerCrate;
        existingBatch.CostPerCrate = batch.CostPerCrate;

        // Recalculate remaining stock.
        // This is appropriate while we have not yet implemented
        // sales and stock adjustments.
        existingBatch.EggsRemaining = batch.EggsReceived;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/InventoryBatches/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBatch(int id)
    {
        var batch = await _context.InventoryBatches
            .FindAsync(id);

        if (batch == null)
        {
            return NotFound();
        }

        _context.InventoryBatches.Remove(batch);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

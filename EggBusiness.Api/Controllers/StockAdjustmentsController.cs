using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public StockAdjustmentsController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/StockAdjustments
    [HttpGet]
    public async Task<IActionResult> GetAdjustments()
    {
        var adjustments = await _context.StockAdjustments
            .Include(a => a.InventoryBatch)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new
            {
                a.Id,
                a.InventoryBatchId,
                BatchGrade = a.InventoryBatch!.Grade,
                a.QuantityEggs,
                a.Reason,
                a.LoggedBy,
                a.Timestamp
            })
            .ToListAsync();

        return Ok(adjustments);
    }

    // GET: api/StockAdjustments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdjustment(int id)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.InventoryBatch)
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.InventoryBatchId,
                BatchGrade = a.InventoryBatch!.Grade,
                a.QuantityEggs,
                a.Reason,
                a.LoggedBy,
                a.Timestamp
            })
            .FirstOrDefaultAsync();

        if (adjustment == null)
        {
            return NotFound();
        }

        return Ok(adjustment);
    }

    // POST: api/StockAdjustments
    [HttpPost]
    public async Task<IActionResult> CreateAdjustment(
        StockAdjustment adjustment)
    {
        var batch = await _context.InventoryBatches
            .FirstOrDefaultAsync(
                b => b.Id == adjustment.InventoryBatchId);

        if (batch == null)
        {
            return BadRequest("Inventory batch does not exist.");
        }

        if (adjustment.QuantityEggs <= 0)
        {
            return BadRequest(
                "Quantity of eggs must be greater than zero.");
        }

        if (adjustment.QuantityEggs > batch.EggsRemaining)
        {
            return BadRequest(
                "Adjustment quantity cannot exceed remaining stock.");
        }

        var allowedReasons = new[]
        {
            "Transit breakage",
            "Customer return",
            "Spoilage",
            "Internal use"
        };

        if (!allowedReasons.Contains(adjustment.Reason))
        {
            return BadRequest(
                "Invalid adjustment reason.");
        }

        adjustment.Id = 0;
        adjustment.Timestamp = DateTime.UtcNow;

        batch.EggsRemaining -= adjustment.QuantityEggs;

        _context.StockAdjustments.Add(adjustment);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAdjustment),
            new { id = adjustment.Id },
            new
            {
                adjustment.Id,
                adjustment.InventoryBatchId,
                adjustment.QuantityEggs,
                adjustment.Reason,
                adjustment.LoggedBy,
                adjustment.Timestamp,
                RemainingEggs = batch.EggsRemaining,
                RemainingCrates =
                    batch.EggsRemaining / batch.EggsPerCrate,
                RemainingLooseEggs =
                    batch.EggsRemaining % batch.EggsPerCrate
            });
    }

    // DELETE: api/StockAdjustments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdjustment(int id)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (adjustment == null)
        {
            return NotFound();
        }

        var batch = await _context.InventoryBatches
            .FirstOrDefaultAsync(
                b => b.Id == adjustment.InventoryBatchId);

        if (batch != null)
        {
            batch.EggsRemaining += adjustment.QuantityEggs;
        }

        _context.StockAdjustments.Remove(adjustment);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
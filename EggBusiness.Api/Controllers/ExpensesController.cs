using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public ExpensesController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Expenses
    [HttpGet]
    public async Task<IActionResult> GetExpenses()
    {
        var expenses = await _context.Expenses
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new
            {
                e.Id,
                e.Category,
                e.Amount,
                e.Description,
                e.ExpenseDate,
                e.CreatedBy
            })
            .ToListAsync();

        return Ok(expenses);
    }

    // GET: api/Expenses/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpense(int id)
    {
        var expense = await _context.Expenses
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.Category,
                e.Amount,
                e.Description,
                e.ExpenseDate,
                e.CreatedBy
            })
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return NotFound();
        }

        return Ok(expense);
    }

    // POST: api/Expenses
    [HttpPost]
    public async Task<IActionResult> CreateExpense(
        Expense expense)
    {
        if (string.IsNullOrWhiteSpace(expense.Category))
        {
            return BadRequest("Expense category is required.");
        }

        if (expense.Amount <= 0)
        {
            return BadRequest(
                "Expense amount must be greater than zero.");
        }

        expense.Id = 0;
        expense.ExpenseDate = DateTime.UtcNow;

        _context.Expenses.Add(expense);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetExpense),
            new { id = expense.Id },
            new
            {
                expense.Id,
                expense.Category,
                expense.Amount,
                expense.Description,
                expense.ExpenseDate,
                expense.CreatedBy
            });
    }

    // PUT: api/Expenses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(
        int id,
        Expense expense)
    {
        if (id != expense.Id)
        {
            return BadRequest(
                "The ID in the URL does not match the expense ID.");
        }

        var existingExpense = await _context.Expenses
            .FindAsync(id);

        if (existingExpense == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(expense.Category))
        {
            return BadRequest("Expense category is required.");
        }

        if (expense.Amount <= 0)
        {
            return BadRequest(
                "Expense amount must be greater than zero.");
        }

        existingExpense.Category = expense.Category;
        existingExpense.Amount = expense.Amount;
        existingExpense.Description = expense.Description;
        existingExpense.CreatedBy = expense.CreatedBy;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Expenses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses
            .FindAsync(id);

        if (expense == null)
        {
            return NotFound();
        }

        _context.Expenses.Remove(expense);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
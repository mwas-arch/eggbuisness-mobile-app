using EggBusiness.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public ReportsController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Reports/ProfitAndLoss
    [HttpGet("ProfitAndLoss")]
    public async Task<IActionResult> GetProfitAndLoss(
        DateTime? from,
        DateTime? to)
    {
        var startDate = from?.Date
            ?? DateTime.UtcNow.Date.AddDays(-30);

        var endDate = to?.Date.AddDays(1)
            ?? DateTime.UtcNow.Date.AddDays(1);

        var sales = await _context.Sales
            .Where(s =>
                s.SaleDate >= startDate &&
                s.SaleDate < endDate)
            .Select(s => new
            {
                s.TotalAmount,
                Items = s.SaleItems.Select(i => new
                {
                    i.QuantityEggs,
                    i.InventoryBatchId,
                    i.InventoryBatch!.CostPerCrate,
                    i.InventoryBatch.EggsPerCrate
                })
            })
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e =>
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .ToListAsync();

        decimal revenue = sales.Sum(s => s.TotalAmount);

        decimal costOfGoodsSold = sales
            .SelectMany(s => s.Items)
            .Sum(i =>
                i.QuantityEggs *
                (i.CostPerCrate / i.EggsPerCrate));

        decimal totalExpenses = expenses.Sum(e => e.Amount);

        decimal grossProfit =
            revenue - costOfGoodsSold;

        decimal netProfit =
            grossProfit - totalExpenses;

        return Ok(new
        {
            Period = new
            {
                From = startDate,
                To = endDate.AddDays(-1)
            },

            Revenue = revenue,

            CostOfGoodsSold = costOfGoodsSold,

            GrossProfit = grossProfit,

            Expenses = totalExpenses,

            NetProfit = netProfit,

            SalesCount = sales.Count,

            ExpenseCount = expenses.Count
        });
    }

    // GET: api/Reports/Dashboard
    [HttpGet("Dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalEggs = await _context.InventoryBatches
            .SumAsync(b => b.EggsRemaining);

        var totalSales = await _context.Sales
            .SumAsync(s => s.TotalAmount);

        var outstandingCredit = await _context.Sales
            .SumAsync(s => s.BalanceDue);

        var totalExpenses = await _context.Expenses
            .SumAsync(e => e.Amount);

        var today = DateTime.UtcNow.Date;

        var todaySales = await _context.Sales
            .Where(s => s.SaleDate >= today)
            .SumAsync(s => s.TotalAmount);

        var todayExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= today)
            .SumAsync(e => e.Amount);

        return Ok(new
        {
            Inventory = new
            {
                TotalEggs = totalEggs,
                Crates = totalEggs / 30,
                LooseEggs = totalEggs % 30
            },

            Sales = new
            {
                TotalSales = totalSales,
                TodaySales = todaySales,
                OutstandingCredit = outstandingCredit
            },

            Expenses = new
            {
                TotalExpenses = totalExpenses,
                TodayExpenses = todayExpenses
            },

            Profit = new
            {
                Today = todaySales - todayExpenses,
                Overall = totalSales - totalExpenses
            }
        });
    }
}
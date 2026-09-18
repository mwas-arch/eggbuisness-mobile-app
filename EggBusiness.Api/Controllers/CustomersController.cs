using EggBusiness.Api.Data;
using EggBusiness.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EggBusiness.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly EggBusinessDbContext _context;

    public CustomersController(EggBusinessDbContext context)
    {
        _context = context;
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _context.Customers
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.Address,
                c.CreditLimit,
                TotalSales = c.Sales.Count(),
                OutstandingBalance = c.Sales
                    .Sum(s => s.BalanceDue)
            })
            .ToListAsync();

        return Ok(customers);
    }

    // GET: api/Customers/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.Address,
                c.CreditLimit,

                Sales = c.Sales
                    .OrderByDescending(s => s.SaleDate)
                    .Select(s => new
                    {
                        s.Id,
                        s.TotalAmount,
                        s.AmountPaid,
                        s.BalanceDue,
                        s.PaymentMethod,
                        s.SaleDate
                    })
                    .ToList(),

                OutstandingBalance = c.Sales
                    .Sum(s => s.BalanceDue)
            })
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<IActionResult> CreateCustomer(
        Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            return BadRequest("Customer name is required.");
        }

        if (customer.CreditLimit < 0)
        {
            return BadRequest(
                "Credit limit cannot be negative.");
        }

        customer.Id = 0;

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            new
            {
                customer.Id,
                customer.Name,
                customer.Phone,
                customer.Address,
                customer.CreditLimit
            });
    }

    // PUT: api/Customers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(
        int id,
        Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest(
                "The ID in the URL does not match the customer ID.");
        }

        var existingCustomer = await _context.Customers
            .FindAsync(id);

        if (existingCustomer == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            return BadRequest("Customer name is required.");
        }

        if (customer.CreditLimit < 0)
        {
            return BadRequest(
                "Credit limit cannot be negative.");
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Address = customer.Address;
        existingCustomer.CreditLimit = customer.CreditLimit;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Customers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers
            .FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        var hasSales = await _context.Sales
            .AnyAsync(s => s.CustomerId == id);

        if (hasSales)
        {
            return BadRequest(
                "This customer cannot be deleted because they have sales records.");
        }

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    // GET: api/Customers/5/account
    [HttpGet("{id}/account")]
    public async Task<IActionResult> GetCustomerAccount(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound("Customer does not exist.");
        }

        var sales = await _context.Sales
            .Where(s => s.CustomerId == id)
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new
            {
                s.Id,
                s.TotalAmount,
                s.AmountPaid,
                s.BalanceDue,
                s.PaymentMethod,
                s.SaleDate
            })
            .ToListAsync();

        var totalPurchases = sales.Sum(s => s.TotalAmount);
        var totalPaid = sales.Sum(s => s.AmountPaid);
        var outstandingBalance = sales.Sum(s => s.BalanceDue);

        var availableCredit =
            customer.CreditLimit - outstandingBalance;

        return Ok(new
        {
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Address,
            customer.CreditLimit,

            TotalPurchases = totalPurchases,
            TotalPaid = totalPaid,
            OutstandingBalance = outstandingBalance,
            AvailableCredit = availableCredit,

            Sales = sales
        });
    }
}
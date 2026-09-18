namespace eggs_accounting_app.Models;

public class DashboardData
{
    public InventorySummary Inventory { get; set; } = new();

    public SalesSummary Sales { get; set; } = new();

    public ExpenseSummary Expenses { get; set; } = new();

    public ProfitSummary Profit { get; set; } = new();
}

public class InventorySummary
{
    public int TotalEggs { get; set; }

    public int Crates { get; set; }

    public int LooseEggs { get; set; }
}

public class SalesSummary
{
    public decimal TotalSales { get; set; }

    public decimal TodaySales { get; set; }

    public decimal OutstandingCredit { get; set; }
}

public class ExpenseSummary
{
    public decimal TotalExpenses { get; set; }

    public decimal TodayExpenses { get; set; }
}

public class ProfitSummary
{
    public decimal Today { get; set; }

    public decimal Overall { get; set; }
}
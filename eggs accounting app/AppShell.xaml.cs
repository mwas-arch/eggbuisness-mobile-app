using eggs_accounting_app.Pages;

namespace eggs_accounting_app;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(SuppliersPage),
            typeof(SuppliersPage));
        
        Routing.RegisterRoute(
            nameof(InventoryPage),
            typeof(InventoryPage));
        Routing.RegisterRoute(
            nameof(SalesPage),
            typeof(SalesPage)); 
        Routing.RegisterRoute(
            nameof(SalesHistoryPage),
            typeof(SalesHistoryPage));
        Routing.RegisterRoute(
            nameof(ExpensesPage),
            typeof(ExpensesPage));

        Routing.RegisterRoute(
            nameof(ExpenseHistoryPage),
            typeof(ExpenseHistoryPage));
        Routing.RegisterRoute(
            nameof(CustomersPage),
            typeof(CustomersPage));
        Routing.RegisterRoute(
            nameof(CustomerAccountPage),
            typeof(CustomerAccountPage));
        Routing.RegisterRoute(
            nameof(StockAdjustmentsPage),
            typeof(StockAdjustmentsPage));
        Routing.RegisterRoute(
            nameof(ProductPricesPage),
            typeof(ProductPricesPage));
        
    }
}
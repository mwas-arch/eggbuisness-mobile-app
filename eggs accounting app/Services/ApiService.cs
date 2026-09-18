using System.Net.Http.Json;
using eggs_accounting_app.Models;

namespace eggs_accounting_app.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://192.168.0.2:5194/")
        };
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Supplier>>(
            "api/Suppliers") ?? new List<Supplier>();
    }

    public async Task<List<InventoryBatch>> GetInventoryAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<InventoryBatch>>(
            "api/InventoryBatches") ?? new List<InventoryBatch>();
    }
    public async Task<HttpResponseMessage> CreateInventoryBatchAsync(
        CreateInventoryBatchRequest request)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/InventoryBatches",
            request);
    }
    public async Task<List<Sale>> GetSalesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Sale>>(
            "api/Sales") ?? new List<Sale>();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Product>>(
            "api/Products") ?? new List<Product>();
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Customer>>(
            "api/Customers") ?? new List<Customer>();
    }

    public async Task<HttpResponseMessage> CreateSaleAsync(
        CreateSaleRequest request)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/Sales",
            request);
    }

    public async Task<List<Expense>> GetExpensesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Expense>>(
            "api/Expenses") ?? new List<Expense>();
    }

    public async Task<HttpResponseMessage> CreateExpenseAsync(
        Expense expense)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/Expenses",
            expense);
    }

    public async Task<DashboardData?> GetDashboardAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardData>(
            "api/Reports/dashboard");
    }

    public async Task<CustomerAccount?> GetCustomerAccountAsync(
        int customerId)
    {
        return await _httpClient.GetFromJsonAsync<CustomerAccount>(
            $"api/Customers/{customerId}/account");
    }

    public async Task<HttpResponseMessage> CreateCustomerAsync(
        Customer customer)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/Customers",
            customer);
    }

    public async Task<HttpResponseMessage> CreateStockAdjustmentAsync(
        StockAdjustment adjustment)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/StockAdjustments",
            adjustment);
    }

    public async Task<List<ProductPrice>> GetProductPricesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProductPrice>>(
            "api/ProductPrices") ?? new List<ProductPrice>();
    }

    public async Task<HttpResponseMessage> CreateProductPriceAsync(
        ProductPrice price)
    {
        return await _httpClient.PostAsJsonAsync(
            "api/ProductPrices",
            price);
    }

    public async Task<ProductPrice?> GetProductPriceAsync(
        int productId)
    {
        var prices = await GetProductPricesAsync();

        return prices
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefault();
    }
}
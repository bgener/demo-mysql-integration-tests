using FluentAssertions;
using OrderSystem.Tests.Infrastructure;
using Xunit;

namespace OrderSystem.Tests.Optimized;

[Trait("Category", "Optimized")]
[Collection("Optimized")]
public class OptimizedCustomerTests(OptimizedMySqlContainerFixture fixture) : MySqlTestBase(fixture)
{
    [Fact]
    public async Task CreateCustomer_StoresEmail()
    {
        var repo = new OrderRepository(ConnectionString);

        var customerId = await repo.CreateCustomerAsync("Alice", "alice@example.com");

        var email = await QueryScalarAsync<string>($"SELECT email FROM Customers WHERE id = {customerId}");
        email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task CreateOrder_WithCustomer_LinksCorrectly()
    {
        var productId = await SeedProductAsync("Widget", price: 10.00m, stock: 100);
        var repo = new OrderRepository(ConnectionString);

        var customerId = await repo.CreateCustomerAsync("Bob", "bob@example.com");
        var orderId = await repo.CreateOrderAsync(productId, quantity: 2, unitPrice: 10.00m, customerId: customerId);

        var total = await QueryScalarAsync<decimal>($"SELECT totalAmount FROM Orders WHERE id = {orderId}");
        total.Should().Be(20.00m);
    }
}

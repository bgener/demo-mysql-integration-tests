using FluentAssertions;
using OrderSystem.Tests.Infrastructure;
using Xunit;

namespace OrderSystem.Tests.Optimized;

[Trait("Category", "Optimized")]
[Collection("Optimized")]
public class OptimizedOrderTotalTests(OptimizedMySqlContainerFixture fixture) : MySqlTestBase(fixture)
{
    [Fact]
    public async Task CreateOrder_CalculatesTotal()
    {
        var productId = await SeedProductAsync("Widget", price: 10.00m, stock: 100);
        var repo = new OrderRepository(ConnectionString);

        var orderId = await repo.CreateOrderAsync(productId, quantity: 2, unitPrice: 10.00m);

        var total = await QueryScalarAsync<decimal>($"SELECT totalAmount FROM Orders WHERE id = {orderId}");
        total.Should().Be(20.00m);
    }

    [Fact]
    public async Task CreateOrder_CalculatesTotal_WithDecimalPrecision()
    {
        var productId = await SeedProductAsync("Gadget", price: 9.99m, stock: 100);
        var repo = new OrderRepository(ConnectionString);

        var orderId = await repo.CreateOrderAsync(productId, quantity: 3, unitPrice: 9.99m);

        var total = await QueryScalarAsync<decimal>($"SELECT totalAmount FROM Orders WHERE id = {orderId}");
        total.Should().Be(29.97m);
    }

    [Fact]
    public async Task CreateOrder_WithSingleItem_CalculatesCorrectTotal()
    {
        var productId = await SeedProductAsync("Pen", price: 4.95m, stock: 500);
        var repo = new OrderRepository(ConnectionString);

        var orderId = await repo.CreateOrderAsync(productId, quantity: 1, unitPrice: 4.95m);

        var total = await QueryScalarAsync<decimal>($"SELECT totalAmount FROM Orders WHERE id = {orderId}");
        total.Should().Be(4.95m);
    }
}

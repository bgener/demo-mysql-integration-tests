using FluentAssertions;
using OrderSystem.Tests.Infrastructure;
using Xunit;

namespace OrderSystem.Tests.Simple;

[Trait("Category", "Simple")]
[Collection("Simple")]
public class SimpleStockTriggerTests(SimpleMySqlContainerFixture fixture) : MySqlTestBase(fixture)
{
    [Fact]
    public async Task CreateOrder_DecrementsStock()
    {
        var productId = await SeedProductAsync("Gadget", price: 10.00m, stock: 50);
        var repo = new OrderRepository(ConnectionString);

        await repo.CreateOrderAsync(productId, quantity: 5, unitPrice: 10.00m);

        var stock = await QueryScalarAsync<int>($"SELECT stock FROM Products WHERE id = {productId}");
        stock.Should().Be(45);
    }

    [Fact]
    public async Task MultipleOrders_DecrementStock_Correctly()
    {
        var productId = await SeedProductAsync("Bolt", price: 1.50m, stock: 1000);
        var repo = new OrderRepository(ConnectionString);

        await repo.CreateOrderAsync(productId, quantity: 100, unitPrice: 1.50m);
        await repo.CreateOrderAsync(productId, quantity: 250, unitPrice: 1.50m);

        var stock = await QueryScalarAsync<int>($"SELECT stock FROM Products WHERE id = {productId}");
        stock.Should().Be(650);
    }

    [Fact]
    public async Task CreateOrder_WithZeroQuantity_StockUnchanged()
    {
        var productId = await SeedProductAsync("Thing", price: 5.00m, stock: 20);
        var repo = new OrderRepository(ConnectionString);

        await repo.CreateOrderAsync(productId, quantity: 0, unitPrice: 5.00m);

        var stock = await QueryScalarAsync<int>($"SELECT stock FROM Products WHERE id = {productId}");
        stock.Should().Be(20);
    }
}

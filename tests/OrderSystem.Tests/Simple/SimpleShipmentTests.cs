using FluentAssertions;
using OrderSystem.Tests.Infrastructure;
using Xunit;

namespace OrderSystem.Tests.Simple;

[Trait("Category", "Simple")]
[Collection("Simple")]
public class SimpleShipmentTests(SimpleMySqlContainerFixture fixture) : MySqlTestBase(fixture)
{
    [Fact]
    public async Task CreateShipment_DefaultStatusIsPending()
    {
        var productId = await SeedProductAsync("Widget", price: 10.00m, stock: 100);
        var repo = new OrderRepository(ConnectionString);

        var orderId = await repo.CreateOrderAsync(productId, quantity: 1, unitPrice: 10.00m);
        var shipmentId = await repo.CreateShipmentAsync(orderId);

        var status = await QueryScalarAsync<string>($"SELECT status FROM Shipments WHERE id = {shipmentId}");
        status.Should().Be("pending");
    }

    [Fact]
    public async Task ShipOrder_UpdatesStatusToShipped()
    {
        var productId = await SeedProductAsync("Gadget", price: 25.00m, stock: 50);
        var repo = new OrderRepository(ConnectionString);

        var orderId = await repo.CreateOrderAsync(productId, quantity: 1, unitPrice: 25.00m);
        var shipmentId = await repo.CreateShipmentAsync(orderId);
        await repo.ShipOrderAsync(shipmentId, "TRACK-12345");

        var status = await QueryScalarAsync<string>($"SELECT status FROM Shipments WHERE id = {shipmentId}");
        status.Should().Be("shipped");
    }
}

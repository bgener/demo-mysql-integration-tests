using Dapper;
using MySqlConnector;

namespace OrderSystem;

public class OrderRepository(string connectionString)
{
    public async Task<int> CreateOrderAsync(int productId, int quantity, decimal unitPrice, int? customerId = null)
    {
        await using var connection = new MySqlConnection(connectionString);

        var orderId = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Orders (productId, customerId, quantity, unitPrice)
              VALUES (@productId, @customerId, @quantity, @unitPrice);
              SELECT LAST_INSERT_ID();",
            new { productId, customerId, quantity, unitPrice });

        // Call stored procedure to calculate order total
        await connection.ExecuteAsync("CALL sp_calculateOrderTotal(@id);", new { id = orderId });

        return orderId;
    }

    public async Task<decimal?> GetOrderTotalAsync(int orderId)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<decimal?>(
            "SELECT totalAmount FROM Orders WHERE id = @id;",
            new { id = orderId });
    }

    public async Task<int> GetProductStockAsync(int productId)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT stock FROM Products WHERE id = @id;",
            new { id = productId });
    }

    public async Task<int> CreateCustomerAsync(string name, string email)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Customers (name, email) VALUES (@name, @email);
              SELECT LAST_INSERT_ID();",
            new { name, email });
    }

    public async Task<string?> GetCustomerEmailAsync(int customerId)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<string?>(
            "SELECT email FROM Customers WHERE id = @id;",
            new { id = customerId });
    }

    public async Task<int> CreateShipmentAsync(int orderId)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Shipments (orderId) VALUES (@orderId);
              SELECT LAST_INSERT_ID();",
            new { orderId });
    }

    public async Task ShipOrderAsync(int shipmentId, string trackingNumber)
    {
        await using var connection = new MySqlConnection(connectionString);

        await connection.ExecuteAsync(
            @"UPDATE Shipments SET status = 'shipped', trackingNumber = @trackingNumber, shippedAt = NOW()
              WHERE id = @shipmentId;",
            new { shipmentId, trackingNumber });
    }

    public async Task<string?> GetShipmentStatusAsync(int shipmentId)
    {
        await using var connection = new MySqlConnection(connectionString);

        return await connection.ExecuteScalarAsync<string?>(
            "SELECT status FROM Shipments WHERE id = @id;",
            new { id = shipmentId });
    }
}

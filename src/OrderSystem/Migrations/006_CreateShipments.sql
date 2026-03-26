CREATE TABLE Shipments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orderId INT NOT NULL,
    trackingNumber VARCHAR(100) NULL,
    status ENUM('pending', 'shipped', 'delivered') NOT NULL DEFAULT 'pending',
    shippedAt TIMESTAMP NULL,
    deliveredAt TIMESTAMP NULL,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (orderId) REFERENCES Orders(id),
    UNIQUE KEY uk_shipments_order (orderId)
);

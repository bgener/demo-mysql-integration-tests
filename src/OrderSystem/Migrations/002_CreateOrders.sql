CREATE TABLE Orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    productId INT NOT NULL,
    quantity INT NOT NULL,
    unitPrice DECIMAL(10,4) NOT NULL,
    totalAmount DECIMAL(10,2) NULL,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (productId) REFERENCES Products(id)
);

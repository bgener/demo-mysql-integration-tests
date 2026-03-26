CREATE TABLE Customers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_customers_email (email)
);

ALTER TABLE Orders ADD COLUMN customerId INT NULL AFTER productId;
ALTER TABLE Orders ADD CONSTRAINT fk_orders_customer FOREIGN KEY (customerId) REFERENCES Customers(id);

DELIMITER //
CREATE PROCEDURE sp_calculateOrderTotal(IN orderId INT)
BEGIN
    UPDATE Orders
    SET totalAmount = ROUND(quantity * unitPrice, 2)
    WHERE id = orderId;
END//
DELIMITER ;

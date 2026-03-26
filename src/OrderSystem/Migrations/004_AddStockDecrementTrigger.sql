DELIMITER //
CREATE TRIGGER trg_decrementStock
AFTER INSERT ON Orders
FOR EACH ROW
BEGIN
    UPDATE Products
    SET stock = stock - NEW.quantity
    WHERE id = NEW.productId;
END//
DELIMITER ;

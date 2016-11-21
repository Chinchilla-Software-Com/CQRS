
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE ProductsSalesByMonth
	@ProductID INT
AS
BEGIN
	SELECT CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME) AS Date,  [Order Details].Quantity FROM Orders
	INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID
	WHERE [Order Details].ProductID = @ProductID
	GROUP BY CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME), [Order Details].Quantity
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO 
CREATE PROCEDURE MonthlySalesByEmployee
	@EmployeeID INT
AS
BEGIN 
	SET NOCOUNT ON;

		SELECT  Orders.EmployeeID, 
				SUM((Quantity * UnitPrice) - (Quantity * UnitPrice * Discount))  AS EmployeeSales,  
				CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME) AS Date
		FROM [Order Details]
			INNER JOIN Orders ON Orders.OrderID = [Order Details].OrderID
		WHERE Orders.EmployeeID = @EmployeeID
		GROUP BY Orders.EmployeeID, CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME)
	END
GO

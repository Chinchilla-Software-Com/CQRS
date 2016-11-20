SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON 
GO
CREATE PROCEDURE [dbo].[SalesAmounts]
    @EmployeeID INT
AS
BEGIN 
    SET NOCOUNT ON;
     
SELECT AllSales.TotalSales AS TotalSales, EmployeeSales.EmployeeSales AS EmployeeSales, AllSales.Date from   
        (SELECT Sales.Date, SUM(Sales.EmployeeSales) AS TotalSales  FROM
            (
                SELECT  Orders.EmployeeID, 
                        SUM((Quantity * UnitPrice) - (Quantity * UnitPrice * Discount))  AS EmployeeSales,  
                        CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME) AS Date
                FROM [Order Details]
                    INNER JOIN Orders ON Orders.OrderID = [Order Details].OrderID
                GROUP BY Orders.EmployeeID, CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME)
            ) AS Sales 
            GROUP BY Sales.Date
        ) AS AllSales 
    LEFT OUTER JOIN
        (SELECT  Orders.EmployeeID, 
                SUM((Quantity * UnitPrice) - (Quantity * UnitPrice * Discount))  AS EmployeeSales,  
                CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME) AS Date
        FROM [Order Details]
            INNER JOIN Orders ON Orders.OrderID = [Order Details].OrderID
        WHERE Orders.EmployeeID = @EmployeeID
        GROUP BY Orders.EmployeeID, CAST(CONVERT(VARCHAR, DATEPART(YEAR, Orders.OrderDate)) + '-' + CONVERT(VARCHAR, DATEPART(MONTH, Orders.OrderDate)) + '-1'  AS DATETIME)
        ) AS EmployeeSales  
    ON AllSales.Date = EmployeeSales.Date
END

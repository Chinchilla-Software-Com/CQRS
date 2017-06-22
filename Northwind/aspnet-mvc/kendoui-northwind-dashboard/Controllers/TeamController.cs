using KendoUI.Northwind.Dashboard.Controllers;
using KendoUI.Northwind.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Data.SqlClient;

namespace KendoUI.Northwind.Dashboard.Controllers
{
    public class TeamController : Controller
    {
        public ActionResult TeamEfficiency()
        {
            return View();
        }

        public ActionResult GetEmployees()
        {
            var employees = new NorthwindEntities().Employees.Select(e => new EmployeeViewModel
            {
                EmployeeID = e.EmployeeID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                EmployeeName = e.FirstName + " " + e.LastName,
                Notes = e.Notes,
                Title = e.Title,
                HomePhone = e.HomePhone
            }).OrderBy(e => e.FirstName);

            return Json(employees, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeesList([DataSourceRequest]DataSourceRequest request)
        {
            var employees = new NorthwindEntities().Employees.Select(e => new EmployeeViewModel
            {
                EmployeeID = e.EmployeeID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                EmployeeName = e.FirstName + " " + e.LastName,
                Notes = e.Notes,
                Title = e.Title,
                HomePhone = e.HomePhone
            }).OrderBy(e => e.FirstName);

            return Json(employees.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeeSales([DataSourceRequest]DataSourceRequest request)
        {
            var northwind = new NorthwindEntities();
            var data = northwind.Orders.Join(northwind.Customers, c => c.CustomerID, o => o.CustomerID, (o, c) => new { Order = o, Customer = c }).ToList();
            
            var sales = data.Select(o => new SaleViewModel
            {
                SaleID = o.Order.OrderID,
                EmployeeID = o.Order.EmployeeID,
                Title = o.Customer.CompanyName,
                Start = DateTime.SpecifyKind((DateTime)o.Order.OrderDate, DateTimeKind.Utc),
                End = DateTime.SpecifyKind((DateTime)o.Order.OrderDate, DateTimeKind.Utc).AddHours(1),
                IsAllDay = false
            });

            return Json(sales.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeeAndTeamSales(int EmployeeID, DateTime startDate, DateTime endDate)
        {
            var northwind = new NorthwindEntities();
            var q1 = (from o in northwind.Orders
                      join od in northwind.Order_Details on o.OrderID equals od.OrderID
                      where o.OrderDate >= startDate && o.OrderDate <= endDate
                      select new
                      {
                          OrderID = o.OrderID,
                          EmployeeID = o.EmployeeID,
                          Date = o.OrderDate,
                          Sales = od.Quantity * od.UnitPrice
                      }).AsEnumerable();
            var q2 = (from allSales in q1
                      group allSales by allSales.OrderID into g
                      select new
                      {
                          OrderID = g.Key,
                          EmployeeID = g.FirstOrDefault().EmployeeID,
                          Sales = g.Sum(x => x.Sales),
                          Date = new DateTime(g.FirstOrDefault().Date.Value.Year, g.FirstOrDefault().Date.Value.Month, 1),
                      });
            var q3 = (from groupedSales in q2
                      group groupedSales by new { groupedSales.EmployeeID, groupedSales.Date } into gs
                      select new
                      {
                          EmployeeID = gs.FirstOrDefault().EmployeeID,
                          Date = gs.Key.Date,
                          Sales = gs.Sum(x => x.Sales)
                      });
            var result = (from totalSales in q3
                          group totalSales by totalSales.Date into gs
                          select new
                          {
                              Date = gs.Key,
                              TotalSales = gs.Sum(x => x.Sales),
                              EmployeeSales = (int?)gs.Where(z => z.EmployeeID == EmployeeID).Sum(z => z.Sales) ?? 0
                          }
            );
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmployeeAverageSales(int EmployeeID, DateTime startDate, DateTime endDate)
        {
            var northwind = new NorthwindEntities();
            var result = (from allSales in
                              (from o in northwind.Orders
                               join od in northwind.Order_Details on o.OrderID equals od.OrderID
                               where o.EmployeeID == EmployeeID && o.OrderDate >= startDate && o.OrderDate <= endDate
                               select new
                               {
                                   EmployeeID = o.EmployeeID,
                                   Date = o.OrderDate,
                                   Sales = od.Quantity * od.UnitPrice
                               }
                                  ).ToList()
                          group allSales by new DateTime(allSales.Date.Value.Year, allSales.Date.Value.Month, 1) into g
                          select new
                          {
                              EmployeeID = g.FirstOrDefault().EmployeeID,
                              EmployeeSales = g.Sum(x => x.Sales),
                              Date = g.Key,
                          }
            );
            
            return Json(result, JsonRequestBehavior.AllowGet); 
        }

        public ActionResult EmployeeQuarterSales(int EmployeeID, DateTime endDate)
        {
            DateTime startDate = endDate.AddMonths(-3);
            var northwind = new NorthwindEntities();
            var sales = northwind.Orders.Where(w => w.EmployeeID == EmployeeID)
                .Join(northwind.Order_Details, orders => orders.OrderID, orderDetails => orderDetails.OrderID, (orders, orderDetails) => new { Order = orders, OrderDetails = orderDetails })
                .Where(d => d.Order.OrderDate >= startDate && d.Order.OrderDate <= endDate).ToList()
                .Select(o => new QuarterToDateSalesViewModel
                {
                    Current = (o.OrderDetails.Quantity * o.OrderDetails.UnitPrice) - (o.OrderDetails.Quantity * o.OrderDetails.UnitPrice * (decimal)o.OrderDetails.Discount)
                });
            //TODO: Generate the target based on team's average sales
            var result = new List<QuarterToDateSalesViewModel>() { 
                     new QuarterToDateSalesViewModel {Current = sales.Sum(s=>s.Current), Target = 15000, OrderDate = endDate}
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}

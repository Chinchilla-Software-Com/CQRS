using Kendo.Mvc.UI;
using KendoUI.Northwind.Dashboard.Models;
using System.Collections.Generic;
using System.Linq; 
using System.Web.Mvc;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using System.Collections;


namespace KendoUI.Northwind.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult ProductsAndOrders()
        {
            ViewData["employees"] = GetEmployees();
            ViewData["customers"] = GetCustomers();
            ViewData["products"] = GetProducts();
            ViewData["shippers"] = GetShippers();
            return View();
        }

        public ActionResult TeamEfficiency()
        {
            return View();
        }

        public ActionResult RegionalSalesStatus()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Products_Read(string text)
        {
            var northwind = new NorthwindEntities();

            var products = northwind.Products.Select(product => new ProductViewModel
            {
                CategoryID = product.CategoryID,
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice ?? 0,
                UnitsInStock = product.UnitsInStock ?? 0,
                UnitsOnOrder = product.UnitsOnOrder ?? 0,
                Discontinued = product.Discontinued
            });

            if (!string.IsNullOrEmpty(text))
            {
                products = products.Where(p => p.ProductName.Contains(text));
            }

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public static IEnumerable<EmployeeViewModel> GetEmployees()
        {
            var employees = new NorthwindEntities().Employees.Select(e => new EmployeeViewModel
            {
                EmployeeID = e.EmployeeID,
                EmployeeName = e.FirstName + " " + e.LastName
            }).OrderBy(e => e.EmployeeName);

            return employees;
        }

        public IEnumerable<ProductViewModel> GetProducts()
        {
            var products = new NorthwindEntities().Products.Select(e => new ProductViewModel
            {
                ProductID = e.ProductID,
                ProductName = e.ProductName
            }).OrderBy(e => e.ProductName);

            return products;
        }


        public static IQueryable<CustomerViewModel> GetCustomers()
        {
            var customers = new NorthwindEntities().Customers.Select(e => new CustomerViewModel
            {
                CustomerID = e.CustomerID,
                CompanyName = e.CompanyName
            }).OrderBy(e => e.CompanyName);

            return customers;
        }


        public IQueryable<ShipperViewModel> GetShippers()
        {
            var shippers = new NorthwindEntities().Shippers.Select(e => new ShipperViewModel
            {
                ShipperID = e.ShipperID,
                CompanyName = e.CompanyName
            }).OrderBy(e => e.CompanyName);

            return shippers;
        }
    }
}

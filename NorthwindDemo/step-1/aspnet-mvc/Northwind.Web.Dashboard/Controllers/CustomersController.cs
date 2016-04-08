using KendoUI.Northwind.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Data;

namespace KendoUI.Northwind.Dashboard.Controllers
{
    public class CustomersController : Controller
    {
        public ActionResult Customers_Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetCustomers().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Customers_Create([DataSourceRequest]DataSourceRequest request, CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {
                    var entity = new Customer
                    {
                        CustomerID = Guid.NewGuid().ToString().Substring(0,5),
                        CompanyName = customer.CompanyName,
                        Country = customer.Country,
                        City = customer.City,
                        ContactName = customer.ContactName,
                        Phone = customer.Phone,
                    };
                    northwind.Customers.Add(entity);
                    northwind.SaveChanges();
                    customer.CustomerID = entity.CustomerID;
                }
            }
            return Json(new[] { customer }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Customers_Update([DataSourceRequest]DataSourceRequest request, CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {
                    var entity = new Customer
                    {
                        CustomerID = customer.CustomerID,
                        CompanyName = customer.CompanyName,
                        Country = customer.Country,
                        City = customer.City,
                        ContactName = customer.ContactName,
                        Phone = customer.Phone,
                    };
                    northwind.Customers.Attach(entity);
                    northwind.Entry(entity).State = EntityState.Modified;
                    northwind.SaveChanges();
                }
            }
            return Json(new[] { customer }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Customers_Destroy([DataSourceRequest]DataSourceRequest request, CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {
                    var entity = new Customer
                    {
                        CustomerID = customer.CustomerID,
                        CompanyName = customer.CompanyName,
                        Country = customer.Country,
                        City = customer.City,
                        ContactName = customer.ContactName,
                        Phone = customer.Phone,
                    };
                    northwind.Customers.Attach(entity);
                    northwind.Customers.Remove(entity);
                    northwind.SaveChanges();
                }
            }
            return Json(new[] { customer }.ToDataSourceResult(request, ModelState));
        } 

        private static IEnumerable<CustomerViewModel> GetCustomers()
        {
            var northwind = new NorthwindEntities();
            var customers = northwind.Customers.Select(customer => new CustomerViewModel
            {
                CustomerID = customer.CustomerID,
                CompanyName = customer.CompanyName,
                ContactName = customer.ContactName,
                City = customer.City,
                Country = customer.Country,
                Phone = customer.Phone
            });

            return customers;
        }

    }
}

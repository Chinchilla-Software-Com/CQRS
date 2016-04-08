using KendoUI.Northwind.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.Extensions;
using System.Web.Mvc;
using Kendo.Mvc.UI;

namespace KendoUI.Northwind.Dashboard.Controllers
{
    public class ProductsController : Controller
    {
        public ActionResult Products_Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetProducts().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_Product(int ID)
        {
            return Json(GetProducts().Where(product => product.ProductID == ID).SingleOrDefault(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductsSalesByMonth(int ProductID)
        {
            var northwind = new NorthwindEntities();
            var result = from o in northwind.Orders
                        join od in northwind.Order_Details on o.OrderID equals od.OrderID
                        where od.ProductID == ProductID
                    select new {
                        Date = o.OrderDate,
                        Quantity = od.Quantity
                    };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private static IQueryable<ProductViewModel> GetProducts()
        {
            var northwind = new NorthwindEntities();
            var products = northwind.Products.Select(product => new ProductViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Category = new CategoryViewModel() {
                    CategoryID = product.Category.CategoryID,
                    CategoryName = product.Category.CategoryName
                },
                UnitsInStock = (short)product.UnitsInStock,
                UnitsOnOrder = (short)product.UnitsOnOrder,
                ReorderLevel = (short)product.ReorderLevel,
                QuantityPerUnit = product.QuantityPerUnit
            });

            return products;
        }
    }
}

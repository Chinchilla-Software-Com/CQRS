using KendoUI.Northwind.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc; 
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace KendoUI.Northwind.Dashboard.Controllers
{
    public class OrderDetailsController : Controller
    {
        public ActionResult OrderDetails_Read([DataSourceRequest] DataSourceRequest request, int OrderID)
        {
            var orders = GetOrderDetails().Where(o => o.OrderID.Equals(OrderID));
            return Json(orders.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private static IEnumerable<OrderDetailViewModel> GetOrderDetails()
        {
            var northwind = new NorthwindEntities();
            var order_details = northwind.Order_Details.Select(od => new OrderDetailViewModel
            {
                OrderID = od.OrderID,
                ProductID = od.ProductID,
                UnitPrice = od.UnitPrice,
                Quantity = od.Quantity,
                Discount = od.Discount
            });

            return order_details;
        }

        public ActionResult OrderDetails_Create([DataSourceRequest]DataSourceRequest request, OrderDetailViewModel order, int ParentID)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {
                    var existingEntity = northwind.Order_Details.FirstOrDefault(detail => detail.OrderID == ParentID && detail.ProductID == order.ProductID);
                    if (existingEntity != null)
                    {
                        string errorMessage = string.Format("Record with ProductID:{0} and OrderID:{1} already exists in current order.", order.OrderID, order.ProductID);
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        var entity = new Order_Detail
                        {
                            OrderID = ParentID,
                            ProductID = order.ProductID,
                            UnitPrice = order.UnitPrice,
                            Quantity = (short)order.Quantity,
                            Discount = order.Discount
                        };
                        northwind.Order_Details.Add(entity);
                        northwind.SaveChanges();
                        order.OrderID = entity.OrderID;
                    }
                }
            }
            return Json(new[] { order }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult OrderDetails_Update([DataSourceRequest]DataSourceRequest request, OrderDetailViewModel order)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {

                    var entity = northwind.Order_Details.FirstOrDefault(detail => detail.OrderID == order.OrderID && detail.ProductID == order.ProductID);
                    if (entity == null)
                    {
                        string errorMessage = string.Format("Cannot update record with ProductID:{0} and OrderID:{1} as it's not available in current order.", order.OrderID, order.ProductID);
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        entity.UnitPrice = order.UnitPrice;
                        entity.Quantity = (short)order.Quantity;
                        entity.Discount = order.Discount;

                        northwind.SaveChanges();
                    }
                }
            }
            return Json(new[] { order }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult OrderDetails_Destroy([DataSourceRequest]DataSourceRequest request, OrderDetailViewModel order)
        {
            if (ModelState.IsValid)
            {
                using (var northwind = new NorthwindEntities())
                {

                    var entity = northwind.Order_Details.FirstOrDefault(detail => detail.OrderID == order.OrderID && detail.ProductID == order.ProductID);
                    if (entity == null)
                    {
                        string errorMessage = string.Format("Cannot delete record with ProductID:{0} and OrderID:{1} as it's not available in current order.", order.OrderID, order.ProductID);
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        northwind.Order_Details.Remove(entity);
                        northwind.SaveChanges();
                    }
                }
            }
            return Json(new[] { order }.ToDataSourceResult(request, ModelState));
        }
    }
}

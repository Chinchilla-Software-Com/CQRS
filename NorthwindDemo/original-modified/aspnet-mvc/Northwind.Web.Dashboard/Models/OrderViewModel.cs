using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KendoUI.Northwind.Dashboard.Models
{
    public class OrderViewModel
    {
        [ScaffoldColumn(false)]
        public int OrderID { get; set; }

        [Required]
        [UIHint("CustomGridForeignKey")]
        [DisplayName("Customer")]
        public string CustomerID { get; set; }

        [ScaffoldColumn(false)]
        public string ContactName { get; set; }

        public decimal? Freight { get; set; }
        
        [Required]
        [DisplayName("Ship Address")]
        public string ShipAddress { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Order Date")]
        public DateTime? OrderDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Shipped Date")]
        public DateTime? ShippedDate { get; set; }

        [Required]
        [UIHint("ShipCountry")]
        [DisplayName("Ship Country")]
        public string ShipCountry { get; set; }

        [Required]
        [DisplayName("Ship City")]
        public string ShipCity { get; set; }

        [Required]
        [DisplayName("Ship Name")]
        public string ShipName { get; set; }

        [UIHint("CustomGridForeignKey")]
        [DisplayName("Employee")]
        public int? EmployeeID { get; set; }

        [UIHint("CustomGridForeignKey")]
        public int? ShipVia { get; set; }

        //[Required]
        [DisplayName("Ship Postal Code")]
        public string ShipPostalCode { get; set; }
    }
}

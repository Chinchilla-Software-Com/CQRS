using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KendoUI.Northwind.Dashboard.Models
{
    public class CustomerViewModel
    {
        [ScaffoldColumn(false)]
        public string CustomerID { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Phone { get; set; }
        public string Fax { get; set; }
        public Nullable<bool> Bool { get; set; }
    }
}
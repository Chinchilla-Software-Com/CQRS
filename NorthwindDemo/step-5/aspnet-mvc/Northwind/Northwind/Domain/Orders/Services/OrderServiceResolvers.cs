using Cqrs.Domain;
using Northwind.Domain.Orders;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Services;

namespace Northwind.Domain.Orders.Services
{

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IOrderService.GetAll"/> via WCF
	/// </summary>
	public partial class OrderServiceGetAllResolver
	{
	}

}

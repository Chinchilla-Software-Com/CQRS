using System.Runtime.Serialization;

namespace Northwind.Domain.Orders.Services
{


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IOrderService.CreateOrder"/> via WCF
	/// </summary>
	public partial class OrderServiceCreateOrderParametersResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IOrderService.UpdateOrder"/> via WCF
	/// </summary>
	public partial class OrderServiceUpdateOrderParametersResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IOrderService.DeleteOrder"/> via WCF
	/// </summary>
	public partial class OrderServiceDeleteOrderParametersResolver
	{
	}

}

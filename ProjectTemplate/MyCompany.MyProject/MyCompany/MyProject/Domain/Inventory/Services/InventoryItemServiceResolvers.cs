using Cqrs.Domain;
using MyCompany.MyProject.Domain.Inventory.Entities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Services;

namespace MyCompany.MyProject.Domain.Inventory.Services
{

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.GetAll"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceGetAllResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.GetByRsn"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceGetByRsnResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.ChangeName"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceChangeNameResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.CheckIn"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceCheckInResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.Create"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceCreateResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.Deactivate"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceDeactivateResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IInventoryItemService.Remove"/> via WCF
	/// </summary>
	public partial class InventoryItemServiceRemoveResolver
	{
	}

}

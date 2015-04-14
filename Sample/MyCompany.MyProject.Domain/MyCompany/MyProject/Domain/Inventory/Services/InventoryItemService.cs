#region Copyright
// -----------------------------------------------------------------------
// <copyright company="cdmdotnet Limited">
//     Copyright cdmdotnet Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Repositories.Queries;
using Cqrs.Services;
using MyCompany.MyProject.Domain.Inventory.Commands;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Repositories;
using MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies;

namespace MyCompany.MyProject.Domain.Inventory.Services
{
	public partial class InventoryItemService 
	{
		protected IInventoryItemSummaryRepository InventoryItemSummaryRepository { get; private set; }

		public InventoryItemService(IInventoryItemSummaryRepository inventoryItemSummaryRepository)
		{
			InventoryItemSummaryRepository = inventoryItemSummaryRepository;
		}

		partial void OnGetAll(IServiceRequestWithData<Guid, InventoryItemServiceGetAllParameters> serviceRequest, ref IServiceResponseWithResultData<IEnumerable<InventoryItemSummaryEntity>> results)
		{
			// Define Query
			ICollectionResultQuery<InventoryItemSummaryQueryStrategy, InventoryItemSummaryEntity> query = QueryFactory.CreateNewCollectionResultQuery<InventoryItemSummaryQueryStrategy, InventoryItemSummaryEntity>();

			// Retrieve Data, but remember if no items exist, the value is null
			query = InventoryItemSummaryRepository.Retrieve(query);
			IEnumerable<InventoryItemSummaryEntity> inventoryItems = query.Result;

			results = new ServiceResponseWithResultData<IEnumerable<InventoryItemSummaryEntity>>(inventoryItems);
		}

		partial void OnGetByRsn(IServiceRequestWithData<Guid, InventoryItemServiceGetByRsnParameters> serviceRequest, ref IServiceResponseWithResultData<InventoryItemEntity> results)
		{
			// Define Query
			ISingleResultQuery<InventoryItemQueryStrategy, InventoryItemEntity> query = QueryFactory.CreateNewSingleResultQuery<InventoryItemQueryStrategy, InventoryItemEntity>();
			query.QueryStrategy.WithRsn(serviceRequest.Data.rsn);

			// Retrieve Data, but remember if no items exist, the value is null
			query = InventoryItemRepository.Retrieve(query);
			InventoryItemEntity inventoryItem = query.Result;

			results = new ServiceResponseWithResultData<InventoryItemEntity>(inventoryItem);
		}

		partial void OnChangeName(IServiceRequestWithData<Guid, InventoryItemServiceChangeNameParameters> serviceRequest, ref IServiceResponse results)
		{
			UnitOfWorkService.SetCommitter(this);
			InventoryItemServiceChangeNameParameters item = serviceRequest.Data;

			// The use of Guid.Empty is simply because we use ints as ids
			var command = new RenameInventoryItemCommand(item.rsn, item.newName);
			CommandSender.Send(command);

			UnitOfWorkService.Commit(this);
			results = new ServiceResponse();
		}

		partial void OnCheckIn(IServiceRequestWithData<Guid, InventoryItemServiceCheckInParameters> serviceRequest, ref IServiceResponse results)
		{
			UnitOfWorkService.SetCommitter(this);
			InventoryItemServiceCheckInParameters item = serviceRequest.Data;

			// The use of Guid.Empty is simply because we use ints as ids
			var command = new CheckInItemsToInventoryCommand(item.rsn, item.count);
			CommandSender.Send(command);

			UnitOfWorkService.Commit(this);
			results = new ServiceResponse();
		}

		partial void OnCreate(IServiceRequestWithData<Guid, InventoryItemServiceCreateParameters> serviceRequest, ref IServiceResponse results)
		{
			UnitOfWorkService.SetCommitter(this);
			InventoryItemServiceCreateParameters item = serviceRequest.Data;

			// The use of Guid.Empty is simply because we use ints as ids
			var command = new CreateInventoryItemCommand(Guid.Empty, item.name);
			CommandSender.Send(command);

			UnitOfWorkService.Commit(this);
			results = new ServiceResponse();
		}

		partial void OnDeactivate(IServiceRequestWithData<Guid, InventoryItemServiceDeactivateParameters> serviceRequest, ref IServiceResponse results)
		{
			UnitOfWorkService.SetCommitter(this);
			InventoryItemServiceDeactivateParameters item = serviceRequest.Data;

			// The use of Guid.Empty is simply because we use ints as ids
			var command = new DeactivateInventoryItemCommand(item.rsn);
			CommandSender.Send(command);

			UnitOfWorkService.Commit(this);
			results = new ServiceResponse();
		}

		partial void OnRemove(IServiceRequestWithData<Guid, InventoryItemServiceRemoveParameters> serviceRequest, ref IServiceResponse results)
		{
			UnitOfWorkService.SetCommitter(this);
			InventoryItemServiceRemoveParameters item = serviceRequest.Data;

			// The use of Guid.Empty is simply because we use ints as ids
			var command = new RemoveItemsFromInventoryCommand(item.rsn, item.count);
			CommandSender.Send(command);

			UnitOfWorkService.Commit(this);
			results = new ServiceResponse();
		}
	}
}
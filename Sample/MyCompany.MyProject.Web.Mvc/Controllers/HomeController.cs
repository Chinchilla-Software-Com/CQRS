using System;
using System.Web.Mvc;
using Cqrs.Authentication;
using Cqrs.Services;
using MyCompany.MyProject.Domain.Authentication.Services;
using MyCompany.MyProject.Domain.Inventory.Services;

namespace MyCompany.MyProject.Web.Mvc.Controllers
{
	public class HomeController : Controller
	{
		protected IInventoryItemService InventoryItemService { get; private set; }

		protected IUserService UserService { get; private set; }

		public HomeController(IInventoryItemService inventoryItemService)
		{
			InventoryItemService = inventoryItemService;
		}

		protected virtual ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken) HttpContext.Session["UserToken"];
		}

		public ActionResult DtoIndex()
		{
			ServiceRequestWithData<ISingleSignOnToken, UserServiceGetAllParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, UserServiceGetAllParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new UserServiceGetAllParameters()
			};
			ViewData.Model = UserService.GetAll(parameters);
			return View();
		}

		public ActionResult DtoDetails(Guid id)
		{
			ServiceRequestWithData<ISingleSignOnToken, UserServiceGetByRsnParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, UserServiceGetByRsnParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new UserServiceGetByRsnParameters { rsn = id }
			};
			ViewData.Model = UserService.GetByRsn(parameters);
			return View();
		}

		public ActionResult DtoAdd()
		{
			return View();
		}

		[HttpPost]
		public ActionResult DtoAdd(string name)
		{
			ServiceRequestWithData<ISingleSignOnToken, Domain.Authentication.Entities.UserEntity> parameters = new ServiceRequestWithData<ISingleSignOnToken, Domain.Authentication.Entities.UserEntity>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new Domain.Authentication.Entities.UserEntity { Name = name }
			};
			UserService.CreateUser(parameters);

			return RedirectToAction("DtoIndex");
		}




		public ActionResult Index()
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetAllParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetAllParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceGetAllParameters()
			};
			ViewData.Model = InventoryItemService.GetAll(parameters);
			return View();
		}

		public ActionResult Details(Guid id)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceGetByRsnParameters {rsn = id}
			};
			ViewData.Model = InventoryItemService.GetByRsn(parameters);
			return View();
		}

		public ActionResult Add()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Add(string name)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCreateParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCreateParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceCreateParameters { name = name }
			};
			InventoryItemService.Create(parameters);
			return RedirectToAction("Index");
		}

		public ActionResult ChangeName(Guid id)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceGetByRsnParameters { rsn = id }
			};
			ViewData.Model = InventoryItemService.GetByRsn(parameters);
			return View();
		}

		[HttpPost]
		public ActionResult ChangeName(Guid id, string name, int version)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceChangeNameParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceChangeNameParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceChangeNameParameters { rsn = id, newName = name }
			};
			InventoryItemService.ChangeName(parameters);

			return RedirectToAction("Index");
		}

		public ActionResult Deactivate(Guid id, int version)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceDeactivateParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceDeactivateParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceDeactivateParameters { rsn = id }
			};
			InventoryItemService.Deactivate(parameters);

			return RedirectToAction("Index");
		}

		public ActionResult CheckIn(Guid id)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceGetByRsnParameters { rsn = id }
			};
			ViewData.Model = InventoryItemService.GetByRsn(parameters);
			return View();
		}

		[HttpPost]
		public ActionResult CheckIn(Guid id, int number, int version)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCheckInParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCheckInParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceCheckInParameters { rsn = id, count = number }
			};
			InventoryItemService.CheckIn(parameters);

			return RedirectToAction("Index");
		}

		public ActionResult Remove(Guid id)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceGetByRsnParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceGetByRsnParameters { rsn = id }
			};
			ViewData.Model = InventoryItemService.GetByRsn(parameters);
			return View();
		}

		[HttpPost]
		public ActionResult Remove(Guid id, int number, int version)
		{
			ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceRemoveParameters> parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceRemoveParameters>
			{
				AuthenticationToken = GetAuthenticationToken(),
				Data = new InventoryItemServiceRemoveParameters { rsn = id }
			};
			InventoryItemService.Remove(parameters);
			return RedirectToAction("Index");
		}
	}
}

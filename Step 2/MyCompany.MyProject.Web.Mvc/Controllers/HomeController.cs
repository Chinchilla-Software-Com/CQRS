using System;
using System.Web.Mvc;
using Cqrs.Authentication;
using cdmdotnet.Logging;
using Cqrs.Services;

namespace MyCompany.MyProject.Web.Mvc.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}

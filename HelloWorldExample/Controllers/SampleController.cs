using System.Web.Mvc;

namespace HelloWorldExample.Controllers
{
	[Authorize]
	public class SampleController : Controller
	{
		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}
	}
}
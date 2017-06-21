using System.Web.Mvc;

namespace HelloWorld.Controllers
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
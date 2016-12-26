using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace $safeprojectname$.Areas.Client.Controllers
{
	public class ClientController : Controller
	{
		public virtual PartialViewResult Index()
		{
			var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
			var apiMethods = apiExplorer.ApiDescriptions.Select(ad => new ApiMethodModel(ad)).ToList();
			HttpContext.Response.ContentType = "application/javascript";
			return PartialView(apiMethods);
		}

		public class ApiMethodModel
		{
			public string Method { get; set; }
			public string Url { get; set; }
			public string ControllerName { get; set; }
			public string ActionName { get; set; }
			public IEnumerable<ApiParameterModel> Parameters { get; set; }

			public ApiMethodModel(ApiDescription apiDescription)
			{
				Method = apiDescription.HttpMethod.Method;
				Url = apiDescription.RelativePath;
				ControllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
				ActionName = apiDescription.ActionDescriptor.ActionName;
				Parameters = apiDescription.ParameterDescriptions.Select(pd => new ApiParameterModel(pd));
			}
		}

		public class ApiParameterModel
		{
			public string Name { get; set; }
			public bool IsUriParameter { get; set; }

			public ApiParameterModel(ApiParameterDescription apiParameterDescription)
			{
				Name = apiParameterDescription.Name;
				IsUriParameter = apiParameterDescription.Source == ApiParameterSource.FromUri;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace Results.Domain.Host.Web.Areas.Client.Controllers
{
	public class ClientController : Controller
	{
		public virtual PartialViewResult Index()
		{
			IApiExplorer apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
			IDictionary<string, IList<ApiDescription>> apiDescriptions = new Dictionary<string, IList<ApiDescription>>();
			foreach (ApiDescription apiDescription in apiExplorer.ApiDescriptions)
			{
				IList<ApiDescription> list;
				if (!apiDescriptions.TryGetValue(apiDescription.ActionDescriptor.ActionName, out list))
				{
					list = new List<ApiDescription>();
					apiDescriptions.Add(apiDescription.ActionDescriptor.ActionName, list);
				}
				list.Add(apiDescription);
			}
			IList<ApiMethodModel> apiMethods = new List<ApiMethodModel>();
			foreach (KeyValuePair<string, IList<ApiDescription>> apiDescription in apiDescriptions)
			{
				IList<ApiDescription> ads = apiDescription.Value;
				if (ads.Count == 1)
					apiMethods.Add(new ApiMethodModel(ads.Single()));
				if (ads.Count == 2)
				{
					string actionName0 = ads[0].ActionDescriptor.ActionName;
					string actionName1 = ads[1].ActionDescriptor.ActionName;
					if (actionName0 == actionName1)
					{
						var action0 = new ApiMethodModel(ads[0]);
						ApiMethodModel action1;
						if (!ads[0].RelativePath.Contains("{"))
						{
							action1 = action0;
							action0 = new ApiMethodModel(ads[1]);
						}
						else
							action1 = new ApiMethodModel(ads[1]);
						if (action0.ActionName.StartsWith("Delete"))
							action0.ActionName = "Delete";
						else if (action0.ActionName.StartsWith("Update"))
							action0.ActionName = "Update";
						else if (action0.ActionName.StartsWith("Create"))
							action0.ActionName = "Create";
						apiMethods.Add(action0);
						apiMethods.Add(action1);
					}
				}
				else
				{
					for (int i = 0; i < ads.Count; i++)
					{
						ApiDescription description = ads[i];
						var action = new ApiMethodModel(description);
						action.ActionName = action.ActionName + i;
						apiMethods.Add(action);
					}
				}
			}
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

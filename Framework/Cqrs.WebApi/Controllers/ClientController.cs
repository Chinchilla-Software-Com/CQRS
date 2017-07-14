#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cqrs.WebApi.Controllers
{
	[RoutePrefix("Client")]
	public class ClientController : ApiController
	{
		[Route("")]
		[HttpGet]
		public virtual HttpResponseMessage Index()
		{
			var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
			var apiMethods = apiExplorer.ApiDescriptions.Select(ad => new ApiMethodModel(ad)).ToList();

			string host = Url.Content(Request.RequestUri.AbsoluteUri.Substring(0, Request.RequestUri.AbsoluteUri.Length - Request.RequestUri.AbsolutePath.Length));
			string path = Url.Content("~/");
			if (path.StartsWith(host))
				host = null;

			var responseBody = string.Format(@"window.api = window.api || {{
	metadata: {0},
	useJson: true
}};

$.each(window.api.metadata, function (i, action)
{{
	if (!window.api[action.ControllerName])
	{{
		window.api[action.ControllerName] = {{}};
	}}
	window.api[action.ControllerName][action.ActionName] = function (parameters)
	{{
		var url = '{1}{2}' + action.Url;
		var data = {{}};

		if (action.Parameters.length == 1 && action.Parameters[0].Name == 'parameters')
		{{
			data = parameters;
		}}
		else if (action.Parameters.length == 2 && action.Parameters[0].Name == 'entity' && action.Parameters[1].Name == 'rsn')
		{{
			url = url.substring(0, url.length - 5) + parameters['Rsn'];
			data = parameters;
		}}
		else if (window.api.unwrapParameters && action.Parameters.length == 1 && parameters.constructor !== Array)
		{{
			var parameter = action.Parameters[0];
			if (parameter.IsUriParameter)
			{{
				url = url.replace('{{' + parameter.Name + '}}', typeof(parameters) === 'object' ? parameters[parameter.Name] : parameters);
				data = null;
			}}
			else
			{{
				data = parameters;
			}}
		}}
		else
		{{
			$.each(action.Parameters, function (j, parameter)
			{{
				if (parameters[parameter.Name] === undefined)
				{{
					console.error('Missing parameter: ' + parameter.Name + ' for API: ' + action.ControllerName + '/' + action.ActionName);
				}}
				else if (parameter.IsUriParameter)
				{{
					url = url.replace('{{' + parameter.Name + '}}', parameters[parameter.Name]);
				}}
				else if (data[parameter.Name] === undefined)
				{{
					data[parameter.Name] = parameters[parameter.Name];
				}}
				else
				{{
					console.error('Detected multiple body-parameters for API: ' + action.ControllerName + '/' + action.ActionName);
				}}
			}});
		}}

		if (window.api.useJson)
			return $.ajax({{
				type: action.Method,
				url: url,
				data: JSON.stringify(data),
				contentType: 'application/json'
			}});
		return $.ajax({{
			type: action.Method,
			url: url,
			data: data
		}});
	}};
}});",
				System.Web.Helpers.Json.Encode(apiMethods),
				host,
				path);

			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, responseBody);
			response.Content = new StringContent(responseBody, Encoding.UTF8, "application/javascript");
			return response;
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


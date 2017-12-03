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
using Cqrs.Configuration;

namespace Cqrs.WebApi.Controllers
{
	/// <summary>
	/// A WebAPI RESTful service that provide a 'no configuration required' java-script client ready for developers to use.
	/// </summary>
	[RoutePrefix("Client")]
	public class ClientController : ApiController
	{
		/// <summary>
		/// A java-script file with a 'no configuration required' java-script client
		/// </summary>
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
	useJson: true,
	useXToken: true,
	cookieTokenName: '{3}',
	// This is because JQuery notes Global events are never fired for cross-domain script or JSONP requests, regardless of the value of global at https://api.jquery.com/category/ajax/global-ajax-event-handlers/
	globalHandlers: {{
		'before' : function(jqXHR, settings) {{}},
		'complete' : function(jqXHR, textStatus) {{}},
		'error' : function(jqXHR, textStatus, errorThrown) {{}},
		'success' : function(data, textStatus, jqXHR) {{}},
		'setHeaders' : function()
		{{
			if (window.api.useXToken && typeof(Cookies) === 'function')
			{{
				var headers = {{}};
				headers[window.api.cookieTokenName] = Cookies.get(window.api.cookieTokenName);
				return headers;
			}}
			return {{}}
		}},
		'202' : function(data, textStatus, jqXHR) {{}},
		'300' : function(jqXHR, textStatus, errorThrown) {{}},
		'400' : function(jqXHR, textStatus, errorThrown) {{}},
		'401' : function(jqXHR, textStatus, errorThrown) {{}},
		'403' : function(jqXHR, textStatus, errorThrown) {{}},
		'404' : function(jqXHR, textStatus, errorThrown) {{}},
		'412' : function(jqXHR, textStatus, errorThrown) {{}},
		'500' : function(jqXHR, textStatus, errorThrown) {{}},
	}}
}};

$.each(window.api.metadata, function (i, action)
{{
	if (!window.api[action.ControllerName])
	{{
		window.api[action.ControllerName] = {{}};
	}}
	window.api[action.ControllerName][action.ActionName] = function (parameters, successFunction, errorFunction, statusCodeFunctions)
	{{
		var url = '{1}{2}' + action.Url;
		var data = {{}};
		var bodyParameters = 0;
		var complexParameters = 0;

		if (action.Parameters.length == 1 && action.Parameters[0].Name == 'parameters')
		{{
			bodyParameters = 1;
			data = parameters;
		}}
		else if (action.Parameters.length == 2 && action.Parameters[0].Name == 'entity' && action.Parameters[1].Name == 'rsn')
		{{
			bodyParameters = 1;
			url = url.substring(0, url.length - 5) + parameters['Rsn'];
			data = parameters;
		}}
		else
		{{
			var lastBodyParameter = null;
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
					if (parameter.IsBodyParameter)
					{{
						bodyParameters++;
						lastBodyParameter = parameter.Name;
					}}
					else
						complexParameters++;
					data[parameter.Name] = parameters[parameter.Name];
				}}
				else
				{{
					console.error('Detected multiple body-parameters for API: ' + action.ControllerName + '/' + action.ActionName);
				}}
			}});
		}}

		if (bodyParameters == 1 && complexParameters == 0 && lastBodyParameter != null && lastBodyParameter != "")
			data = data[lastBodyParameter];

		if (bodyParameters == 0 && complexParameters == 0)
			data = null;

		if (window.api.useJson)
			return $.ajax({{
				type: action.Method,
				url: url,
				data: data == null ? data : JSON.stringify(data),
				contentType: 'application/json',
				headers: window.api.globalHandlers['setHeaders'](),
				beforeSend: window.api.globalHandlers['before'],
				complete: window.api.globalHandlers['complete'],
				error: (errorFunction || window.api.globalHandlers['error']),
				success: (successFunction || window.api.globalHandlers['success']),
				statusCode: ( statusCodeFunctions || {{
					202: window.api.globalHandlers['202'],
					300: window.api.globalHandlers['300'],
					400: window.api.globalHandlers['400'],
					401: window.api.globalHandlers['401'],
					403: window.api.globalHandlers['403'],
					404: window.api.globalHandlers['404'],
					412: window.api.globalHandlers['412'],
					500: window.api.globalHandlers['500']
				}})
			}});
		return $.ajax({{
			type: action.Method,
			url: url,
			data: data,
			headers: window.api.globalHandlers['setHeaders'](),
			beforeSend: window.api.globalHandlers['before'],
			complete: window.api.globalHandlers['complete'],
			error: (errorFunction || window.api.globalHandlers['error']),
			success: (successFunction || window.api.globalHandlers['success']),
			statusCode: ( statusCodeFunctions || {{
				202: window.api.globalHandlers['202'],
				300: window.api.globalHandlers['300'],
				400: window.api.globalHandlers['400'],
				401: window.api.globalHandlers['401'],
				403: window.api.globalHandlers['403'],
				404: window.api.globalHandlers['404'],
				412: window.api.globalHandlers['412'],
				500: window.api.globalHandlers['500']
			}})
		}});
	}};
}});",
				System.Web.Helpers.Json.Encode(apiMethods),
				host,
				path,
				DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token");

			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, responseBody);
			response.Content = new StringContent(responseBody, Encoding.UTF8, "application/javascript");
			return response;
		}

		/// <summary>
		/// A WebAPI action method.
		/// </summary>
		public class ApiMethodModel
		{
			/// <summary>
			/// The type of the <see cref="HttpMethod"/>.
			/// </summary>
			public string Method { get; set; }

			/// <summary>
			/// The URL of the action method.
			/// </summary>
			public string Url { get; set; }

			/// <summary>
			/// The name of the <see cref="ApiController"/> this action method is in.
			/// </summary>
			public string ControllerName { get; set; }

			/// <summary>
			/// The name of the action method.
			/// </summary>
			public string ActionName { get; set; }

			/// <summary>
			/// The <see cref="ApiParameterModel">parameters</see> of this action method.
			/// </summary>
			public IEnumerable<ApiParameterModel> Parameters { get; set; }

			/// <summary>
			/// Instantiates a new instance of <see cref="ApiMethodModel"/> with the provided <paramref name="apiDescription"/>.
			/// </summary>
			public ApiMethodModel(ApiDescription apiDescription)
			{
				Method = apiDescription.HttpMethod.Method;
				Url = apiDescription.RelativePath;
				ControllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
				ActionName = apiDescription.ActionDescriptor.ActionName;
				Parameters = apiDescription.ParameterDescriptions.Select(pd => new ApiParameterModel(pd));
			}
		}

		/// <summary>
		/// A parameter for a WebAPI action method.
		/// </summary>
		public class ApiParameterModel
		{
			/// <summary>
			/// The name of the parameter.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Indicates if the parameter is provided in the URL of the request or not.
			/// </summary>
			public bool IsUriParameter { get; set; }

			/// <summary>
			/// Indicates if the parameter is provided in the body of the request or not.
			/// </summary>
			public bool IsBodyParameter { get; set; }

			/// <summary>
			/// Instantiates a new instance of <see cref="ApiParameterModel"/> with the provided <paramref name="apiParameterDescription"/>.
			/// </summary>
			public ApiParameterModel(ApiParameterDescription apiParameterDescription)
			{
				Name = apiParameterDescription.Name;
				IsUriParameter = apiParameterDescription.Source == ApiParameterSource.FromUri;
				IsBodyParameter = apiParameterDescription.Source == ApiParameterSource.FromBody;
			}
		}
	}
}
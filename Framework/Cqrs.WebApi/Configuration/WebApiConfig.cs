#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Web.Http;
using System.Web.Http.Cors;
using Cqrs.Configuration;
using Newtonsoft.Json;

namespace Cqrs.WebApi.Configuration
{
	/// <summary>
	/// A configuration class for WebAPI.
	/// </summary>
	public class WebApiConfig
	{
		/// <summary>
		/// Registers the require routes, set relevant CORS settings and defines WebAPI relevant JSON serialisation settings.
		/// </summary>
		public static void Register(HttpConfiguration config)
		{
			var configurationManager = DependencyResolver.Current.Resolve<IConfigurationManager>();
			var cors = new EnableCorsAttribute
			(
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Origins"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Headers"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Methods"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.ExposedHeaders")
			);
			config.EnableCors(cors);

			try
			{
				config.MapHttpAttributeRoutes();
			}
			catch (ArgumentException exception)
			{
				if (exception.ParamName != "name")
					throw;
			}

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
			{
				ContractResolver = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ContractResolver,
				StringEscapeHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.StringEscapeHandling,
				PreserveReferencesHandling = PreserveReferencesHandling.None,
				MissingMemberHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MissingMemberHandling,
				Binder = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Binder,
				CheckAdditionalContent = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.CheckAdditionalContent,
				ConstructorHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ConstructorHandling,
				Context = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Context,
				Converters = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Converters,
				Culture = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Culture,
				DateFormatHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateFormatHandling,
				DateFormatString = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateFormatString,
				DateParseHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateParseHandling,
				DateTimeZoneHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateTimeZoneHandling,
				DefaultValueHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DefaultValueHandling,
				Error = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Error,
				FloatFormatHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.FloatFormatHandling,
				FloatParseHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.FloatParseHandling,
				Formatting = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Formatting,
				MaxDepth = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MaxDepth,
				MetadataPropertyHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MetadataPropertyHandling,
				NullValueHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.NullValueHandling,
				ObjectCreationHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ObjectCreationHandling,
				ReferenceLoopHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ReferenceLoopHandling,
				ReferenceResolver = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ReferenceResolver,
				TraceWriter = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.TraceWriter,
				TypeNameAssemblyFormat = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.TypeNameAssemblyFormat,
				TypeNameHandling = TypeNameHandling.None
			};
		}
	}
}

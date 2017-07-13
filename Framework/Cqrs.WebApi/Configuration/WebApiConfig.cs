#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Web.Http;
using Newtonsoft.Json;

namespace Cqrs.WebApi.Configuration
{
	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
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
				PreserveReferencesHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.PreserveReferencesHandling,
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

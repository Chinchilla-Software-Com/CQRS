#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Authentication;

namespace Cqrs.Configuration
{
	/// <summary>
	/// A collection of extension methods for <see cref="ITelemetryHelper"/>.
	/// </summary>
	public static class ITelemetryHelperExtensions
	{
		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		/// <param name="telemetryHelper">The <see cref="ITelemetryHelper"/> being extended.s</param>
		/// <param name="name">The request name.</param>
		/// <param name="token">The token with user identifiable information.</param>
		/// <param name="startTime">The time when the page was requested.</param>
		/// <param name="duration">The time taken by the application to handle the request.</param>
		/// <param name="responseCode">The response status code.</param>
		/// <param name="wasSuccessfull">True if the request was handled successfully by the application.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public static void TrackRequest<TAuthenticationToken>(this ITelemetryHelper telemetryHelper, string name, TAuthenticationToken token, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool wasSuccessfull, IDictionary<string, string> properties = null)
			where TAuthenticationToken : ISingleSignOnToken
		{
			TrackRequest(telemetryHelper, name, token == null ? null : token.Serialise(), startTime, duration, responseCode, wasSuccessfull, properties);
		}

		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		/// <param name="telemetryHelper">The <see cref="ITelemetryHelper"/> being extended.s</param>
		/// <param name="name">The request name.</param>
		/// <param name="token">The token with user identifiable information.</param>
		/// <param name="startTime">The time when the page was requested.</param>
		/// <param name="duration">The time taken by the application to handle the request.</param>
		/// <param name="responseCode">The response status code.</param>
		/// <param name="wasSuccessfull">True if the request was handled successfully by the application.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public static void TrackRequest(this ITelemetryHelper telemetryHelper, string name, Guid? token, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool wasSuccessfull, IDictionary<string, string> properties = null)
		{
			TrackRequest(telemetryHelper, name, token == null ? null : token.Value.ToString("N"), startTime, duration, responseCode, wasSuccessfull, properties);
		}

		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		/// <param name="telemetryHelper">The <see cref="ITelemetryHelper"/> being extended.s</param>
		/// <param name="name">The request name.</param>
		/// <param name="token">The token with user identifiable information.</param>
		/// <param name="startTime">The time when the page was requested.</param>
		/// <param name="duration">The time taken by the application to handle the request.</param>
		/// <param name="responseCode">The response status code.</param>
		/// <param name="wasSuccessfull">True if the request was handled successfully by the application.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public static void TrackRequest(this ITelemetryHelper telemetryHelper, string name, int? token, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool wasSuccessfull, IDictionary<string, string> properties = null)
		{
			TrackRequest(telemetryHelper, name, token == null ? null : token.Value.ToString(), startTime, duration, responseCode, wasSuccessfull, properties);
		}

		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		/// <param name="telemetryHelper">The <see cref="ITelemetryHelper"/> being extended.s</param>
		/// <param name="name">The request name.</param>
		/// <param name="token">The token with user identifiable information.</param>
		/// <param name="startTime">The time when the page was requested.</param>
		/// <param name="duration">The time taken by the application to handle the request.</param>
		/// <param name="responseCode">The response status code.</param>
		/// <param name="wasSuccessfull">True if the request was handled successfully by the application.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public static void TrackRequest(this ITelemetryHelper telemetryHelper, string name, string token, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool wasSuccessfull, IDictionary<string, string> properties = null)
		{
			Uri url;
			try
			{
				url = new Uri(string.Format("cqrs://{0}", name));
			}
			catch
			{
				url = null;
			}

			telemetryHelper.TrackRequest(name, url, token, startTime, duration, responseCode, wasSuccessfull, properties);
		}
	}
}
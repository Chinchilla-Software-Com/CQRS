using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Authentication;

namespace Cqrs.Configuration
{
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
			Uri url;
			try
			{
				url = new Uri(string.Format("cqrs://{0}", name));
			}
			catch
			{
				url = null;
			}

			telemetryHelper.TrackRequest(name, url, token == null ? null : token.Serialise(), startTime, duration, responseCode, wasSuccessfull, properties);
		}
	}
}
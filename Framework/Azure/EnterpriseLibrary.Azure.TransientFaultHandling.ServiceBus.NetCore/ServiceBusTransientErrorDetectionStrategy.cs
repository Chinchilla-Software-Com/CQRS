// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling
{
	using System;
	using Microsoft.IdentityModel.Tokens;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Security;
	using System.ServiceModel;
	using System.Text.RegularExpressions;
	using Microsoft.Azure.ServiceBus;

	/// <summary> 
	/// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure Service Bus. 
	/// </summary> 
	public class ServiceBusTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		/// <summary> 
		/// Provides a compiled regular expression used for extracting the error code from the message. 
		/// </summary> 
		private static readonly Regex acsErrorCodeRegex = new Regex(@"Error:Code:(\d+):SubCode:(\w\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly int[] httpStatusCodes = new[] { (int)HttpStatusCode.InternalServerError, (int)HttpStatusCode.GatewayTimeout, (int)HttpStatusCode.ServiceUnavailable, (int)HttpStatusCode.RequestTimeout };
		private static readonly WebExceptionStatus[] webExceptionStatus = new[] { WebExceptionStatus.ConnectionClosed, WebExceptionStatus.Timeout, WebExceptionStatus.RequestCanceled };

		/// <summary> 
		/// Determines whether the specified exception represents a transient failure that can be compensated by a retry. 
		/// </summary> 
		/// <param name="ex">The exception object to be verified.</param> 
		/// <returns>true if the specified exception is considered transient; otherwise, false.</returns> 
		public bool IsTransient(Exception ex)
		{
			return ex != null && (CheckIsTransient(ex) || (ex.InnerException != null && CheckIsTransient(ex.InnerException)));
		}

		// SecuritySafeCritical because it references MessagingException
		[SecuritySafeCritical]
		private static bool CheckIsTransient(Exception ex)
		{
			var messagingException = ex as ServiceBusException;
			if (messagingException != null)
			{
				// The IsTransient property already covers the following scenarios:
				//if (ex is MessageLockLostException) return false;
				//if (ex is MessagingEntityAlreadyExistsException) return false;
				//if (ex is MessagingEntityNotFoundException) return false;
				//if (ex is MessagingCommunicationException) return true;
				//if (ex is ServerBusyException) return true;
				return messagingException.IsTransient;
			}

			if (ex is FaultException) return false;

			if (ex is CommunicationObjectFaultedException) return false;

			if (ex is TimeoutException) return true;

			var webException = ex as WebException;
			if (webException != null)
			{
				if (webExceptionStatus.Contains(webException.Status)) return true;

				if (webException.Status == WebExceptionStatus.ProtocolError)
				{
					var response = webException.Response as HttpWebResponse;
					if (response != null && httpStatusCodes.Contains((int)response.StatusCode)) return true;
				}
			}

			if (ex is SecurityTokenException) return true;

			if (ex is ServerTooBusyException) return true;

			if (ex is ProtocolException) return true;

			// This exception may occur when a listener and a consumer are being 
			// initialized out of sync (e.g. consumer is reaching to a listener that 
			// is still in the process of setting up the Service Host). 
			if (ex is EndpointNotFoundException) return true;

			if (ex is CommunicationException) return true;

			var socketFault = ex as SocketException;
			if (socketFault != null)
			{
				return socketFault.SocketErrorCode == SocketError.TimedOut;
			}

			if (ex is UnauthorizedAccessException)
			{
				// Need to provide some resilience against the following fault that was seen a few times: 
				// System.UnauthorizedAccessException: The token provider was unable to provide a security token while accessing 'https://xxx.accesscontrol.windows.net/WRAPv0.9/'.  
				// Token provider returned message: 'Error:Code:500:SubCode:T9002:Detail:An internal network error occured. Please try again.'.  
				// System.IdentityModel.Tokens.SecurityTokenException: The token provider was unable to provide a security token while accessing 'https://xxx.accesscontrol.windows.net/WRAPv0.9/'.  
				// Token provider returned message: 'Error:Code:500:SubCode:T9002:Detail:An internal network error occured. Please try again.'.  
				// System.Net.WebException: The remote server returned an error: (500) Internal Server Error. 
				var match = acsErrorCodeRegex.Match(ex.Message);
				var errorCode = 0;

				if (match.Success && match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out errorCode))
				{
					return httpStatusCodes.Contains(errorCode);
				}
			}

			return false;
		}
	}
}
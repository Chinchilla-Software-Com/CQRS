#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Ninject.Azure.Wcf;
using Cqrs.Ninject.Azure.Wcf.Configuration;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

/// <summary>
/// Sample Function.
/// </summary>
/// <example>
/// [FunctionName("SampleFunction")]
/// public static class MyFunction
/// {
///		public static void Run(ExecutionContext context)
///		{
///			IConfigurationRoot config = new ConfigurationBuilder().Build();
///			CqrsWebHost.SetConfigurationManager(config);
///			new CqrsFunction().Run();
///			
///			// your function code
///		}
/// }
/// </example>
public class CqrsFunction : CqrsWebHost<Guid, DefaultAuthenticationTokenHelper>
{
	/// <summary>
	/// Instantiate a new instance of <see cref="CqrsWebJobProgram"/>
	/// </summary>
	public CqrsFunction(params Type[] commandOrEventTypes)
	{
		HandlerTypes = commandOrEventTypes;
	}
}
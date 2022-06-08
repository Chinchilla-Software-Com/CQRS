#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Ninject.Azure.Wcf;
using Microsoft.Azure.WebJobs;

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
///			CqrsFunction.SetExecutionPath(context, config);
///			new CqrsFunction().Run();
///			
///			// your function code
///		}
/// }
/// </example>
public class CqrsFunction : CqrsWebHost<Guid, DefaultAuthenticationTokenHelper>
{
	/// <summary>
	/// Indicates if the <see cref="SetExecutionPath"/> method has been called.
	/// </summary>
	protected static bool HasSetExecutionPath { private set; get; }
	/// <summary>
	/// Instantiate a new instance of <see cref="CqrsWebJobProgram"/>
	/// </summary>
	public CqrsFunction(params Type[] commandOrEventTypes)
	{
		if (!HasSetExecutionPath)
			throw new InvalidOperationException("Call SetExecutionPath first.");
		HandlerTypes = commandOrEventTypes;
	}

	/// <summary>
	/// Sets the execution path
	/// </summary>
	public static void SetExecutionPath
	(
		ExecutionContext context
#if NETSTANDARD2_0
		, Microsoft.Extensions.Configuration.IConfigurationRoot config
#endif
	)
	{
#if NETSTANDARD2_0
		SetConfigurationManager(config);
#endif
		ConfigurationExtensions.GetExecutionPath = () => Path.Combine(context.FunctionDirectory, "..", "bin");
		HasSetExecutionPath = true;
	}
}
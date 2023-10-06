#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Hosts;
using Cqrs.Ninject.Azure.WebJobs;

/// <summary>
/// Starts the WebJob.
/// </summary>
/// <example>
/// public class MyProgram : CqrsWebJobProgram
/// {
///		public static void Main()
///		{
///			new MyProgram().Go();
///		}
/// }
/// </example>
public class CqrsWebJobProgram
	: CqrsNinjectJobHost<Guid, DefaultAuthenticationTokenHelper>
{
	/// <summary>
	/// Instantiate a new instance of <see cref="CqrsWebJobProgram"/>
	/// </summary>
	public CqrsWebJobProgram()
	{
		HandlerTypes = GetCommandOrEventTypes();
	}

	/// <remarks>
	/// Please set the following connection strings in app.config for this WebJob to run:
	/// AzureWebJobsDashboard and AzureWebJobsStorage
	/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
	/// </remarks>
	public virtual void Go()
	{
		CoreHost = this;
		StartHost();
		Logger.LogInfo("Application Stopped.");
	}

	/// <summary>
	/// Add JUST ONE command and/or event handler here from each assembly you want automatically scanned.
	/// </summary>
	protected virtual Type[] GetCommandOrEventTypes()
	{
		return new Type[] { };
	}
}
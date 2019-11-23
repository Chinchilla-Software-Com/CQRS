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
/// <typeparam name="TCoreHost">This should be the <see cref="Type"/> on the class you are writting. See example</typeparam>
/// <example>
/// public class MyProgram : CqrsWebJobProgram&lt;MyProgram&gt;
/// {
/// }
/// </example>
public class CqrsWebJobProgram<TCoreHost> : CqrsNinjectJobHost<Guid, DefaultAuthenticationTokenHelper>
		where TCoreHost : CoreHost<Guid>, new()
{
	/// <summary>
	/// Instantiate a new instance of <see cref="CqrsWebJobProgram{TCoreHost}"/>
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
	public static void Main()
	{
		CoreHost = new TCoreHost();
		StartHost();
	}

	/// <summary>
	/// Add JUST ONE command and/or event handler here from each assembly you want automatically scanned.
	/// </summary>
	protected virtual Type[] GetCommandOrEventTypes()
	{
		return new Type[] { };
	}
}
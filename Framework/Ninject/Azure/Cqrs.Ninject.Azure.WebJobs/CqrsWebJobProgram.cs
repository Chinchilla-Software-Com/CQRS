#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Ninject.Azure.WebJobs;

/// <summary>
/// Starts the Webjob
/// </summary>
public partial class CqrsWebJobProgram : CqrsNinjectJobHost<SingleSignOnToken, DefaultAuthenticationTokenHelper>
{
	public CqrsWebJobProgram()
	{
		System.Type commandOrEventType = null;
		GetCommandOrEventType(ref commandOrEventType);
		HandlerTypes = new[] { commandOrEventType };
	}

	/// <remarks>
	/// Please set the following connection strings in app.config for this WebJob to run:
	/// AzureWebJobsDashboard and AzureWebJobsStorage
	/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
	/// </remarks>
	public static void Main()
	{
		CoreHost = new CqrsWebJobProgram();
		StartHost();
	}

	/// <summary>
	/// Use a partial class to set add a command or event handler here on <paramref name="commandOrEventType"/>
	/// </summary>
	partial void GetCommandOrEventType(ref System.Type commandOrEventType);
}
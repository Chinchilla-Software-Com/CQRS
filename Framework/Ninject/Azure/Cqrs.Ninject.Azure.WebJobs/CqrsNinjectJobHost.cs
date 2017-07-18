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
using Cqrs.Ninject.Azure.Wcf;
using Cqrs.Ninject.Azure.WebJobs.Configuration;
using Microsoft.Azure.WebJobs;

namespace Cqrs.Ninject.Azure.WebJobs
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob using Ninject
	/// </summary>
	public class CqrsNinjectJobHost<TAuthenticationToken, TAuthenticationTokenHelper> : CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper, WebJobHostModule>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		protected static CoreHost<TAuthenticationToken> CoreHost { get; set; }

		protected static Action PreRunAndBlockAction { get; set; }

		/// <remarks>
		/// Please set the following connection strings in app.config for this WebJob to run:
		/// AzureWebJobsDashboard and AzureWebJobsStorage
		/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
		/// </remarks>
		protected static void StartHost()
		{
			// We use console state as, even though a webjob runs in an azure website, it's technically loaded via something call the 'WindowsScriptHost', which is not web and IIS based so it's threading model is very different and more console based.
			// This actually does all the work... Just sit back and relax... but also stay in memory and don't shutdown.
			CoreHost.Run();

			JobHost host;
			bool disableHostControl;
			// I set this to true ... just because.
			if (!bool.TryParse(_configurationManager.GetSetting("Cqrs.Azure.WebJobs.DisableWebJobHostControl"), out disableHostControl))
				disableHostControl = false;
			if (disableHostControl)
			{
				var jobHostConfig = new JobHostConfiguration
				{
					// https://github.com/Azure/azure-webjobs-sdk/issues/741
					DashboardConnectionString = null
				};
				host = new JobHost(jobHostConfig);
			}
			else
				host = new JobHost();

			if (PreRunAndBlockAction != null)
				PreRunAndBlockAction();

			// The following code ensures that the WebJob will be running continuously
			host.RunAndBlock();
		}
	}
}
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Hosts;
#if NET472
using Microsoft.Azure.WebJobs;
#endif
#if NETSTANDARD2_0
using Microsoft.Extensions.Hosting;
#endif

namespace Cqrs.Azure.WebJobs
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob
	/// </summary>
	public abstract class CqrsJobHost<TAuthenticationToken> : TelemetryCoreHost<TAuthenticationToken>
	{
		/// <summary>
		/// The <see cref="CoreHost"/> that gets executed by the Azure WebJob.
		/// </summary>
		protected static CoreHost<TAuthenticationToken> CoreHost { get; set; }

#if NET472
		/// <summary>
		/// An <see cref="Action"/> that is execute pre <see cref="JobHost.RunAndBlock"/>.
		/// </summary>
#endif
#if NETSTANDARD2_0
		/// <summary>
		/// An <see cref="Action"/> that is execute pre <see cref="HostingAbstractionsHostExtensions.Run(IHost)"/>.
		/// </summary>
#endif
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

#if NET472
			JobHost host;
			bool disableHostControl;
			// I set this to false ... just because.
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
#endif
#if NETSTANDARD2_0
			string environment = null;
			// I set this to false ... just because.
			environment = _configurationManager.GetSetting("Cqrs.Azure.WebJobs.Environment");

			var builder = new HostBuilder();
			if (!string.IsNullOrWhiteSpace(environment))
				builder.UseEnvironment(environment);
			builder.ConfigureWebJobs(builder =>
			{
				builder.AddAzureStorageCoreServices();
			});
			IHost host = builder.Build();
			using (host)
			{
				host.Run();
			}
#endif
		}
	}
}
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Configuration;
using Cqrs.Exceptions;
using System.Threading.Tasks;

namespace Cqrs.Azure.KeyVault
{
	/// <summary>
	/// Provides access to secrets such as keys or credentials from an Azure KeyVault by reading secrets from the <see cref="IConfigurationManager"/>.
	/// </summary>
	public class ConfigurationSecretFactory
		: ISecretFactory
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="ConfigurationSecretFactory"/>
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> user to get required configuration settings</param>
		public ConfigurationSecretFactory(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// The <see cref="IConfigurationManager"/> used to read secrets from.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; set; }

		/// <summary>
		/// Get the specified secret as identified by the provided <paramref name="secretName"/> from the <see cref="IConfigurationManager"/>.
		/// </summary>
		/// <param name="secretName">The name of the secret.</param>
		/// <returns>The secret</returns>
		/// <exception cref="MissingApplicationSettingException">If the secret isn't found by the <see cref="IConfigurationManager"/></exception>
		public string GetSecret(string secretName)
		{
			string key = $"Cqrs.Secrets.{secretName}";
			string result = ConfigurationManager.GetSetting(key);
			if (string.IsNullOrWhiteSpace(result))
				throw new MissingApplicationSettingException(key);
			return result;
		}

		/// <summary>
		/// Get the specified secret as identified by the provided <paramref name="secretName"/> from the <see cref="IConfigurationManager"/>.
		/// </summary>
		/// <param name="secretName">The name of the secret.</param>
		/// <returns>The secret</returns>
		/// <exception cref="MissingApplicationSettingException">If the secret isn't found by the <see cref="IConfigurationManager"/></exception>
		public
#if NET40
#else
			async
#endif
			Task<string> GetSecretAsync(string secretName)
		{
			string secret = GetSecret(secretName);
#if NET40
			TaskCompletionSource<string> tcs1 = new TaskCompletionSource<string>();
			Task<string> task = tcs1.Task;
			tcs1.SetResult(secret);
			return task;
#else
			return await Task.FromResult(secret);
#endif
		}
	}
}
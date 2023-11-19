#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

#if NET472
using System.Threading.Tasks;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
#else
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
#endif

using Cqrs.Configuration;
using Cqrs.Exceptions;

namespace Cqrs.Azure.KeyVault
{
	/// <summary>
	/// Provides access to secrets such as keys or credentials from an Azure KeyVault.
	/// </summary>
	public class KeyVaultSecretFactory
		: ISecretFactory
	{
		private const string KeyVaultNameKeyName = "Cqrs.Azure.Secrets.KeyVault.Name";

		private const string ApplicationIdKeyName = "Cqrs.Azure.Secrets.KeyVault.ApplicationId";

		private const string ClientSecretKeyName = "Cqrs.Azure.Secrets.KeyVault.ClientKey";

		private const string TenantIdKeyName = "Cqrs.Azure.Secrets.KeyVault.TenantId";

		/// <summary>
		/// Instantiates a new instance of the <see cref="KeyVaultSecretFactory"/>
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> user to get required configuration settings</param>
		/// <exception cref="MissingApplicationSettingException">If a required configuration setting isn't set/defined</exception>
		/// <remarks>
		/// Requires the following application settings:
		/// - Cqrs.Azure.Secrets.KeyVault.Name
		/// - Cqrs.Azure.Secrets.KeyVault.ApplicationId
		/// - Cqrs.Azure.Secrets.KeyVault.ClientKey
		/// - Cqrs.Azure.Secrets.KeyVault.TenantId (dotnet core only)
		/// </remarks>
		public KeyVaultSecretFactory(IConfigurationManager configurationManager)
		{
			string keyVaultName = configurationManager.GetSetting(KeyVaultNameKeyName);
			if (string.IsNullOrWhiteSpace(keyVaultName))
				throw new MissingApplicationSettingException(KeyVaultNameKeyName);
			KeyVaultUri = $"https://{keyVaultName}.vault.azure.net";

			string applicationId = configurationManager.GetSetting(ApplicationIdKeyName);
			if (string.IsNullOrWhiteSpace(applicationId))
				throw new MissingApplicationSettingException(ApplicationIdKeyName);

			string clientKey = configurationManager.GetSetting(ClientSecretKeyName);
			if (string.IsNullOrWhiteSpace(clientKey))
				throw new MissingApplicationSettingException(ClientSecretKeyName);

#if NET472
			ClientCredential clientCredential;
			async Task<string> getToken(string authority, string resource, string scope)
			{
				var authContext = new AuthenticationContext(authority);
				AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCredential);

				if (result == null)
					throw new InvalidOperationException("Failed to obtain an access token");
				return result.AccessToken;
			}

			clientCredential = new ClientCredential(applicationId, clientKey);

			var azureServiceTokenProvider = new AzureServiceTokenProvider();
			Client = new KeyVaultClient(getToken);
#else
			string tenantId = configurationManager.GetSetting(TenantIdKeyName);
			if (string.IsNullOrWhiteSpace(tenantId))
				throw new MissingApplicationSettingException(TenantIdKeyName);

			Client = new SecretClient(new Uri(KeyVaultUri), new ClientSecretCredential(tenantId, applicationId, clientKey));
#endif
		}

		/// <summary>
		/// The uri of the Azure KeyVault
		/// </summary>
		protected string KeyVaultUri { get; set; }

#if NET472
		/// <summary>
		/// The <see cref="Microsoft.Azure.KeyVault.KeyVaultClient"/> used interally to access the KayVault
		/// </summary>
		protected KeyVaultClient Client { get; set; }
#else
		/// <summary>
		/// The <see cref="SecretClient"/> used interally to access the KayVault
		/// </summary>
		protected SecretClient Client { get; set; }
#endif

		/// <summary>
		/// Get the specified secret as identified by the provided <paramref name="secretName"/> from Azure KeyVault.
		/// </summary>
		/// <param name="secretName">The name of the secret.</param>
		/// <returns>The secret</returns>
		public string GetSecret(string secretName)
		{
			string secret = null;
#if NET472
			Task keyVaultSecretTask = Task.Factory.StartNew(() => {
				Task<SecretBundle> getKeyVaultSecretTask = Client.GetSecretAsync(KeyVaultUri, secretName);
				getKeyVaultSecretTask.Wait();
				SecretBundle keyVaultSecret = getKeyVaultSecretTask.Result;
				secret = keyVaultSecret.Value;
			});
			keyVaultSecretTask.Wait();
#else
			secret = Client.GetSecret(secretName).Value.Value;
#endif
			return secret;
		}
	}
}
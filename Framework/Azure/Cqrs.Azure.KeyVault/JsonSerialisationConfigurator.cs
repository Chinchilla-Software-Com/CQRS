#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Azure.KeyVault
{
	/// <summary>
	/// Configures the <see cref="DefaultJsonSerializerSettings"/> to use the <see cref="KeyVaultSecretFactory"/> for secrets
	/// </summary>
	public class JsonSerialisationConfigurator
	{
		/// <summary>
		/// Configures the <see cref="DefaultJsonSerializerSettings"/> to use the <see cref="KeyVaultSecretFactory"/> for secrets
		/// obtaining the <see cref="IConfigurationManager"/> from the provided <paramref name="dependencyResolver"/>.
		/// </summary>
		public static void Configure(IDependencyResolver dependencyResolver)
		{
			Configure(dependencyResolver.Resolve<IConfigurationManager>());
		}

		/// <summary>
		/// Configures the <see cref="DefaultJsonSerializerSettings"/> to use the <see cref="KeyVaultSecretFactory"/> for secrets
		/// using the provided <paramref name="configurationManager"/>.
		/// </summary>
		public static void Configure(IConfigurationManager configurationManager)
		{
			DefaultJsonSerializerSettings.ReconfigureEncryptedContractResolver(new KeyVaultSecretFactory(configurationManager));
		}
	}
}
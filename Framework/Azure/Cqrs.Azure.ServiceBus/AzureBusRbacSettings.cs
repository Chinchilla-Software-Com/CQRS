#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// Settings for using RBAC instead of a connection string
	/// </summary>
	public class AzureBusRbacSettings
	{
		/// <summary>
		/// Gets or sets the endpoint to the bus.
		/// </summary>
		public string Endpoint { get; set; }

		/// <summary>
		/// Gets or sets the Application Id.
		/// </summary>
		public string ApplicationId { get; set; }

		/// <summary>
		/// Gets or sets the Client Key/Secret.
		/// </summary>
		public string ClientKey { get; set; }

		/// <summary>
		/// Gets or sets the Tenant Id.
		/// </summary>
		public string TenantId { get; set; }

		/// <summary>
		/// Gets the default authority combining the <see cref="TenantId" />
		/// </summary>
		public string GetDefaultAuthority()
		{
			return $"https://login.windows.net/{TenantId}";
		}

		/// <summary>
		/// Returns a string that represents the current object. This May contain sensitive data
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return $"Endpoint: {Endpoint}, ApplicationId: {ApplicationId}, ClientKey: {ClientKey}, TenantId: {TenantId},";
		}
	}
}
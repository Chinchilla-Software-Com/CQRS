#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="IServiceResponse"/> that provides version information.
	/// </summary>
	public interface IVersionedServiceResponse : IServiceResponse
	{
		/// <summary>
		/// The version of the data being returned
		/// </summary>
		double Version { get; set; }
	}
}
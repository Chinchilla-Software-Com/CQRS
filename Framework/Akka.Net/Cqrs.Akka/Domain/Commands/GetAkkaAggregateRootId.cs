#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Domain;

namespace Cqrs.Akka.Domain.Commands
{
	/// <summary>
	/// Get the <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> from the Akka.net system.
	/// </summary>
	public class GetAkkaAggregateRootId
	{
	}
}
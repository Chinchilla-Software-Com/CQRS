#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers command handlers that listen and respond to commands.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/CommandHandlerRegistrar")]
	public interface ICommandHandlerRegistrar : IHandlerRegistrar
	{
	}
}
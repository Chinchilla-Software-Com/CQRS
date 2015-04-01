#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Commands;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	public class TestCommand : ICommand<Guid>
	{
		#region Implementation of IMessageWithAuthenticationToken<Guid>

		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of ICommand<Guid>

		public int ExpectedVersion { get; set; }

		#endregion

		public Guid Id { get; set; }

		#region Implementation of IMessage

		public string CorrolationId { get; set; }

		#endregion
	}
}
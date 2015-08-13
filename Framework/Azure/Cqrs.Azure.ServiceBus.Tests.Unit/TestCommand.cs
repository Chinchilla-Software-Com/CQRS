#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Commands;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	public class TestCommand : ICommand<Guid>
	{
		#region Implementation of IMessageWithAuthenticationToken<Guid>

		[DataMember]
		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of ICommand<Guid>

		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion

		[DataMember]
		public Guid Id { get; set; }

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}
}
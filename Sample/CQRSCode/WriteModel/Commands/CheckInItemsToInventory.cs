using System;
using Cqrs.Commands;
using Cqrs.Authentication;

namespace CQRSCode.WriteModel.Commands
{
	public class CheckInItemsToInventory : ICommand<ISingleSignOnToken>
	{
		public readonly int Count;

		public CheckInItemsToInventory(Guid id, int count, int originalVersion) 
		{
			Id = id;
			Count = count;
			ExpectedVersion = originalVersion;
		}

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrelationId { get; set; }

		[Obsolete("Use CorrelationId")]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}
}
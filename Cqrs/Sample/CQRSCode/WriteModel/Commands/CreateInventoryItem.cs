using System;
using Cqrs.Commands;
using Cqrs.Authentication;

namespace CQRSCode.WriteModel.Commands
{
	public class CreateInventoryItem : ICommand<ISingleSignOnToken>
	{
		public readonly string Name;
		
		public CreateInventoryItem(Guid id, string name)
		{
			Id = id;
			Name = name;
		}

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}
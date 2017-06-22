namespace KendoUI.Northwind.Dashboard.Code.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Commands;
	using Cqrs.Entities;
	using Models;

	/// <summary>
	/// A <see cref="ICommand{TAuthenticationToken}"/> that requests an existing order gets deleted.
	/// </summary>
	public class DeleteOrderCommand : ICommand<string>
	{
		#region Implementation of ICommand

		[DataMember]
		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}

		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<string>

		[DataMember]
		public string AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="T:Cqrs.Messages.IMessage"/> has been delivered to/sent via already.
		/// </summary>
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		[DataMember]
		public Guid Rsn { get; set; }

		public DeleteOrderCommand()
		{
		}

		public DeleteOrderCommand(Guid rsn)
		{
			Rsn = rsn;
		}

		/// <summary>
		/// Explicit cast of <see cref="OrderViewModel"/> to <see cref="DeleteOrderCommand"/>
		/// </summary>
		/// <param name="order">A <see cref="OrderViewModel"/> <see cref="Entity"/> to convert</param>
		/// <returns>A <see cref="CreateOrderCommand"/> object</returns>
		public static explicit operator DeleteOrderCommand(OrderViewModel order)
		{
			var result = new DeleteOrderCommand(order.Rsn);

			return result;
		}
	}
}
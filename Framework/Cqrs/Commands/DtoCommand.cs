#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Commands
{
	/// <summary>
	/// A <see cref="ICommand{TPermissionToken}"/> for <see cref="IDto"/> objects
	/// </summary>
	[Serializable]
	[DataContract]
	public class DtoCommand<TAuthenticationToken, TDto> : ICommand<TAuthenticationToken>
		where TDto : IDto
	{
		[DataMember]
		public TDto Original { get; set; }

		[DataMember]
		public TDto New { get; set; }

		public DtoCommand(Guid id, TDto original, TDto @new)
		{
			Id = id;
			Original = original;
			New = @new;
		}

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[DataMember]
		[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
		public FrameworkType Framework { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		public IEnumerable<string> Frameworks { get; set; }

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
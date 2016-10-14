#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Cqrs.Messages
{
	public interface IMessage
	{
		[Obsolete("Use CorrelationId")]
		Guid CorrolationId { get; set; }

		Guid CorrelationId { get; set; }

		[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
		FrameworkType Framework { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		IEnumerable<string> Frameworks { get; set; }
	}
}
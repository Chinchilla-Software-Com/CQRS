#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Messages
{
	public interface IMessage
	{
		[Obsolete("Use CorrelationId")]
		Guid CorrolationId { get; set; }

		Guid CorrelationId { get; set; }

		FrameworkType Framework { get; set; }
	}
}
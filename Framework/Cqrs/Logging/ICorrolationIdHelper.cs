#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Logging
{
	public interface ICorrolationIdHelper
	{
		Guid GetCorrolationId();

		Guid SetCorrolationId(Guid corrolationId);
	}
}
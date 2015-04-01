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
	/// <summary>
	/// Always returns <see cref="Guid.Empty"/>
	/// </summary>
	public class NullCorrolationIdHelper : ICorrolationIdHelper
	{
		public Guid GetCorrolationId()
		{
			return Guid.Empty;
		}

		public Guid SetCorrolationId(Guid corrolationId)
		{
			return Guid.Empty;
		}
	}
}
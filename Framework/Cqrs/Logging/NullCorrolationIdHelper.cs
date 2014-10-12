#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Logging
{
	/// <summary>
	/// Always returns null
	/// </summary>
	public class NullCorrolationIdHelper : ICorrolationIdHelper
	{
		public string GetCorrolationId()
		{
			return null;
		}
	}
}
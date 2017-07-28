#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Events
{
	/// <summary>
	/// The type of CRUD operation.
	/// </summary>
	public enum DtoAggregateEventType
	{
		/// <summary>
		/// An unknown CRUD operations.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The Create CRUD operations.
		/// </summary>
		Created = 1,

		/// <summary>
		/// The Update CRUD operations.
		/// </summary>
		Updated = 3,

		/// <summary>
		/// The Delete CRUD operations.
		/// </summary>
		Deleted = 4
	}
}
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Events
{
	public enum DtoAggregateEventType
	{
		Unknown = 0,
		Created = 1,
		Updated = 3,
		Deleted = 4
	}
}
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure Table storage.
	/// </summary>
	public interface IEntityTableEntity<TEntity>
	{
		/// <summary>
		/// Gets or sets the <typeparamref name="TEntity"/>.
		/// </summary>
		TEntity Entity { get; set; }
	}
}
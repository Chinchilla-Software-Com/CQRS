#if NET40

#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Data.Linq;
using Cqrs.Entities;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A collection of extension methods for <see cref="Table{TData}"/>
	/// </summary>
	public static class LinqToSqlSqlDataStoreExtension
	{
		/// <summary>
		/// Calls the TRUNCATE SQL command on the <see cref="Table{TEntity}"/>.
		/// </summary>
		/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> to truncate all data of.</typeparam>
		/// <param name="table">The <see cref="Table{TEntity}"/> to truncate all data of.</param>
		public static void Truncate<TEntity>(this Table<TEntity> table) where TEntity : class
		{
			Type rowType = table.GetType().GetGenericArguments()[0];
			string tableName = table.Context.Mapping.GetTable(rowType).TableName;
			const string sqlCommand = "TRUNCATE TABLE {0}";
			table.Context.ExecuteCommand(string.Format(sqlCommand, tableName));
		}
	}
}
#endif
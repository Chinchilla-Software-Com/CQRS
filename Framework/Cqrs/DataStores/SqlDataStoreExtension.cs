#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Reflection;
using System.Linq;
#if NET40
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
#endif
using Cqrs.Entities;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A collection of extension methods for <see cref="DbSet{TData}"/>
	/// </summary>
	public static class SqlDataStoreExtension
	{
		/// <summary>
		/// Calls the TRUNCATE SQL command on the <see cref="DbSet{TEntity}"/>.
		/// </summary>
		/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> to truncate all data of.</typeparam>
		/// <param name="table">The <see cref="DbSet{TEntity}"/> to truncate all data of.</param>
		public static void Truncate<TEntity>(this DbSet<TEntity> table) where TEntity : class
		{
			var context = table.GetContext();
			string tableName = table.GetTableName(context);
			const string sqlCommand = "TRUNCATE TABLE {0}";
#pragma warning disable EF1000 // Review SQL queries for security vulnerabilities
#if NETCOREAPP3_0
			_ = context.Database.ExecuteSqlRaw(string.Format(sqlCommand, tableName));
#else
			_ = context.Database.ExecuteSqlCommand(string.Format(sqlCommand, tableName));
#endif
#pragma warning restore EF1000 // Review SQL queries for security vulnerabilities
		}

		private static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet)
			where TEntity : class
		{
			object internalSet = dbSet
				.GetType()
				.GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(dbSet);
			object internalContext = internalSet
				.GetType()
				.BaseType
				.GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue(internalSet);
			return (DbContext)internalContext
				.GetType()
				.GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public)
				.GetValue(internalContext, null);
		}

		private static string GetTableName<TEntity>(this DbSet<TEntity> dbSet, DbContext dbContext = null) where TEntity : class
		{
			dbContext = dbContext ?? dbSet.GetContext();
#if NET40
			string tableName = (dbContext as IObjectContextAdapter).ObjectContext.CreateObjectSet<TEntity>().EntitySet.Name; ;
#else
			IModel model = dbContext.Model;
			IEnumerable<IEntityType> entityTypes = model.GetEntityTypes();
			IEntityType entityType = entityTypes.First(t => t.ClrType == typeof(TEntity));
			IAnnotation tableNameAnnotation = entityType.GetAnnotation("Relational:TableName");
			string tableName = tableNameAnnotation.Value.ToString();
#endif
			return tableName;
		}
	}
}
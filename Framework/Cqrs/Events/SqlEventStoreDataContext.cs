#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;

#if NET40
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
#else
using Microsoft.EntityFrameworkCore;
#endif
#if NETCOREAPP3_1_OR_GREATER
using Microsoft.EntityFrameworkCore.Diagnostics;
#endif
using System.Linq;

namespace Cqrs.Events
{
	/// <summary>
	/// A custom <see cref="DbContext"/> that supports specifying a table name.
	/// </summary>
	public class SqlEventStoreDataContext : DbContext
	{
		/// <summary>
		/// Name of Table this entity will use
		/// </summary>
		protected string TableName { get; private set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlEventStoreDataContext"/> class using the given string as the name or connection string for the database to which a connection will be made. See the class remarks for how this is used to create a connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		public SqlEventStoreDataContext(string nameOrConnectionString)
#if NET40
			: base(nameOrConnectionString) { }
#else
			: base()
		{
			NameOrConnectionString = nameOrConnectionString;
		}
#endif

#if NET40
#else
		private string NameOrConnectionString { get; }

#if NETCOREAPP3_1_OR_GREATER
		private static  DbCommandInterceptor Interceptor { get; }
#endif

		/// <summary>
		/// Override this method to configure the database (and other options) to be used for this context. This method is called for each instance of the context that is created. The base implementation does nothing.
		/// In situations where an instance of Microsoft.EntityFrameworkCore.DbContextOptions may or may not have been passed to the constructor, you can use Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured to determine if the options have already been set, and skip some or all of the logic in Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder).
		/// </summary>
		/// <param name="optionsBuilder">A builder used to create or modify options for this context. Databases (and other extensions) typically define extension methods on this object that allow you to configure the context.</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
#if NETCOREAPP3_1_OR_GREATER
			optionsBuilder.UseSqlServer(NameOrConnectionString)
			  .AddInterceptors(Interceptor);
#else
			optionsBuilder.UseSqlServer(NameOrConnectionString);
#endif

			base.OnConfiguring(optionsBuilder);
		}
#endif

		static SqlEventStoreDataContext()
		{
#if NET40
			DbInterception.Add(new QueueMessageInterceptor());
#endif
#if NETCOREAPP3_1_OR_GREATER
			Interceptor = new QueueMessageInterceptor();
#endif
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlEventStoreDataContext"/> class using the given string as the name or connection string for the database to which a connection will be made. See the class remarks for how this is used to create a connection.
		/// </summary>
		/// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
		/// <param name="tableName">The name of the table to use for this entity.</param>
		protected SqlEventStoreDataContext(string nameOrConnectionString, string tableName)
			: this(nameOrConnectionString)
		{
			TableName = tableName;
		}

#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER
		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlEventStoreDataContext"/> class by referencing a file source.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="fileOrServerOrConnection">
		/// This argument can be any one of the following: 
		/// The name of a file where a SQL Server Express database resides. 
		/// The name of a server where a database is present. In this case the provider uses the default database for a user. 
		/// A complete connection string. LINQ to SQL just passes the string to the provider without modification.
		/// </param>
		public static SqlEventStoreDataContext New(string tableName, string fileOrServerOrConnection)
		{
			throw new NotSupportedException("Upgrade to .NET Core 3.0 or downgrade to .NET Framework 4.0 in-order to specify table name overrides.");
		}
#else
		/// <summary>
		/// Instantiates a new instance of the <see cref="SqlEventStoreDataContext"/> class by referencing a file source.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="fileOrServerOrConnection">
		/// This argument can be any one of the following: 
		/// The name of a file where a SQL Server Express database resides. 
		/// The name of a server where a database is present. In this case the provider uses the default database for a user. 
		/// A complete connection string. LINQ to SQL just passes the string to the provider without modification.
		/// </param>
		public static SqlEventStoreDataContext New(string tableName, string fileOrServerOrConnection)
		{
			return new SqlEventStoreDataContext(tableName, fileOrServerOrConnection);
		}
#endif

#if NETCOREAPP3_1_OR_GREATER
		internal class QueueMessageInterceptor : DbCommandInterceptor
		{
			private const string TableReplaceString = "[EventStore]";

			private void ReplaceTableName(DbCommand command, CommandEventData eventData)
			{
				var myContext = eventData.Context as SqlEventStoreDataContext;
				if (myContext != null && command != null && command.CommandText.Contains(TableReplaceString))
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
					command.CommandText = command.CommandText.Replace(TableReplaceString, $"[{myContext.TableName}]");
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
			}

			public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}

			public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}

			public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}

			public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}

			public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}

			public override object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object result)
			{
				ReplaceTableName(command, eventData);
				return result;
			}
		}
#endif
#if NET40
		internal class QueueMessageInterceptor : IDbCommandInterceptor
		{
			private const string TableReplaceString = "[EventStore]";

			private void ReplaceTableName(DbCommand command, IEnumerable<DbContext> contexts)
			{
				var myContext = contexts?.FirstOrDefault(x => x is SqlEventStoreDataContext) as SqlEventStoreDataContext;
				if (myContext != null && command != null && command.CommandText.Contains(TableReplaceString))
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
					command.CommandText = command.CommandText.Replace(TableReplaceString, $"[{myContext.TableName}]");
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
			}

			public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}

			public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}

			public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}

			public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}

			public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}

			public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
			{
				ReplaceTableName(command, interceptionContext.DbContexts);
			}
		}
#endif
	}
}
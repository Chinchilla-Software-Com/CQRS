#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Exceptions;

namespace Cqrs.Events
{
	/// <summary>
	/// A simplified SqlServer based <see cref="EventStore{TAuthenticationToken}"/> that uses LinqToSql and follows a rigid schema that also replicates to multiple connections, but only reads from one connection.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class ReplicatedSqlEventStore<TAuthenticationToken> : SqlEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// A collection of connection strings that are used to write to the database.
		/// </summary>
		protected IEnumerable<string> WritableConnectionStrings { get; private set; }

		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="ReplicatedSqlEventStore{TAuthenticationToken}"/> class.
		/// </summary>
		public ReplicatedSqlEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IConfigurationManager configurationManager)
			: base(eventBuilder, eventDeserialiser, logger, configurationManager)
		{
			var writableConnectionStrings = new List<string>();

			string connectionStringkey;
			if (!ConfigurationManager.TryGetSetting(SqlEventStoreConnectionNameApplicationKey, out connectionStringkey) || string.IsNullOrEmpty(connectionStringkey))
				throw new MissingApplicationSettingException(SqlEventStoreConnectionNameApplicationKey);
			string connectionString;
			int writeIndex = 1;
			while (!string.IsNullOrWhiteSpace(connectionStringkey))
			{
				try
				{
					connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringkey].ConnectionString;
				}
				catch (NullReferenceException exception)
				{
					throw new MissingConnectionStringException(connectionStringkey, exception);
				}
				writableConnectionStrings.Add(connectionString);
				if (!ConfigurationManager.TryGetSetting($"{SqlEventStoreConnectionNameApplicationKey}.{writeIndex}", out connectionStringkey) || string.IsNullOrEmpty(connectionStringkey))
					connectionStringkey = null;
				writeIndex++;
			}

			WritableConnectionStrings = writableConnectionStrings;
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into each SQL Server in <see cref="WritableConnectionStrings"/>.
		/// A single <see cref="TransactionScope"/> wraps all SQL servers, so all must complete successfully, or they will ALL roll back.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override
#if NET40
			void PersistEvent
#else
			async Task PersistEventAsync
#endif
				(EventData eventData)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					IList<Task> persistTasks = new List<Task>();
					foreach (string connectionString in WritableConnectionStrings)
					{
						// Do not remove this variable copying or the parallel task stuff will bork.
						var safeConnectionString = connectionString;
						DependentTransaction subTransaction = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
						Task task = Task.Factory.StartNewSafely
						(
							(subTransactionObject) =>
							{
								var subTrx = (DependentTransaction) subTransactionObject;
								//Pass the DependentTransaction to the scope, so that work done in the scope becomes part of the transaction passed to the worker thread
								using (TransactionScope ts = new TransactionScope(subTrx))
								{
									using (SqlEventStoreDataContext dbDataContext = new SqlEventStoreDataContext(safeConnectionString))
#if NET40
										Add(dbDataContext, eventData);
#else
										Task.Run(async () => {
											await AddAsync(dbDataContext, eventData);
										}).Wait();
#endif

									//Call complete on the transaction scope
									ts.Complete();
								}

								//Call complete on the dependent transaction
								subTrx.Complete();
							},
							subTransaction
						);
						persistTasks.Add(task);
					}

					bool anyFailed = Task.Factory.ContinueWhenAll(persistTasks.ToArray(), tasks =>
					{
						return tasks.Any(task => task.IsFaulted);
					}).Result;
					if (anyFailed)
						throw new AggregateException("Persisting data to the SQL event store failed. Check the logs for more details.");
					scope.Complete();
				}
			}
			catch (TransactionException exception)
			{
				Logger.LogError("There was an issue with the SQL transaction persisting data to the SQL event store.", exception: exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue persisting data to the SQL event store.", exception: exception);
				throw;
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}
	}
}
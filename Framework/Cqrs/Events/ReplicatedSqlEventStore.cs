#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using cdmdotnet.Logging;
using Cqrs.Configuration;

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
				throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", SqlEventStoreConnectionNameApplicationKey));
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
					throw new NullReferenceException(string.Format("No connection string setting named '{0}' was found in the configuration file with the SQL Event Store connection string.", connectionStringkey), exception);
				}
				writableConnectionStrings.Add(connectionString);
				if (!ConfigurationManager.TryGetSetting(string.Format("{0}.{1}", SqlEventStoreConnectionNameApplicationKey, writeIndex), out connectionStringkey) || string.IsNullOrEmpty(connectionStringkey))
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
		protected override void PersistEvent(EventData eventData)
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
									using (DataContext dbDataContext = new DataContext(safeConnectionString))
										Add(dbDataContext, eventData);

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
		}
	}
}
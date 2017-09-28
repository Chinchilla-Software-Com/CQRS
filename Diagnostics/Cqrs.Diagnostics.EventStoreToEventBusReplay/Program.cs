using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Serialisers;
using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.MongoDB.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace Cqrs.Diagnostics.EventStoreToEventBusReplay
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			int argumentCount = args.Length;
			new Program<SingleSignOnToken>().Run();
		}
	}

	class Program<TAuthenticationToken>
	{
		private bool LoadAllDataFirst { get; set; }

		public virtual void Run()
		{
			bool loadAllDataFirst;
			if (!bool.TryParse(ConfigurationManager.AppSettings["LoadAllDataFirst"], out loadAllDataFirst))
				loadAllDataFirst = true;
			LoadAllDataFirst = loadAllDataFirst;

			StartResolver();

			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo(LoadAllDataFirst ? "All data will be loaded first." : "Events will be sent to the event bus as they are found.", "Run");

			// Find event store
			IEventStore<TAuthenticationToken> eventStore = FindEventStore();

			// Find event publisher
			IEventPublisher<TAuthenticationToken> eventPublisher = FindEventPublisher();

			// Load events from event store
			string fromDateValue = ConfigurationManager.AppSettings["FromDate"];
			DateTime? fromDate = null;
			if (!string.IsNullOrWhiteSpace(fromDateValue))
			{
				DateTime fromDate1;
				if (DateTime.TryParse(fromDateValue, out fromDate1))
					fromDate = fromDate1;
			}
			string toDateValue = ConfigurationManager.AppSettings["ToDate"];
			DateTime? toDate = null;
			if (!string.IsNullOrWhiteSpace(toDateValue))
			{
				DateTime toDate1;
				if (DateTime.TryParse(toDateValue, out toDate1))
					toDate = toDate1;
			}
			IEnumerable<IEvent<TAuthenticationToken>> events = LoadEventsFromEventStore(eventPublisher, eventStore, fromDate, toDate, ScanAndPickTypes());

			if (LoadAllDataFirst)
			{
				// Publish on event bus.
				PublishEventsOnTheEventBus(eventPublisher, events);
			}
		}

		protected virtual void StartResolver()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<TAuthenticationToken, AuthenticationTokenHelper<TAuthenticationToken>>(false, false, true));
			NinjectDependencyResolver.ModulesToLoad.Add(new AzureEventBusPublisherModule<TAuthenticationToken>());
			NinjectDependencyResolver.ModulesToLoad.Add(new MongoDbEventStoreModule<TAuthenticationToken>());
			NinjectDependencyResolver.Start();
		}

		/// <summary>
		/// Read the "EventStore" appSetting and using reflection load said <see cref="IEventStore{TAuthenticationToken}">event store</see>
		/// </summary>
		protected virtual IEventStore<TAuthenticationToken> FindEventStore()
		{
			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo("Locating event store.", "FindEventStore");
			return NinjectDependencyResolver.Current.Resolve<IEventStore<TAuthenticationToken>>();
		}

		/// <summary>
		/// Read the "EventBus" appSetting and using reflection load said <see cref="IEventPublisher{TAuthenticationToken}">event bus</see>
		/// </summary>
		protected virtual IEventPublisher<TAuthenticationToken> FindEventPublisher()
		{
			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo("Locating event publisher.", "FindEventPublisher");
			return NinjectDependencyResolver.Current.Resolve<IEventPublisher<TAuthenticationToken>>();
		}

		protected virtual Type[] ScanAndPickTypes()
		{
			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo("Scanning all assemblies for events.", "ScanAndPickTypes");
			string folder = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
			IEnumerable<string> fileNames = Directory.EnumerateFiles(folder, "*.dll");
			IList<Type> allLocatedTypes = new List<Type>();
			Type iEventType = typeof (IEvent<TAuthenticationToken>);
			int index = 0;
			Console.WriteLine("0: All Events");

			string[] extraDataTypesToLoad = ConfigurationManager.AppSettings["Cqrs.MongoDb.ExtraDataTypesToLoad"].Split(';');
			Type basicStructSerialiserType = typeof (BasicStructSerialiser<>);

			foreach (string fileName in fileNames)
			{
				Assembly assembly = Assembly.LoadFile(fileName);
				List<Type> types = assembly
					.GetTypes()
					.Where(type => iEventType.IsAssignableFrom(type) || extraDataTypesToLoad.Any(extraDataType => type.AssemblyQualifiedName.StartsWith(extraDataType)))
					.ToList();

				if (!types.Any())
					continue;

				Console.WriteLine("Assembly " + assembly.FullName + ":");

				foreach (Type type in types)
				{
					if (!iEventType.IsAssignableFrom(type))
					{
						Type thisBasicStructSerialiserType = basicStructSerialiserType.MakeGenericType(type);
						BsonSerializer.RegisterSerializer(type, (IBsonSerializer)Activator.CreateInstance(thisBasicStructSerialiserType));
						continue;
					}
					Console.WriteLine("\t" + index++ + ": " + type.FullName);
					allLocatedTypes.Add(type);

					var classMap = new BsonClassMap(type);
					classMap.AutoMap();
					BsonClassMap.RegisterClassMap(classMap);
				}
			}

			Console.WriteLine("Enter the events you want: e.g. 1,14,57");
			string indexes = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(indexes) || indexes == "0")
				return allLocatedTypes.ToArray();

			string[] indexParts = indexes.Split(',');
			IEnumerable<Type> selectedTypes = indexParts.Select(indexPart => allLocatedTypes[int.Parse(indexPart.Trim())]);

			return selectedTypes.ToArray();
		}

		/// <summary>
		/// Scan the <see cref="IEventStore{TAuthenticationToken}">event store</see>
		/// </summary>
		protected virtual IEnumerable<IEvent<TAuthenticationToken>> LoadEventsFromEventStore(IEventPublisher<TAuthenticationToken> eventPublisher, IEventStore<TAuthenticationToken> eventStore, DateTime? fromDate = null, DateTime? toDate = null, params Type[] eventTypes)
		{
			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			MongoDbEventStore<TAuthenticationToken> mongoDbEventStore = eventStore as MongoDbEventStore<TAuthenticationToken>;
			if (mongoDbEventStore != null)
			{
				IMongoCollection<MongoDbEventData> mongoCollection = new ExposedMongoEventStore(mongoDbEventStore).MongoCollection;

				IMongoQueryable<MongoDbEventData> query = mongoCollection
					.AsQueryable()
					.OrderBy(eventData => eventData.Timestamp);

				IList<string> eventTypeNames = new List<string>();

				Expression<Func<MongoDbEventData, bool>> subQuery = PredicateBuilder.True<MongoDbEventData>();
				foreach (Type eventType in eventTypes)
				{
					string assemblyQualifiedName = eventType.AssemblyQualifiedName;
					string[] assemblyQualifiedNameParts = assemblyQualifiedName.Split(',');
					if (assemblyQualifiedNameParts.Length > 0)
						assemblyQualifiedName = assemblyQualifiedNameParts[0].Trim() + ", " + assemblyQualifiedNameParts[1].Trim();
					eventTypeNames.Add(assemblyQualifiedName);
					subQuery = subQuery.Or(eventData => eventData.EventType.StartsWith(assemblyQualifiedName));
				}

				if (fromDate != null)
					query = query.Where(eventData => eventData.Timestamp >= fromDate.Value);
				if (toDate != null)
					query = query.Where(eventData => eventData.Timestamp <= toDate.Value);

				int skipCount = 0;
				int limitCount;
				if (!int.TryParse(ConfigurationManager.AppSettings["Cqrs.MongoDb.RecordsetSize"], out limitCount))
					limitCount = 50000;
				int resultsCount = 0;
				IMongoQueryable<MongoDbEventData> runQuery = query.Take(limitCount).Skip(skipCount);
				var results = new List<IEvent<TAuthenticationToken>>();
				do
				{
					IList<MongoDbEventData> subResults = runQuery.ToList();
					if (!subResults.Any())
						break;

					foreach (MongoDbEventData eventData in subResults.Where(eventData => eventTypeNames.Any(eventTypeName => eventData.EventType.StartsWith(eventTypeName))))
					{
						IEvent<TAuthenticationToken> @event = NinjectDependencyResolver.Current.Resolve<IEventDeserialiser<TAuthenticationToken>>().Deserialise(eventData);
						if (LoadAllDataFirst)
							results.Add(@event);
						else
							PublishEventOnTheEventBus(logger, eventPublisher, @event);
						resultsCount++;
					}

					logger.LogProgress(string.Format("Captured {0:N0} items, walked past {1:N0} items and currently have {2:N0} events.", subResults.Count, skipCount, resultsCount), "LoadEventsFromEventStore");
					skipCount = skipCount + subResults.Count;
					limitCount = limitCount + subResults.Count;
					runQuery = query.Take(limitCount).Skip(skipCount);
				} while (true);

				return results;
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Publish the loaded <see cref="IEvent{TAuthenticationToken}">events</see> on <see cref="IEventPublisher{TAuthenticationToken}">event bus</see>.
		/// </summary>
		protected virtual void PublishEventsOnTheEventBus(IEventPublisher<TAuthenticationToken> eventPublisher, IEnumerable<IEvent<TAuthenticationToken>> events)
		{
			var logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
			IList<IEvent<TAuthenticationToken>> eventsToPublish = events.ToList();
			int eventCount = eventsToPublish.Count;
			for (int index = 0; index < eventCount; index++)
			{
				IEvent<TAuthenticationToken> eventData = eventsToPublish[index];
				PublishEventOnTheEventBus(logger, eventPublisher, eventData);

				if (index++ % 50 == 0)
				{
					// The double casts are needed so it actually reports a non integer number of ZERO
					logger.LogProgress(string.Format("Publishing events is at {0:N0} of {1:N0} - {2:N2}%", index, eventCount, ((double)index / (double)eventCount) * (double)100), "PublishEventsOnTheEventBus");
				}
			}
		}

		/// <summary>
		/// Publish the loaded <see cref="IEvent{TAuthenticationToken}">events</see> on <see cref="IEventPublisher{TAuthenticationToken}">event bus</see>.
		/// </summary>
		protected virtual void PublishEventOnTheEventBus(ILogger logger, IEventPublisher<TAuthenticationToken> eventPublisher, IEvent<TAuthenticationToken> @event)
		{
			var eventWithIdentity = @event as IEventWithIdentity<TAuthenticationToken>;
			Guid rsn = @event.GetIdentity();
			// Flush tracking information so it gets processed again.
			@event.Frameworks = null;
			@event.OriginatingFramework = null;
			int loopCount = 0;
			do
			{
				try
				{
					eventPublisher.Publish(@event);
					return;
				}
				catch (TimeoutException)
				{
					loopCount++;
				}
				catch (Exception exception)
				{
					string exceptionMessage = eventWithIdentity == null
						? "Publishing event {0} failed to be published."
						: "Publishing event {0} with identifier {1} failed to be published.";
					logger.LogError(string.Format(exceptionMessage, @event.Id, rsn), "PublishEventOnTheEventBus", exception);
					return;
				}
			} while (loopCount < 10);
			string message = eventWithIdentity == null
				? "Publishing event {0} failed to be published due to an event bus timeout... we tried 10 times before moving on."
				: "Publishing event {0} with identifier {1} failed to be published due to an event bus timeout... we tried 10 times before moving on.";
			logger.LogError(string.Format(message, @event.Id, rsn), "PublishEventOnTheEventBus");
		}

		public class ExposedMongoEventStore
		{
			public ExposedMongoEventStore(MongoDbEventStore<TAuthenticationToken> mongoDbEventStore)
			{
				PropertyInfo propertyInfo = mongoDbEventStore.GetType().GetProperty("MongoCollection", BindingFlags.Instance | BindingFlags.NonPublic);

				MongoCollection = (IMongoCollection<MongoDbEventData>)propertyInfo.GetValue(mongoDbEventStore);
			}

			internal IMongoCollection<MongoDbEventData> MongoCollection { get; set; }
		}
	}

	public static class PredicateBuilder
	{
		public static Expression<Func<T, bool>> True<T>() { return f => true; }
		public static Expression<Func<T, bool>> False<T>() { return f => false; }
		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
		}
	}
}
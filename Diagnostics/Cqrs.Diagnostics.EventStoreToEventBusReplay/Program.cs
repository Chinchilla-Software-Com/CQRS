using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Serialisers;
using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.MongoDB.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Ninject.Modules;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace Cqrs.Diagnostics.EventStoreToEventBusReplay
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			int argumentCount = args.Length;

			Console.WriteLine(@"This application will replay a collection of events.

It works by scanning any subfolders for .net assemblies and looking for events it can republish on an event bus.

For this to work we suggest you create a subfolder and place any assemblies you want to process in that folder.
You will also need to adjust/modify the .config file for this application with any required configuration your assemblies need. Once you have done that you will be ready to proceed.

Have you created your subfolder, added your assemblies and configured the .config file? Press Y if so to proceed. Press any other key to exit the application and configure your environment.

Press Y to proceed.");
			ConsoleKeyInfo character = Console.ReadKey();
			Console.WriteLine();
			if (!(new[] {'y', 'Y'}.Contains(character.KeyChar)))
				return;

			IProgram program;
			string authenticationType = ConfigurationManager.AppSettings["Cqrs.AuthenticationTokenType"] ?? string.Empty;

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				program = new Program<int>();
			else if (authenticationType.ToLowerInvariant() == "guid")
				program = new Program<Guid>();
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				program = new Program<string>();
			else if (authenticationType == "SingleSignOnToken")
				program = new Program<SingleSignOnToken>();
			else if (authenticationType == "SingleSignOnTokenWithUserRsn")
				program = new Program<SingleSignOnTokenWithUserRsn>();
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn")
				program = new Program<SingleSignOnTokenWithCompanyRsn>();
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn")
				program = new Program<SingleSignOnTokenWithUserRsnAndCompanyRsn>();
			else if (authenticationType == "ISingleSignOnToken")
				program = new Program<ISingleSignOnToken>();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsn")
				program = new Program<ISingleSignOnTokenWithUserRsn>();
			else if (authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				program = new Program<ISingleSignOnTokenWithCompanyRsn>();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				program = new Program<ISingleSignOnTokenWithUserRsnAndCompanyRsn>();
			else
				program = new Program<Guid>();
			program.Run();
		}
	}

	interface IProgram
	{
		void Run();
	}

	class Program<TAuthenticationToken>
		: IProgram
	{
		private bool LoadAllDataFirst { get; set; }

		private IDictionary<string, Assembly> LoadableAssemblies { get; set; }

		public virtual void Run()
		{
			LoadableAssemblies = new Dictionary<string, Assembly>();

			bool loadAllDataFirst;
			if (!bool.TryParse(ConfigurationManager.AppSettings["LoadAllDataFirst"], out loadAllDataFirst))
				loadAllDataFirst = true;
			LoadAllDataFirst = loadAllDataFirst;

			StartResolver();

			var logger = DependencyResolver.Current.Resolve<ILogger>();
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

			IEnumerable<IEvent<TAuthenticationToken>> events = LoadEventsFromEventStore(eventPublisher, eventStore, fromDate, toDate, ScanAndPickTypes<IEvent<TAuthenticationToken>>("Events"));

			if (LoadAllDataFirst)
			{
				// Publish on event bus.
				PublishEventsOnTheEventBus(eventPublisher, events);
			}
		}

		protected virtual void StartResolver()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<TAuthenticationToken, AuthenticationTokenHelper<TAuthenticationToken>>(false, false, true));
			bool useAzureServiceBusEventBus = bool.Parse(ConfigurationManager.AppSettings["EventBus.AzureServiceBus"]);
			bool useInProcessEventBus = bool.Parse(ConfigurationManager.AppSettings["EventBus.InProcessBus"]);
			if (useAzureServiceBusEventBus)
				NinjectDependencyResolver.ModulesToLoad.Add(new AzureEventBusPublisherModule<TAuthenticationToken>());
			if (useInProcessEventBus)
				NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<TAuthenticationToken>());
			bool useMongoDbEventStore = bool.Parse(ConfigurationManager.AppSettings["EventStore.MongoDB"]);
			bool useSqlEventStore = bool.Parse(ConfigurationManager.AppSettings["EventStore.SQL"]);
			if (useMongoDbEventStore)
				NinjectDependencyResolver.ModulesToLoad.Add(new MongoDbEventStoreModule<TAuthenticationToken>());
			if (useSqlEventStore)
				NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<TAuthenticationToken>());

			string[] additionalNinjectModules = (ConfigurationManager.AppSettings["AdditionalNinjectModules"] ?? string.Empty).Split(',');
			if (additionalNinjectModules.Any())
			{
				IEnumerable<Type> ninjectModuleTypes = ScanAndPickTypes<NinjectModule>("Ninject Modules", true)
					.Where(type => additionalNinjectModules.Contains(type.FullName) || additionalNinjectModules.Contains(type.Name));

				foreach (Type ninjectModuleType in ninjectModuleTypes)
				{
					NinjectDependencyResolver.ModulesToLoad.Add((NinjectModule)Activator.CreateInstance(ninjectModuleType));
				}
			}

			NinjectDependencyResolver.Start();
			if (useInProcessEventBus)
			{
				Type[] eventHandlerTypes = ScanAndPickTypes<IEventHandler>("Event Handlers");
				Type iEventType = typeof(IEventHandler);
				var eventHandlerRegistrar = new BusRegistrar(DependencyResolver.Current);
				eventHandlerRegistrar.Register
				(
					true,
					eventHandlerRegistrar.ResolveEventHandlerInterface,
					true,
					eventHandlerTypes
						.Where(eventHandlerType => iEventType.IsAssignableFrom(eventHandlerType))
						.Select(eventHandlerType => new BusRegistrar.HandlerTypeInformation { Type = eventHandlerType, Interfaces = eventHandlerRegistrar.ResolveEventHandlerInterface(eventHandlerType) })
						.ToArray()
				);
			}
		}

		/// <summary>
		/// Read the "EventStore" appSetting and using reflection load said <see cref="IEventStore{TAuthenticationToken}">event store</see>
		/// </summary>
		protected virtual IEventStore<TAuthenticationToken> FindEventStore()
		{
			var logger = DependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo("Locating event store.", "FindEventStore");
			return DependencyResolver.Current.Resolve<IEventStore<TAuthenticationToken>>();
		}

		/// <summary>
		/// Read the "EventBus" appSetting and using reflection load said <see cref="IEventPublisher{TAuthenticationToken}">event bus</see>
		/// </summary>
		protected virtual IEventPublisher<TAuthenticationToken> FindEventPublisher()
		{
			var logger = DependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo("Locating event publisher.", "FindEventPublisher");
			return DependencyResolver.Current.Resolve<IEventPublisher<TAuthenticationToken>>();
		}

		protected virtual Type[] ScanAndPickTypes<TType>(string typeName, bool defaultToAll = false)
		{
			var logger = DependencyResolver.Current == null ? new ConsoleLogger(new LoggerSettings(), new NullCorrelationIdHelper()) : DependencyResolver.Current.Resolve<ILogger>();
			logger.LogInfo(string.Format("Scanning all assemblies for {0}.", typeName.ToLowerInvariant()), "ScanAndPickTypes");
			string folder = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
			List<string> fileNames = Directory.EnumerateFiles(folder, "*.dll").ToList();
			fileNames.AddRange
			(
				Directory.EnumerateDirectories(folder).SelectMany(subFolder => Directory.EnumerateFiles(subFolder, "*.dll"))
			);
			IList<Type> allLocatedTypes = new List<Type>();
			Type iEventType = typeof(TType);
			int index = 0;
			Console.WriteLine("{0}: All {1}", index++, typeName.ToLowerInvariant());

			string[] extraDataTypesToLoad = ConfigurationManager.AppSettings["Cqrs.MongoDb.ExtraDataTypesToLoad"].Split(';');
			Type basicStructSerialiserType = typeof(BasicStructSerialiser<>);

			// Pre-load all the assemblies so we don't get any error if out-of-order loading occurs.
			foreach (string fileName in fileNames)
				Assembly.LoadFrom(fileName);

			AppDomain.CurrentDomain.AssemblyResolve += (sender, arguments) =>
			{
				Assembly assembly;
				if (LoadableAssemblies.TryGetValue(arguments.Name, out assembly))
					return assembly;
				Console.WriteLine("You need to add {0} to your working folder as it couldn't be resolved{1}.", arguments.Name, arguments.RequestingAssembly == null ? null : " while loading " + arguments.RequestingAssembly);
				return null;
			};

			foreach (string fileName in fileNames)
			{
				Assembly assembly = Assembly.LoadFrom(fileName);
				List<Type> types = assembly
					.GetTypes()
					.Where(type => iEventType.IsAssignableFrom(type) || (extraDataTypesToLoad.Any(extraDataType => !string.IsNullOrWhiteSpace(extraDataType) && type.AssemblyQualifiedName.StartsWith(extraDataType))))
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
					if (!LoadableAssemblies.ContainsKey(assembly.FullName))
						LoadableAssemblies.Add(assembly.FullName, assembly);
					if (!LoadableAssemblies.ContainsKey(assembly.GetName().Name))
						LoadableAssemblies.Add(assembly.GetName().Name, assembly);

					var classMap = new BsonClassMap(type);
					classMap.AutoMap();
					if (!type.IsInterface)
					{
						try
						{
							BsonClassMap.RegisterClassMap(classMap);
						}
						catch { /* */ }
					}
				}
			}

			string indexes = null;
			if (!defaultToAll)
			{
				Console.WriteLine("Enter the {0} you want: e.g. 1,14,57,101-115,600", typeName.ToLowerInvariant());
				indexes = Console.ReadLine();
			}
			if (string.IsNullOrWhiteSpace(indexes) || indexes == "0")
				return allLocatedTypes.ToArray();

			string[] indexParts = indexes.Split(',');
			var selectedTypes = new List<Type>();
			foreach (string indexPart in indexParts)
			{
				int rangeIndex = indexPart.IndexOf("-");
				if (rangeIndex == -1)
					selectedTypes.Add(allLocatedTypes[int.Parse(indexPart.Trim()) - 1]);
				else
				{
					int skip = int.Parse(indexPart.Substring(0, rangeIndex).Trim()) - 1;
					int take = int.Parse(indexPart.Substring(rangeIndex + 1).Trim()) - skip;
					selectedTypes.AddRange(allLocatedTypes.Skip(skip).Take(take));
				}
			}

			return selectedTypes.ToArray();
		}

		/// <summary>
		/// Scan the <see cref="IEventStore{TAuthenticationToken}">event store</see>
		/// </summary>
		protected virtual IEnumerable<IEvent<TAuthenticationToken>> LoadEventsFromEventStore(IEventPublisher<TAuthenticationToken> eventPublisher, IEventStore<TAuthenticationToken> eventStore, DateTime? fromDate = null, DateTime? toDate = null, params Type[] eventTypes)
		{
			bool resumeOnError;
			if (!bool.TryParse(ConfigurationManager.AppSettings["ResumeOnError"], out resumeOnError))
				resumeOnError = false;

			var logger = DependencyResolver.Current.Resolve<ILogger>();
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
						try
						{
							IEvent<TAuthenticationToken> @event = DependencyResolver.Current.Resolve<IEventDeserialiser<TAuthenticationToken>>().Deserialise(eventData);
							if (LoadAllDataFirst)
								results.Add(@event);
							else
								PublishEventOnTheEventBus(logger, eventPublisher, @event);
							resultsCount++;
						}
						catch (Exception exception)
						{
							logger.LogError("Failed to process message", exception: exception);
							if (resumeOnError)
								continue;
							throw;
						}
					}

					logger.LogProgress(string.Format("Captured {0:N0} items, walked past {1:N0} items and currently have {2:N0} events.", subResults.Count, skipCount, resultsCount), "LoadEventsFromEventStore");
					skipCount = skipCount + subResults.Count;
					limitCount = limitCount + subResults.Count;
					runQuery = query.Take(limitCount).Skip(skipCount);
				} while (true);

				return results;
			}

			SqlEventStore<TAuthenticationToken> sqlEventStore = eventStore as SqlEventStore<TAuthenticationToken>;
			if (sqlEventStore != null)
			{
				IQueryable<EventData> query = new ExposedSqlEventStore(sqlEventStore).RawQuery
					.OrderBy(eventData => eventData.Timestamp);

				return LoadEventsFromEventStore(eventPublisher, query, fromDate, toDate, eventTypes);
			}

			throw new NotImplementedException();
		}

		protected virtual IEnumerable<IEvent<TAuthenticationToken>> LoadEventsFromEventStore(IEventPublisher<TAuthenticationToken> eventPublisher, IQueryable<EventData> query, DateTime? fromDate = null, DateTime? toDate = null, params Type[] eventTypes)
		{
			bool resumeOnError;
			if (!bool.TryParse(ConfigurationManager.AppSettings["ResumeOnError"], out resumeOnError))
				resumeOnError = false;

			var logger = DependencyResolver.Current.Resolve<ILogger>();
			IList<string> eventTypeNames = new List<string>();

			Expression<Func<EventData, bool>> subQuery = PredicateBuilder.True<EventData>();
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
			if (!int.TryParse(ConfigurationManager.AppSettings["Cqrs.SQL.RecordsetSize"], out limitCount))
				limitCount = 50000;
			int resultsCount = 0;
			IQueryable<EventData> runQuery = query.Take(limitCount).Skip(skipCount);
			var results = new List<IEvent<TAuthenticationToken>>();

			do
			{
				IList<EventData> subResults = runQuery.ToList();
				if (!subResults.Any())
					break;

				foreach (EventData eventData in subResults.Where(eventData => eventTypeNames.Any(eventTypeName => eventData.EventType.StartsWith(eventTypeName))))
				{
					try
					{
						IEvent<TAuthenticationToken> @event = DependencyResolver.Current.Resolve<IEventDeserialiser<TAuthenticationToken>>().Deserialise(eventData);
						if (LoadAllDataFirst)
							results.Add(@event);
						else
							PublishEventOnTheEventBus(logger, eventPublisher, @event);
						resultsCount++;
					}
					catch (Exception exception)
					{
						logger.LogError("Failed to process message", exception: exception);
						if (!resumeOnError)
						{
							Console.WriteLine("Review the above fatal issue, then press any key to continue.");
							Console.ReadKey();
						}
					}
				}

				logger.LogProgress(string.Format("Captured {0:N0} items, walked past {1:N0} items and currently have {2:N0} events.", subResults.Count, skipCount, resultsCount), "LoadEventsFromEventStore");
				skipCount = skipCount + subResults.Count;
				runQuery = query.Take(limitCount).Skip(skipCount);
			} while (true);

			return results;
		}


		/// <summary>
		/// Publish the loaded <see cref="IEvent{TAuthenticationToken}">events</see> on <see cref="IEventPublisher{TAuthenticationToken}">event bus</see>.
		/// </summary>
		protected virtual void PublishEventsOnTheEventBus(IEventPublisher<TAuthenticationToken> eventPublisher, IEnumerable<IEvent<TAuthenticationToken>> events)
		{
			var logger = DependencyResolver.Current.Resolve<ILogger>();
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

					bool resumeOnError;
					if (!bool.TryParse(ConfigurationManager.AppSettings["ResumeOnError"], out resumeOnError))
						resumeOnError = false;

					if (!resumeOnError)
					{
						Console.WriteLine("Review the above issue, then press any key to continue.");
						Console.ReadKey();
					}

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

		public class ExposedSqlEventStore
		{
			public ExposedSqlEventStore(SqlEventStore<TAuthenticationToken> sqlEventStore)
			{
				MethodInfo methodInfo = sqlEventStore.GetType().GetMethod("CreateDbDataContext", BindingFlags.Instance | BindingFlags.NonPublic);
				DataContext dbDataContext = (DataContext)methodInfo.Invoke(sqlEventStore, new object[] { null });
				methodInfo = sqlEventStore.GetType().GetMethod("GetEventStoreTable", BindingFlags.Instance | BindingFlags.NonPublic);

				RawQuery = ((Table<EventData>)methodInfo.Invoke(sqlEventStore, new object[] { dbDataContext }))
					.AsQueryable();
			}

			internal IQueryable<EventData> RawQuery { get; set; }
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
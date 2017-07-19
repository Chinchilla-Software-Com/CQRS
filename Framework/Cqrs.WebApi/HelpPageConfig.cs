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
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.Services;

namespace Cqrs.WebApi
{
	public static class HelpPageConfig<TAuthenticationToken>
	{
		public static IList<string> AssemblyXmlFileNames { get; set; }

		static HelpPageConfig()
		{
			AssemblyXmlFileNames = new List<string>();
		}

		public class UserCreatedEvent : IEvent<TAuthenticationToken>
		{
			#region Implementation of IEvent

			[DataMember]
			public Guid Id
			{
				get
				{
					return Rsn;
				}
				set
				{
					Rsn = value;
				}
			}

			[DataMember]
			public int Version { get; set; }

			[DataMember]
			public DateTimeOffset TimeStamp { get; set; }

			#endregion

			#region Implementation of IMessageWithAuthenticationToken<TSingleSignOnToken>

			[DataMember]
			public TAuthenticationToken AuthenticationToken { get; set; }

			#endregion

			#region Implementation of IMessage

			[DataMember]
			public Guid CorrelationId { get; set; }

			[DataMember]
			[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
			public FrameworkType Framework { get; set; }

			/// <summary>
			/// The originating framework this message was sent from.
			/// </summary>
			[DataMember]
			public string OriginatingFramework { get; set; }

			/// <summary>
			/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
			/// </summary>
			[DataMember]
			public IEnumerable<string> Frameworks { get; set; }

			[Obsolete("Use CorrelationId")]
			[DataMember]
			public Guid CorrolationId
			{
				get { return CorrelationId; }
				set { CorrelationId = value; }
			}

			#endregion

			public Guid Rsn { get; set; }

			public string Name { get; set; }

			public string EmailAddress { get; set; }
		}

		static TAuthenticationToken GenerateAuthenticationToken()
		{
			string authenticationType;
			if (!DependencyResolver.Current.Resolve<IConfigurationManager>().TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType))
				authenticationType = "Guid";

			TAuthenticationToken token = default(TAuthenticationToken);

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				token = (TAuthenticationToken)(object)123;
			else if (authenticationType.ToLowerInvariant() == "guid")
				token = (TAuthenticationToken)(object)Guid.NewGuid();
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				token = (TAuthenticationToken)(object)"UserToken123";
			else if (authenticationType == "SingleSignOnToken" || authenticationType == "ISingleSignOnToken")
				token = (TAuthenticationToken)(object)new SingleSignOnToken
				{
					Token = Guid.NewGuid().ToString("N"),
					DateIssued = DateTime.Now,
					TimeOfExpiry = DateTime.Now.AddMinutes(20)
				};
			else if (authenticationType == "SingleSignOnTokenWithUserRsn" || authenticationType == "ISingleSignOnTokenWithUserRsn")
				token = (TAuthenticationToken)(object)new SingleSignOnTokenWithUserRsn
				{
					Token = Guid.NewGuid().ToString("N"),
					DateIssued = DateTime.Now,
					TimeOfExpiry = DateTime.Now.AddMinutes(20),
					UserRsn = Guid.NewGuid()
				};
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn" || authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				token = (TAuthenticationToken)(object)new SingleSignOnTokenWithCompanyRsn
				{
					Token = Guid.NewGuid().ToString("N"),
					DateIssued = DateTime.Now,
					TimeOfExpiry = DateTime.Now.AddMinutes(20),
					CompanyRsn = Guid.NewGuid()
				};
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn" || authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				token = (TAuthenticationToken)(object)new SingleSignOnTokenWithUserRsnAndCompanyRsn
				{
					Token = Guid.NewGuid().ToString("N"),
					DateIssued = DateTime.Now,
					TimeOfExpiry = DateTime.Now.AddMinutes(20),
					UserRsn = Guid.NewGuid(),
					CompanyRsn = Guid.NewGuid()
				};

			return token;
		}

		public static IDictionary<Type, object> GetBasicSampleObjects()
		{
			var eventCorrelationId = Guid.NewGuid();
			var correlationId = Guid.NewGuid();

			var sameplEvent = new UserCreatedEvent
			{
				CorrelationId = correlationId,
				AuthenticationToken = GenerateAuthenticationToken(),
				EmailAddress = "john@smith.com",
				Frameworks = new List<string> { "Azure", "Amazon EC2" },
				Id = Guid.NewGuid(),
				Name = "John Smith",
				OriginatingFramework = "Azure",
				TimeStamp = DateTime.Now.AddMinutes(-3),
				Version = 1
			};

			return new Dictionary<Type, object>
			{
				{
					typeof(string),
					"sample string"
				},
				{
					typeof(IServiceResponse),
					new ServiceResponse
					{
						CorrelationId = correlationId,
						State = ServiceResponseStateType.Succeeded
					}
				},
				{
					typeof(IServiceRequestWithData<TAuthenticationToken, Guid>),
					new ServiceRequestWithData<TAuthenticationToken, Guid>
					{
						AuthenticationToken = GenerateAuthenticationToken(),
						CorrelationId = correlationId,
						Data = eventCorrelationId
					}
				},
				{
					typeof(IServiceResponseWithResultData<IEnumerable<EventData>>),
					new ServiceResponseWithResultData<IEnumerable<EventData>>
					{
						CorrelationId = correlationId,
						ResultData = new List<EventData>
						{
							new EventData
							{
								AggregateId = string.Format("{0}//{1}", typeof(UserCreatedEvent).FullName, sameplEvent.Rsn),
								CorrelationId = eventCorrelationId,
								Data = sameplEvent,
								AggregateRsn = sameplEvent.Rsn,
								EventId = Guid.NewGuid(),
								EventType = typeof(UserCreatedEvent).FullName,
								Timestamp = sameplEvent.TimeStamp.DateTime,
								Version = sameplEvent.Version
							}
						},
						Version = 0,
						State = ServiceResponseStateType.Succeeded
					}
				}
			};
		}

		public static void GenerateAssemblyXmlFileNames()
		{
			if (AssemblyXmlFileNames.Any())
				return;

			string webAssemblyName = Assembly.GetCallingAssembly().FullName;
			webAssemblyName = webAssemblyName.Substring(0, webAssemblyName.IndexOf(","));
			AssemblyXmlFileNames = new List<string> { webAssemblyName };
			try
			{
				string publicAssemblyName = webAssemblyName.Substring(0, webAssemblyName.Length - ".Domain.Host.Web".Length);
				AssemblyXmlFileNames.Add(publicAssemblyName);
			}
			catch (ArgumentOutOfRangeException) { }
			try
			{
				string domainAssemblyName = webAssemblyName.Substring(0, webAssemblyName.Length - ".Host.Web".Length);
				AssemblyXmlFileNames.Add(domainAssemblyName);
			}
			catch (ArgumentOutOfRangeException) { }
		}

		public static void CreateXmlDocumentation()
		{
			GenerateAssemblyXmlFileNames();
			var assemblyXmlFileNames = new List<string>(AssemblyXmlFileNames) {"Cqrs", "Cqrs.WebApi"};
			var finalDocumentation = new XmlDocument();
			for (int i = 0; i < assemblyXmlFileNames.Count; i++)
			{
				string assemblyXmlFileName = assemblyXmlFileNames[i];

				XmlDocument documentation = new XmlDocument();
				if (i == 0)
				{
					finalDocumentation.Load(HttpContext.Current.Server.MapPath(string.Format("~/bin/{0}.XML", assemblyXmlFileName)));
					continue;
				}
				documentation.Load(HttpContext.Current.Server.MapPath(string.Format("~/bin/{0}.XML", assemblyXmlFileName)));

				foreach (XmlNode childNode in documentation.DocumentElement.ChildNodes)
					finalDocumentation.DocumentElement.AppendChild(finalDocumentation.ImportNode(childNode, true));
			}

			finalDocumentation.Save(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml"));
		}
	}
}
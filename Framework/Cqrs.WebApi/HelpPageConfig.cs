using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.Services;

namespace Cqrs.WebApi
{
	public static class HelpPageConfig<TSingleSignOnToken>
		where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		public class UserCreatedEvent : IEvent<TSingleSignOnToken>
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
			public TSingleSignOnToken AuthenticationToken { get; set; }

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

		public static IDictionary<Type, object> GetBasicSampleObjects()
		{
			var eventCorrelationId = Guid.NewGuid();
			var correlationId = Guid.NewGuid();

			var authenticationToken = new TSingleSignOnToken
			{
				Token = Guid.NewGuid().ToString("N"),
				DateIssued = DateTime.Now,
				TimeOfExpiry = DateTime.Now.AddMinutes(20)
			};

			var sameplEvent = new UserCreatedEvent
			{
				CorrelationId = correlationId,
				AuthenticationToken = authenticationToken,
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
					typeof(IServiceRequestWithData<TSingleSignOnToken, Guid>),
					new ServiceRequestWithData<TSingleSignOnToken, Guid>
					{
						AuthenticationToken = new TSingleSignOnToken
						{
							Token = Guid.NewGuid().ToString("N"),
							DateIssued = DateTime.Now,
							TimeOfExpiry = DateTime.Now.AddMinutes(20)
						},
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

		public static void CreateXmlDocumentation()
		{
			XmlDocument cqrsDocumentation = new XmlDocument();
			cqrsDocumentation.Load(HttpContext.Current.Server.MapPath("~/bin/Cqrs.xml"));

			string webAssemblyName = Assembly.GetCallingAssembly().FullName;
			webAssemblyName = webAssemblyName.Substring(0, webAssemblyName.IndexOf(","));
			XmlDocument finalDocumentation = new XmlDocument();
			finalDocumentation.Load(HttpContext.Current.Server.MapPath("~/bin/" + webAssemblyName + ".XML"));
			foreach (XmlNode childNode in cqrsDocumentation.DocumentElement.ChildNodes)
				finalDocumentation.DocumentElement.AppendChild(finalDocumentation.ImportNode(childNode, true));

			string publicAssemblyName = webAssemblyName.Substring(0, webAssemblyName.Length - ".Domain.Host.Web".Length);
			XmlDocument publicDocumentation = new XmlDocument();
			publicDocumentation.Load(HttpContext.Current.Server.MapPath("~/bin/" + publicAssemblyName + ".XML"));
			foreach (XmlNode childNode in publicDocumentation.DocumentElement.ChildNodes)
				finalDocumentation.DocumentElement.AppendChild(finalDocumentation.ImportNode(childNode, true));

			string domainAssemblyName = webAssemblyName.Substring(0, webAssemblyName.Length - ".Host.Web".Length);
			XmlDocument domainDocumentation = new XmlDocument();
			domainDocumentation.Load(HttpContext.Current.Server.MapPath("~/bin/" + domainAssemblyName + ".XML"));
			foreach (XmlNode childNode in domainDocumentation.DocumentElement.ChildNodes)
				finalDocumentation.DocumentElement.AppendChild(finalDocumentation.ImportNode(childNode, true));

			finalDocumentation.Save(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml"));
		}
	}
}
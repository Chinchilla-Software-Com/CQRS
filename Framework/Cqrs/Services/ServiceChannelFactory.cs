#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Cqrs.Services
{
	/// <summary>
	/// A factory that creates channels of different types that are used by clients to send messages to variously configured service endpoints.
	/// </summary>
	/// <typeparam name="TService">The <see cref="Type"/> of service this <see cref="ChannelFactory"/> is for.</typeparam>
	public class ServiceChannelFactory<TService> : ChannelFactory<TService>
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="ServiceChannelFactory{TService}"/> class with a specified endpoint configuration name.
		/// </summary>
		/// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
		public ServiceChannelFactory(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
			RegisterDataContracts();
			AttachDataContractResolver(Endpoint);
		}

		/// <summary>
		/// Uses <see cref="WcfDataContractResolverConfiguration.GetDataContracts{TService}"/>
		/// to find <see cref="DataContractResolver">resolvers</see> to automatically attach to each
		/// <see cref="OperationDescription"/> in <see cref="ContractDescription.Operations"/> of <see cref="ServiceEndpoint.Contract"/> of the provided <paramref name="endpoint"/>.
		/// </summary>
		protected virtual void AttachDataContractResolver(ServiceEndpoint endpoint)
		{
			ContractDescription contractDescription = endpoint.Contract;

			foreach (OperationDescription operationDescription in contractDescription.Operations)
			{
				Type dataContractType = WcfDataContractResolverConfiguration.Current.GetDataContracts<TService>(operationDescription.Name);
				if (dataContractType == null)
					continue;
				DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
				if (serializerBehavior == null)
					operationDescription.Behaviors.Add(serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription));
				#if NET40
				serializerBehavior.DataContractResolver = (DataContractResolver)Activator.CreateInstance(AppDomain.CurrentDomain, dataContractType.Assembly.FullName, dataContractType.FullName).Unwrap();
				#endif
				#if NETCOREAPP3_0
				serializerBehavior.DataContractResolver = (DataContractResolver)Activator.CreateInstance(dataContractType.Assembly.FullName, dataContractType.FullName).Unwrap();
				#endif
			}
		}

		/// <summary>
		/// Register any additional <see cref="DataContractResolver">resolvers</see>.
		/// </summary>
		protected virtual void RegisterDataContracts() { }
	}
}
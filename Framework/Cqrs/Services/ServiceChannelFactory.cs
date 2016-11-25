using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Cqrs.Services
{
	public class ServiceChannelFactory<TService> : ChannelFactory<TService>
	{
		/// <summary>
		/// Instanciates a new instance of the <see cref="ServiceChannelFactory{TService}"/> class with a specified endpoint configuration name.
		/// </summary>
		/// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
		public ServiceChannelFactory(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
			RegisterDataContracts();
			AttachDataContractResolver(Endpoint);
		}

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
				serializerBehavior.DataContractResolver = (DataContractResolver)Activator.CreateInstance(AppDomain.CurrentDomain, dataContractType.Assembly.FullName, dataContractType.FullName).Unwrap();
			}
		}

		protected virtual void RegisterDataContracts() { }
	}
}
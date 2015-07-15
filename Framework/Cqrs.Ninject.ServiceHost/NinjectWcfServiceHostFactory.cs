using System;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Cqrs.Services;
using Ninject.Extensions.Wcf;

namespace Cqrs.Ninject.ServiceHost
{
	public class NinjectWcfServiceHostFactory<TServiceType> : NinjectServiceHostFactory
	{
		protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			System.ServiceModel.ServiceHost host = base.CreateServiceHost(serviceType, baseAddresses);

			foreach (ServiceEndpoint serviceEndpoint in host.Description.Endpoints)
			{
				foreach (OperationDescription operationDescription in serviceEndpoint.Contract.Operations)
				{
					Type dataContractType = WcfDataContractResolverConfiguration.Current.GetDataContracts<TServiceType>(operationDescription.Name);
					if (dataContractType == null)
						continue;
					DataContractSerializerOperationBehavior serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (serializerBehavior == null)
						operationDescription.Behaviors.Add(serializerBehavior = new DataContractSerializerOperationBehavior(operationDescription));
					serializerBehavior.DataContractResolver = (DataContractResolver)Activator.CreateInstance(AppDomain.CurrentDomain, dataContractType.Assembly.FullName, dataContractType.FullName).Unwrap();
				}
			}

			return host;
		}
	}
}
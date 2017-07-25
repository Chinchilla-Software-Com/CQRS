using System;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using Cqrs.Services;
using Ninject.Extensions.Wcf;

namespace Cqrs.Ninject.ServiceHost
{
	/// <summary>
	/// A <see cref="NinjectServiceHostFactory"/> suitable for use with WCF.
	/// </summary>
	/// <typeparam name="TServiceType">The <see cref="Type"/> of the WCF service contract.</typeparam>
	public class NinjectWcfServiceHostFactory<TServiceType> : NinjectServiceHostFactory
	{
		/// <summary>
		/// Creates a <see cref="System.ServiceModel.ServiceHost"/> for a specified type of service with a specific base address.
		/// </summary>
		/// <param name="serviceType">Specifies the <see cref="Type"/> of service to host.</param>
		/// <param name="baseAddresses">The <see cref="System.Array"/> of type <see cref="System.Uri"/> that contains the base addresses for the service hosted.</param>
		/// <returns>A <see cref="System.ServiceModel.ServiceHost"/> for the type of service specified with a specific base address.</returns>
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
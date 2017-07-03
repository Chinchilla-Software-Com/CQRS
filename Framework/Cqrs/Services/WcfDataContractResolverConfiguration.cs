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

namespace Cqrs.Services
{
	public class WcfDataContractResolverConfiguration
	{
		public static WcfDataContractResolverConfiguration Current { get; protected set; }

		protected IDictionary<Type, IDictionary<string, Type>> DataContracts { get; private set; }

		public WcfDataContractResolverConfiguration()
		{
			DataContracts = new Dictionary<Type, IDictionary<string, Type>>();
		}

		static WcfDataContractResolverConfiguration()
		{
			Current = new WcfDataContractResolverConfiguration();
		}

		public virtual void RegisterDataContract<TService, TDataContract>(string operationName, RegistrationHandling registraionHandling = RegistrationHandling.Replace)
			where TDataContract : new ()
		{
			Type serviceType = typeof (TService);
			IDictionary<string, Type> dataContracts;
			if (!DataContracts.TryGetValue(serviceType, out dataContracts))
			{
				lock (DataContracts)
				{
					if (!DataContracts.TryGetValue(serviceType, out dataContracts))
					{
						dataContracts = new Dictionary<string, Type>();
						DataContracts.Add(serviceType, dataContracts);
					}
				}
			}
			if (dataContracts.ContainsKey(operationName))
			{
				switch (registraionHandling)
				{
					case RegistrationHandling.ThrowExceptionOnDuplicate:
						dataContracts.Add(operationName, typeof(TDataContract));
						break;
					case RegistrationHandling.Replace:
						dataContracts[operationName] = typeof(TDataContract);
						break;
					case RegistrationHandling.Nothing:
						return;
				}
			}
			else
				dataContracts.Add(operationName, typeof(TDataContract));
		}

		public virtual Type GetDataContracts<TService>(string operationName)
		{
			Type serviceType = typeof (TService);
			IDictionary<string, Type> dataContracts;
			if (!DataContracts.TryGetValue(serviceType, out dataContracts))
			{
				lock (DataContracts)
				{
					if (!DataContracts.TryGetValue(serviceType, out dataContracts))
					{
						dataContracts = new Dictionary<string, Type>();
						DataContracts.Add(serviceType, dataContracts);
					}
				}
			}

			Type dataContractType;
			if (dataContracts.TryGetValue(operationName, out dataContractType))
				return dataContractType;
			if (operationName == "GetEventData")
				return dataContracts.Values.FirstOrDefault();
			return null;
		}

		public enum RegistrationHandling
		{
			Replace = 0,

			ThrowExceptionOnDuplicate = 1,

			Nothing = 2
		}
	}
}
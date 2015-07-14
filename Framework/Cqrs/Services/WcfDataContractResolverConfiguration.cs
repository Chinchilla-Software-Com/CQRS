#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

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

		public void RegisterDataContract<TService, TDataContract>(string operationName)
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
			dataContracts.Add(operationName, typeof(TDataContract));
		}

		public Type GetDataContracts<TService>(string operationName)
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
			return null;
		}
	}
}
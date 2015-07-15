#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
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

		public virtual void RegisterDataContract<TService, TDataContract>(string operationName, RegistraionHandling registraionHandling = RegistraionHandling.Replace)
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
					case RegistraionHandling.ThrowExceptionOnDuplicate:
						dataContracts.Add(operationName, typeof(TDataContract));
						break;
					case RegistraionHandling.Replace:
						dataContracts[operationName] = typeof(TDataContract);
						break;
					case RegistraionHandling.Nothing:
						return;
				}
			}
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
			return null;
		}

	    public enum RegistraionHandling
	    {
	        Replace = 0,

            ThrowExceptionOnDuplicate = 1,

            Nothing = 2
	    }
	}
}
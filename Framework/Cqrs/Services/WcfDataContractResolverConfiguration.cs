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
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	/// <summary>
	/// Configuration information for setting up WCF <see cref="DataContractResolver">resolvers</see>.
	/// </summary>
	public class WcfDataContractResolverConfiguration
	{
		/// <summary>
		/// The current instance of <see cref="WcfDataContractResolverConfiguration"/> to use.
		/// </summary>
		public static WcfDataContractResolverConfiguration Current { get; protected set; }

		/// <summary>
		/// Gets or set the data contracts for the system to use.
		/// </summary>
		protected IDictionary<Type, IDictionary<string, Type>> DataContracts { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="WcfDataContractResolverConfiguration"/>
		/// </summary>
		public WcfDataContractResolverConfiguration()
		{
			DataContracts = new Dictionary<Type, IDictionary<string, Type>>();
		}

		static WcfDataContractResolverConfiguration()
		{
			Current = new WcfDataContractResolverConfiguration();
		}

		/// <summary>
		/// Register the <typeparamref name="TDataContract"/> to use for the operation named <paramref name="operationName"/> for the <typeparamref name="TService"/>.
		/// </summary>
		/// <typeparam name="TService">The <see cref="Type"/> of service.</typeparam>
		/// <typeparam name="TDataContract">The <see cref="Type"/> of the resolver.</typeparam>
		/// <param name="operationName">The name of the operation.</param>
		/// <param name="registrationHandling">Defaults to <see cref="RegistrationHandling.Replace"/></param>
		public virtual void RegisterDataContract<TService, TDataContract>(string operationName, RegistrationHandling registrationHandling = RegistrationHandling.Replace)
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
				switch (registrationHandling)
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

		/// <summary>
		/// Gets the <see cref="Type"/> of <see cref="DataContractResolver"/> for the operation named <paramref name="operationName"/>
		/// of the <typeparamref name="TService"/>
		/// </summary>
		/// <typeparam name="TService">The <see cref="Type"/> of service.</typeparam>
		/// <param name="operationName">The name of the operation.</param>
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

		/// <summary>
		/// The type of registration action to take
		/// </summary>
		public enum RegistrationHandling
		{
			/// <summary>
			/// Add any new, and replace any existing.
			/// </summary>
			Replace = 0,

			/// <summary>
			/// Throw an <see cref="Exception"/> if one already exists.
			/// </summary>
			ThrowExceptionOnDuplicate = 1,

			/// <summary>
			/// Keep the existing one and don't do anything.
			/// </summary>
			Nothing = 2
		}
	}
}
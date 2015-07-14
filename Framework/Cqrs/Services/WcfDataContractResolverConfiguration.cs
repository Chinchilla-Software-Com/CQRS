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

		protected IDictionary<Type, IList<Tuple<string, string>>> DataContracts { get; private set; }

		public WcfDataContractResolverConfiguration()
		{
			DataContracts = new Dictionary<Type, IList<Tuple<string, string>>>();
		}

		static WcfDataContractResolverConfiguration()
		{
			Current = new WcfDataContractResolverConfiguration();
		}

		public void RegisterDataContract<TService, TDataContract>()
			where TDataContract : new ()
		{
			Type serviceType = typeof (TService);
			IList<Tuple<string, string>> dataContracts;
			if (!DataContracts.TryGetValue(serviceType, out dataContracts))
			{
				dataContracts = new List<Tuple<string, string>>();
				DataContracts.Add(serviceType, dataContracts);
			}
			dataContracts.Add(new Tuple<string, string>(typeof(TDataContract).Assembly.FullName, typeof(TDataContract).FullName));
		}
	}
}
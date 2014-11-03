using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Configuration;
using StructureMap;

namespace CQRSWeb
{
	public class SmDependencyResolver : IDependencyResolver
	{
		private readonly IContainer _container;

		public SmDependencyResolver(IContainer container)
		{
			_container = container;
		}

		public T Resolve<T>()
		{
			return (T) Resolve(typeof (T));
		}

		public object Resolve(Type serviceType)
		{
			if (serviceType == null) return null;
			try
			{
				return serviceType.IsAbstract || serviceType.IsInterface
					? _container.TryGetInstance(serviceType)
					: _container.GetInstance(serviceType);
			}
			catch
			{

				return null;
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _container.GetAllInstances<object>().Where(s => s.GetType() == serviceType);
		}
	}
}
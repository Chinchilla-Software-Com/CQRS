using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Configuration;
using StructureMap;

namespace CQRSWeb
{
	public class SmDependencyResolver : IDependencyResolver,
		System.Web.Mvc.IDependencyResolver
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

		public object Resolve(Type type)
		{
			return Resolve(type, true);
		}

		T IDependencyResolver.Resolve<T>()
		{
			return (T) Resolve(typeof (T), false);
		}

		public object Resolve(Type serviceType, bool returnNullOnExpcetion = true)
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
				if (!returnNullOnExpcetion)
					throw;
				return null;
			}
		}

		public object GetService(Type serviceType)
		{
			return Resolve(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return _container.GetAllInstances<object>().Where(s => s.GetType() == serviceType);
		}
	}
}
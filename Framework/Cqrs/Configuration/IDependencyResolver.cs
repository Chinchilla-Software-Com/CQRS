using System;

namespace Cqrs.Configuration
{
	public interface IDependencyResolver 
	{
		T Resolve<T>();

		object Resolve(Type type);
	}
}
using System;

namespace Cqrs.Configuration
{
	public interface IServiceLocator 
	{
		T GetService<T>();
		object GetService(Type type);
	}
}
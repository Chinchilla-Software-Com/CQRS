using System;

namespace Cqrs.Config
{
	public interface IServiceLocator 
	{
		T GetService<T>();
		object GetService(Type type);
	}
}
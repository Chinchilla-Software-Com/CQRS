#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Akka.Configuration
{
	public interface IHandlerResolver
	{
		object Resolve(Type serviceType, object rsn);
	}
}
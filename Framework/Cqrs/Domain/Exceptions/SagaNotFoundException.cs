#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain.Exceptions
{
	[Serializable]
	public class SagaNotFoundException<TSaga, TAuthenticationToken> : Exception
		where TSaga : ISaga<TAuthenticationToken>
	{
		public SagaNotFoundException(Guid id)
			: base(string.Format("Saga '{0}' of type '{1}' was not found", id, typeof(TSaga).FullName))
		{
		}
	}
}
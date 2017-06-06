#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Serialization;

namespace Cqrs.Domain
{
	internal class SagaDescriptor<TSaga, TAuthenticationToken> : ISagaDescriptor<TAuthenticationToken>
		where TSaga : ISaga<TAuthenticationToken>
	{
		ISaga<TAuthenticationToken> ISagaDescriptor<TAuthenticationToken>.Saga
		{
			get { return Saga; }
		}

		[DataMember]
		public TSaga Saga { get; set; }

		[DataMember]
		public int Version { get; set; }
	}
}
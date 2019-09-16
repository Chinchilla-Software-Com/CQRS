#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Serialization;

namespace Cqrs.Domain
{
	internal interface IAggregateDescriptor
	{
		[DataMember]
		int Version { get; set; }

		[DataMember]
		bool UseSnapshots { get; set; }
	}

	internal interface IAggregateDescriptor<TAuthenticationToken> : IAggregateDescriptor
	{
		[DataMember]
		IAggregateRoot<TAuthenticationToken> Aggregate { get; }
	}
}
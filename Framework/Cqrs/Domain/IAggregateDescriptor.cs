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
	internal interface IAggregateDescriptor
	{
		[DataMember]
		int Version { get; set; }
	}

	internal interface IAggregateDescriptor<TAuthenticationToken> : IAggregateDescriptor
	{
		[DataMember]
		IAggregateRoot<TAuthenticationToken> Aggregate { get; }
	}
}
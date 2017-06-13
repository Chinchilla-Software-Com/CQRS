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
	internal interface ISagaDescriptor
	{
		[DataMember]
		int Version { get; set; }
	}

	internal interface ISagaDescriptor<TAuthenticationToken> : ISagaDescriptor
	{
		[DataMember]
		ISaga<TAuthenticationToken> Saga { get; }
	}
}
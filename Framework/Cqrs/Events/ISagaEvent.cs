#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Serialization;

namespace Cqrs.Events
{
	public interface ISagaEvent<TAuthenticationToken>
		: IEvent<TAuthenticationToken>
	{
		[DataMember]
		IEvent<TAuthenticationToken> Event { get; set; }
	}
}
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Commands
{
	/// <summary>
	/// People request changes to the domain by sending <see cref="ICommand{TAuthenticationToken}"/>s. They are named with a verb in the imperative mood plus and may include the <see cref="IAggregateRoot{TAuthenticationToken}"/> type, for example ConfirmOrder. Unlike an <see cref="IEvent{TAuthenticationToken}"/>, a <see cref="ICommand{TAuthenticationToken}"/> is not a statement of fact; it's only a request, and thus may be refused. (A typical way to convey refusal is to raise a specifically typed <see cref="IEvent{TAuthenticationToken}"/> stating the <see cref="ICommand{TAuthenticationToken}"/> didn't happen for a specific reason).
	/// </summary>
	/// <example>
	/// public class ConfirmOrder
	/// {
	/// 	public Guid OrderRsn;
	/// }
	/// </example>
	/// <remarks>
	/// What does a <see cref="ICommand{TAuthenticationToken}"/> or an <see cref="IEvent{TAuthenticationToken}"/> look like?
	/// 
	/// An <see cref="ICommand{TAuthenticationToken}"/> or <see cref="IEvent{TAuthenticationToken}"/> is simply a data structure that contain data for reading, and no behavior. We call such structures "Data Transfer Objects" (DTOs). The name indicates the purpose. In many languages they are represented as classes, but they are not true classes in the real OO sense.
	/// 
	/// 
	/// What is the difference between a <see cref="ICommand{TAuthenticationToken}"/> and an <see cref="IEvent{TAuthenticationToken}"/>?
	/// 
	/// Their intent.
	/// 
	/// 
	/// What is immutability? Why is a <see cref="ICommand{TAuthenticationToken}"/> or <see cref="IEvent{TAuthenticationToken}"/> immutable?
	/// 
	/// For the purpose of this question, immutability is not having any setters, or other methods which change internal state. The <see cref="string"/> type in is a familiar example; you never actually change an existing <see cref="string"/> value, you just create new <see cref="string"/> values based on old ones.
	/// 
	/// An <see cref="ICommand{TAuthenticationToken}"/> is immutable because their expected usage is to be sent directly to the domain model side for processing. They do not need to change during their projected lifetime in traveling from client to server.
	/// Sometimes however business logic dictates that a decision may be made to construct a <see cref="ICommand{TAuthenticationToken}"/> and local variables should be used.
	/// 
	/// An <see cref="IEvent{TAuthenticationToken}"/> is immutable because they represent domain actions that took place in the past. Unless you're Marty McFly, you can't change the past, and sometimes not even then.
	/// 
	/// 
	/// What is command upgrading?
	/// 
	/// Upgrading an <see cref="ICommand{TAuthenticationToken}"/> becomes necessary when new requirements cause an existing <see cref="ICommand{TAuthenticationToken}"/> not to be sufficient. Maybe a new field needs to be added, for example, or maybe an existing field should really have been split into several different ones.
	/// 
	/// 
	/// How do I upgrade my <see cref="ICommand{TAuthenticationToken}"/>s?
	/// 
	/// How you do the upgrade depends how much control you have over your clients. If you can deploy your client updates and server updates together, just change things in both and deploy the updates. Job done. If not, it's usually best to have the updated <see cref="ICommand{TAuthenticationToken}"/> be a new type and have the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> accept both for a while.
	/// 
	/// 
	/// Could you give an example of names of some versioned <see cref="ICommand{TAuthenticationToken}"/>?
	/// 
	/// Sure.
	/// 
	/// UploadFile
	/// UploadFile_v2
	/// UploadFile_v3
	/// 
	/// It's just a convention, but a sane one.
	/// ********************************************
	/// Also see http://cqrs.nu/Faq/commands-and-events.
	/// </remarks>
	public interface ICommand<TAuthenticationToken> : IMessageWithAuthenticationToken<TAuthenticationToken>
	{
		Guid Id { get; set; }

		int ExpectedVersion { get; set; }
	}
}
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
	/// An <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> receives an <see cref="ICommand{TAuthenticationToken}"/> and brokers a result from the appropriate <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// "A result" is either a successful application of the command, or an exception.
	/// This is the common sequence of steps an <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> might follow:
	/// 
	/// Validate the <see cref="ICommand{TAuthenticationToken}"/> on its own merits.
	/// Ask an <see cref="IAggregateRoot{TAuthenticationToken}"/> to handle the <see cref="ICommand{TAuthenticationToken}"/>.
	/// If validation is successful, 0..n <see cref="IEvent{TAuthenticationToken}"/> artefacts (1 is common) are queued for publishing.
	/// Attempt to persist the new <see cref="IEvent{TAuthenticationToken}"/> artefacts. If there's a concurrency conflict during this step, either give up, or retry things.
	/// Dispatch the queued <see cref="IEvent{TAuthenticationToken}"/> artefacts.
	/// </summary>
	/// <remarks>
	/// Should a <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> affect one or several <see cref="IAggregateRoot{TAuthenticationToken}"/>s?
	/// 
	/// Only one.
	/// 
	/// 
	/// Do I put logic in <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// Yes. Exactly what logic depends on your factoring.
	/// The logic for validating the <see cref="ICommand{TAuthenticationToken}"/> on its own merits always gets executed in the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>, although we recommend refactoring these into an <see cref="ICommandValidator{TAuthenticationToken,TCommand}"/>.
	/// Provided validation is successful we recommend a more functional factoring, where the <see cref="IAggregateRoot{TAuthenticationToken}"/> exists independently of the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> and the next step would be to load the <see cref="IAggregateRoot{TAuthenticationToken}"/> from the <see cref="IUnitOfWork{TAuthenticationToken}"/> and request the <see cref="IAggregateRoot{TAuthenticationToken}"/> handle the <see cref="ICommand{TAuthenticationToken}"/> itself.
	/// The <see cref="IUnitOfWork{TAuthenticationToken}"/> should then have uncommitted <see cref="IEvent{TAuthenticationToken}"/> artefacts as a results of asking the <see cref="IAggregateRoot{TAuthenticationToken}"/> to handle the <see cref="ICommand{TAuthenticationToken}"/>.
	/// Finally the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> should instruct the <see cref="IUnitOfWork{TAuthenticationToken}"/> to IUnitOfWork.Commit all uncommited <see cref="IEvent{TAuthenticationToken}"/> artefacts.
	/// 
	/// However you have it, the logic boils down to validation and some sequence of steps that lead to the <see cref="ICommand{TAuthenticationToken}"/> becoming an <see cref="Exception"/> or <see cref="IEvent{TAuthenticationToken}"/>(s). If you're tempted to go beyond this, see the rest of the remarks.
	/// 
	/// 
	/// Can I call a read side (such as a read store, data store or <see cref="IAggregateRepository{TAuthenticationToken}"/>) from my <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// No.
	/// 
	/// 
	/// Can I do logging, security, or auditing in my <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// Yes. The decorator pattern comes in handy here to separate those concerns neatly.
	/// 
	/// 
	/// How are conflicts between concurrent <see cref="ICommand{TAuthenticationToken}"/>s handled in the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// The place where the new <see cref="IEvent{TAuthenticationToken}"/> artefacts for the <see cref="IAggregateRoot{TAuthenticationToken}"/> are persisted is the only place in the system where we need to worry about concurrency conflicts. The <see cref="IEventStore{TAuthenticationToken}"/> knows the sequence number of the latest <see cref="IEvent{TAuthenticationToken}"/> applied on that <see cref="IAggregateRoot{TAuthenticationToken}"/>, and the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> knows the sequence number of the last <see cref="IEvent{TAuthenticationToken}"/> it read. If these numbers do not agree, it means some other thread or process got there first. The <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> can then load up the events again and make a new attempt.
	/// 
	/// 
	/// Should I do things that have side-effects in the outside world (such as sending email) directly in a <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// No, since a concurrency conflict will mean the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> logic will be run again. Do such things in an Apply <see cref="IEvent{TAuthenticationToken}"/> method in an <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// ********************************************
	/// Also see http://cqrs.nu/Faq/command-handlers.
	/// </remarks>
	public interface ICommandHandler<TAuthenticationToken, in TCommand>
		: IMessageHandler<TCommand>
		, ICommandHandle
		where TCommand : ICommand<TAuthenticationToken>
	{
	}

	/// <summary>
	/// An <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> receives an <see cref="ICommand{TAuthenticationToken}"/> and brokers a result from the appropriate <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// "A result" is either a successful application of the command, or an exception.
	/// This is the common sequence of steps an <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> might follow:
	/// 
	/// Validate the <see cref="ICommand{TAuthenticationToken}"/> on its own merits.
	/// Ask an <see cref="IAggregateRoot{TAuthenticationToken}"/> to handle the <see cref="ICommand{TAuthenticationToken}"/>.
	/// If validation is successful, 0..n <see cref="IEvent{TAuthenticationToken}"/> artefacts (1 is common) are queued for publishing.
	/// Attempt to persist the new <see cref="IEvent{TAuthenticationToken}"/> artefacts. If there's a concurrency conflict during this step, either give up, or retry things.
	/// Dispatch the queued <see cref="IEvent{TAuthenticationToken}"/> artefacts.
	/// </summary>
	/// <remarks>
	/// Should a <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> affect one or several <see cref="IAggregateRoot{TAuthenticationToken}"/>s?
	/// 
	/// Only one.
	/// 
	/// 
	/// Do I put logic in <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// Yes. Exactly what logic depends on your factoring.
	/// The logic for validating the <see cref="ICommand{TAuthenticationToken}"/> on its own merits always gets executed in the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>, although we recommend refactoring these into an <see cref="ICommandValidator{TAuthenticationToken,TCommand}"/>.
	/// Provided validation is successful we recommend a more functional factoring, where the <see cref="IAggregateRoot{TAuthenticationToken}"/> exists independently of the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> and the next step would be to load the <see cref="IAggregateRoot{TAuthenticationToken}"/> from the <see cref="IUnitOfWork{TAuthenticationToken}"/> and request the <see cref="IAggregateRoot{TAuthenticationToken}"/> handle the <see cref="ICommand{TAuthenticationToken}"/> itself.
	/// The <see cref="IUnitOfWork{TAuthenticationToken}"/> should then have uncommitted <see cref="IEvent{TAuthenticationToken}"/> artefacts as a results of asking the <see cref="IAggregateRoot{TAuthenticationToken}"/> to handle the <see cref="ICommand{TAuthenticationToken}"/>.
	/// Finally the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> should instruct the <see cref="IUnitOfWork{TAuthenticationToken}"/> to IUnitOfWork.Commit all uncommited <see cref="IEvent{TAuthenticationToken}"/> artefacts.
	/// 
	/// However you have it, the logic boils down to validation and some sequence of steps that lead to the <see cref="ICommand{TAuthenticationToken}"/> becoming an <see cref="Exception"/> or <see cref="IEvent{TAuthenticationToken}"/>(s). If you're tempted to go beyond this, see the rest of the remarks.
	/// 
	/// 
	/// Can I call a read side (such as a read store, data store or <see cref="IAggregateRepository{TAuthenticationToken}"/>) from my <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// No.
	/// 
	/// 
	/// Can I do logging, security, or auditing in my <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// Yes. The decorator pattern comes in handy here to separate those concerns neatly.
	/// 
	/// 
	/// How are conflicts between concurrent <see cref="ICommand{TAuthenticationToken}"/>s handled in the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// The place where the new <see cref="IEvent{TAuthenticationToken}"/> artefacts for the <see cref="IAggregateRoot{TAuthenticationToken}"/> are persisted is the only place in the system where we need to worry about concurrency conflicts. The <see cref="IEventStore{TAuthenticationToken}"/> knows the sequence number of the latest <see cref="IEvent{TAuthenticationToken}"/> applied on that <see cref="IAggregateRoot{TAuthenticationToken}"/>, and the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> knows the sequence number of the last <see cref="IEvent{TAuthenticationToken}"/> it read. If these numbers do not agree, it means some other thread or process got there first. The <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> can then load up the events again and make a new attempt.
	/// 
	/// 
	/// Should I do things that have side-effects in the outside world (such as sending email) directly in a <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>?
	/// 
	/// No, since a concurrency conflict will mean the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> logic will be run again. Do such things in an Apply <see cref="IEvent{TAuthenticationToken}"/> method in an <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// ********************************************
	/// Also see http://cqrs.nu/Faq/command-handlers.
	/// </remarks>
	public interface ICommandHandle : IHandler
	{
	}
}
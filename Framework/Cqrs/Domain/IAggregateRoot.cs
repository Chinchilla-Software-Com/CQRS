#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// An <see cref="IAggregateRoot{TAuthenticationToken}"/> is a larger unit of encapsulation than just a class. Every transaction is scoped to a single aggregate. The lifetimes of the components of an <see cref="IAggregateRoot{TAuthenticationToken}"/> are bounded by the lifetime of the entire <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// 
	/// <para />Concretely, an <see cref="IAggregateRoot{TAuthenticationToken}"/> will handle <see cref="ICommand{TAuthenticationToken}"/>s, apply <see cref="IEvent{TAuthenticationToken}"/>s, and have a state model encapsulated within it that allows it to implement the required command validation, thus upholding the invariants (business rules) of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// </summary>
	/// <remarks>
	/// I know <see cref="IAggregateRoot{TAuthenticationToken}"/> are transaction boundaries, but I really need to transactionally update two <see cref="IAggregateRoot{TAuthenticationToken}"/> in the same transaction. What should I do?
	/// 
	/// <para />You should re-think the following:
	/// <para />* Your <see cref="IAggregateRoot{TAuthenticationToken}"/> boundaries.
	/// <para />* The responsibilities of each <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// <para />* What you can get away with doing in a read side or in a saga.
	/// <para />* The actual non-functional requirements of your domain.
	/// <para />
	/// <para />If you write a solution where two or more <see cref="IAggregateRoot{TAuthenticationToken}"/> are transactionally coupled, you have not understood <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// <para />
	/// <para />
	/// <para />Why is the use of <see cref="Guid"/> as identifiers a good practice?
	/// <para />
	/// <para />Because they are (reasonably) globally unique, and can be generated either by the server or by the client.
	/// <para />
	/// <para />
	/// <para />What is an Rsn and what is an Id?
	/// <para />
	/// <para />Because few systems are truely green field and there is usually some existing system to operate with our framework identifies
	/// <para />Id properties as <see cref="int"/> typed properties from an external system
	/// <para />and Rsn properties as <see cref="Guid"/> typed properties for internal use.
	/// <para />
	/// <para />An example might be
	/// <para />{
	/// <para />	Guid Rsn
	/// <para />	string Name
	/// <para />	Guid CategoryRsn
	/// <para />	int CategoryId
	/// <para />}
	/// <para />
	/// <para />Here the category can be referenced within the CQRS framework by it's Rsn <see cref="Guid"/> typed identifier, but still has a reference to the external systems <see cref="int"/> typed identifier value.
	/// <para />
	/// <para />
	/// <para />How can I get the Rsn for newly created <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />It's an important insight that the client can generate its own Rsns.
	/// <para />
	/// <para />If the client generates a <see cref="Guid"/> and places it in the create-the-aggregate <see cref="ICommand{TAuthenticationToken}"/>, this is a non-issue. Otherwise, you have to listen to the the appropriate the-aggregate-was-created <see cref="IEvent{TAuthenticationToken}"/>, where the Rsn will appear be populated.
	/// <para />
	/// <para />
	/// <para />Should I allow references between <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />In the sense of an actual "memory reference", absolutely not.
	/// <para />
	/// <para />On the write side, an actual memory reference from one <see cref="IAggregateRoot{TAuthenticationToken}"/> to another is forbidden and wrong, since <see cref="IAggregateRoot{TAuthenticationToken}"/> by definition are not allowed to reach outside of themselves. (Allowing this would mean an <see cref="IAggregateRoot{TAuthenticationToken}"/> is no longer a transaction boundary, meaning we can no longer sanely reason about its ability to uphold its invariants; it would also preclude sharding of <see cref="IAggregateRoot{TAuthenticationToken}"/>.)
	/// <para />
	/// <para />Referring to another <see cref="IAggregateRoot{TAuthenticationToken}"/> using an identifier is fine. It is useless on the write side (since the identifier must be treated as an opaque value, since <see cref="IAggregateRoot{TAuthenticationToken}"/> can not reach outside of themselves). Read sides may freely use such information, however, to do interesting correlations.
	/// <para />
	/// <para />
	/// <para />How can I validate a <see cref="ICommand{TAuthenticationToken}"/> across a group of <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />This is a common reaction to not being able to query across <see cref="IAggregateRoot{TAuthenticationToken}"/> anymore. There are several answers:
	/// <para />
	/// <para />* Do client-side validation.
	/// <para />* Use a read side.
	/// <para />* Use a saga.
	/// <para />* If those are all completely impractical, then it's time to consider if you got your <see cref="IAggregateRoot{TAuthenticationToken}"/> boundaries correct.
	/// <para />
	/// <para />
	/// <para />How can I guarantee referential integrity across <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />You're still thinking in terms of foreign relations, not <see cref="IAggregateRoot{TAuthenticationToken}"/>. See last question. Also, remember that just because something would be in two tables in a relational design does not in any way suggest it should be two <see cref="IAggregateRoot{TAuthenticationToken}"/>. Designing an <see cref="IAggregateRoot{TAuthenticationToken}"/> is different.
	/// <para />
	/// <para />
	/// <para />How can I make sure a newly created user has a unique user name?
	/// <para />
	/// <para />This is a commonly occurring question since we're explicitly not performing cross-aggregate operations on the write side. We do, however, have a number of options:
	/// <para />
	/// <para />* Create a read-side of already allocated user names. Make the client query the read-side interactively as the user types in a name.
	/// <para />* Create a reactive saga to flag down and inactivate accounts that were nevertheless created with a duplicate user name. (Whether by extreme coincidence or maliciously or because of a faulty client.)
	/// <para />
	/// <para />
	/// <para />How can I verify that a customer identifier really exists when I place an order?
	/// <para />
	/// <para />Assuming customer and order are <see cref="IAggregateRoot{TAuthenticationToken}"/> here, it's clear that the order <see cref="IAggregateRoot{TAuthenticationToken}"/> cannot really validate this, since that would mean reaching out of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// <para />
	/// <para />Checking up on it after the fact, in a saga or just in a read side that records "broken" orders, is one option. After all, the most important thing about an order is actually recording it, and presumably any interesting data about the recipient of the order is being copied into the order <see cref="IAggregateRoot{TAuthenticationToken}"/> (referring to the customer to find the address is bad design; the order was always made to be deliverd to a particular address, whether or not that customer changes their address in the future).
	/// <para />
	/// <para />Being able to use what data was recorded in this broken order means you've got a chance to rescue it and rectify the situation - which makes a good bit more business sense rather than dropping the order on the floor because a foreign key constraint was violated!
	/// <para />
	/// <para />
	/// <para />How can I update a set of <see cref="IAggregateRoot{TAuthenticationToken}"/> with a single <see cref="ICommand{TAuthenticationToken}"/>?
	/// <para />
	/// <para />A single <see cref="ICommand{TAuthenticationToken}"/> can't act on a set of <see cref="IAggregateRoot{TAuthenticationToken}"/>. It just can't.
	/// <para />
	/// <para />First off, ask yourself whether you really need to update several <see cref="IAggregateRoot{TAuthenticationToken}"/> using just one <see cref="ICommand{TAuthenticationToken}"/>. What in the situation makes this a requirement?
	/// <para />
	/// <para />However, here's what you could do. Allow a new kind of "bulk command", conceptually containing the command you want to issue, and a set of <see cref="IAggregateRoot{TAuthenticationToken}"/> (specified either explicitly or implicitly) that you want to issue it on. The write side isn't powerful enough to make the bulk action, but it's able to create a corresponding "bulk event". A saga captures the event, and issues the <see cref="ICommand{TAuthenticationToken}"/> on each of the specified <see cref="IAggregateRoot{TAuthenticationToken}"/>s. The saga can do rollback or send an email, as appropriate, if some of the <see cref="ICommand{TAuthenticationToken}"/> fail.
	/// <para />
	/// <para />There are some advantages to this approach: we store the intent of the bulk action in the event store. The saga automates rollback or equivalent.
	/// <para />
	/// <para />Still, having to resort to this solution is a strong indication that your <see cref="IAggregateRoot{TAuthenticationToken}"/> boundaries are not drawn correctly. You might want to consider changing your <see cref="IAggregateRoot{TAuthenticationToken}"/> boundaries rather than building a saga for this.
	/// <para />
	/// <para />
	/// <para />What is sharding?
	/// <para />
	/// <para />A way to distribute large amounts of <see cref="IAggregateRoot{TAuthenticationToken}"/> on several write-side nodes. We can shard <see cref="IAggregateRoot{TAuthenticationToken}"/> easily because they are completely self-reliant.
	/// <para />
	/// <para />We can shard <see cref="IAggregateRoot{TAuthenticationToken}"/> easily because they don't have any external references.
	/// <para />
	/// <para />
	/// <para />Can an <see cref="IAggregateRoot{TAuthenticationToken}"/> send an <see cref="IEvent{TAuthenticationToken}"/> to another <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />No.
	/// <para />
	/// <para />The factoring of your <see cref="IAggregateRoot{TAuthenticationToken}"/> and <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> will typically already make this idea impossible to express in code. But there's a deeper philosophical reason: go back and re-read the first sentence in the answer to "What is an <see cref="IAggregateRoot{TAuthenticationToken}"/>?". If you manage to circumvent the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> and just push <see cref="IEvent{TAuthenticationToken}"/> into another <see cref="IAggregateRoot{TAuthenticationToken}"/> somehow, you will have taken away that <see cref="IAggregateRoot{TAuthenticationToken}"/>'s chance to participate in validation of changes. That's ultimately why we only allow <see cref="IEvent{TAuthenticationToken}"/> to be created as a result of <see cref="ICommand{TAuthenticationToken}"/>s validated by a <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> on an <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// <para />
	/// <para />
	/// <para />Can I call a read side from my <see cref="IAggregateRoot{TAuthenticationToken}"/>?
	/// <para />
	/// <para />No.
	/// <para />
	/// <para />
	/// <para />How do I send e-mail in a CQRS system?
	/// <para />
	/// <para />In an <see cref="IEventHandler{TAuthenticationToken,TEvent}"/> outside of the <see cref="IAggregateRoot{TAuthenticationToken}"/>. Do not do it in the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>, as if the <see cref="IEvent{TAuthenticationToken}"/> artefacts are not persisted due to losing a race with another <see cref="ICommand{TAuthenticationToken}"/> then the email will have been sent on a false premise.
	/// <para />********************************************
	/// <para />Also see http://cqrs.nu/Faq/aggregates.
	/// </remarks>
	public interface IAggregateRoot<TAuthenticationToken>
	{
		/// <summary>
		/// The identifier of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		Guid Id { get; }

		/// <summary>
		/// The current version of this <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		int Version { get; }

		IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges();

		void MarkChangesAsCommitted();

		void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history);
	}
}
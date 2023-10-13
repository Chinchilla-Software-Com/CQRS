#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Messages;

namespace Cqrs.Azure.ServiceBus.Tests.Integration
{
	/// <summary>
	/// A Test <see cref="IMessageHandler{TMessage}"/> for handling <see cref="TestCommand"/>.
	/// </summary>
	public class TestCommandSuccessHandler
		: IMessageHandler<TestCommand>
	{
		/// <summary>
		/// Instantiate and initialise a new <see cref="TestCommandSuccessHandler"/> specifying the test flag container.
		/// </summary>
		/// <param name="testResponse">The test flag container.</param>
		public TestCommandSuccessHandler(IDictionary<Guid, Tuple<bool, Exception>> testResponse)
		{
			TestResponse = testResponse;
		}

		/// <summary>
		/// A local reference to the test flag container.
		/// </summary>
		protected IDictionary<Guid, Tuple<bool, Exception>> TestResponse { get; private set; }

		#region Implementation of IHandler<in TestCommand>

		/// <summary>
		/// Sets a value into <see cref="TestResponse"/> so it can be reported back to the test class.
		/// </summary>
		public virtual
#if NET472
			void Handle
#else
			async Task HandleAsync
#endif
			(TestCommand message)
		{
			TestResponse[message.Id] = new Tuple<bool, Exception>(true, null);
#if NET472
#else
			await Task.CompletedTask;
#endif
		}

		#endregion
	}
}
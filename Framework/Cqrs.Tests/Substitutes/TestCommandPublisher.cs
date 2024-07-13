using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Commands;

namespace Cqrs.Tests.Substitutes
{
	public class TestCommandPublisher : ICommandPublisher<ISingleSignOnToken>
	{
		public TestCommandPublisher()
		{
			PublishedCommands = new List<ICommand<ISingleSignOnToken>>();
		}

		public void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<ISingleSignOnToken>
		{
			PublishedCommands.Add(command);
		}

		public void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<ISingleSignOnToken>
		{
			PublishedCommands.AddRange(commands.Cast<ICommand<ISingleSignOnToken>>());
		}

		public void Publish<TCommand>(TCommand command, TimeSpan delay)
			where TCommand : ICommand<ISingleSignOnToken>
		{
			PublishedCommands.Add(command);
		}

		public void Publish<TCommand>(IEnumerable<TCommand> commands, TimeSpan delay)
			where TCommand : ICommand<ISingleSignOnToken>
		{
			PublishedCommands.AddRange(commands.Cast<ICommand<ISingleSignOnToken>>());
		}

		public int Published { get { return PublishedCommands.Count; } }

		public List<ICommand<ISingleSignOnToken>> PublishedCommands { get; private set; }
	}
}
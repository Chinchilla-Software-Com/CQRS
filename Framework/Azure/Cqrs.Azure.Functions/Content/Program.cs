using System;
using System.Collections.Generic;
using Cqrs.Authentication;
using Cqrs.Azure.Functions;
using Cqrs.Configuration;

public class Program
	: CqrsFunctionHost<Guid>
{
	public static void Main(string[] args)
	{
		PrepareConfigurationManager();
		new Program().Go();
	}

	protected override void ConfigureDefaultDependencyResolver()
	{
		// todo implement
	}
}
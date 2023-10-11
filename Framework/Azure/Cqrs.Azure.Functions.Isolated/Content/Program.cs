using System;
using System.Collections.Generic;
using Cqrs.Authentication;
using Cqrs.Azure.Functions;
using Cqrs.Configuration;

public class Program
	: CqrsIsolatedFunctionHost<Guid, DefaultAuthenticationTokenHelper, IsolatedFunctionHostModule>
{
	/// <summary>
	/// Entry point for the issolated application.
	/// </summary>
	public static void Main(string[] args)
	{
		PrepareConfigurationManager();
		new Program().Go();
	}
}
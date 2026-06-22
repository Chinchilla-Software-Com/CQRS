using System;
using Cqrs.Authentication;
using Cqrs.Azure.Functions.Isolated;
using Cqrs.Azure.Functions.Isolated.Configuration;

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
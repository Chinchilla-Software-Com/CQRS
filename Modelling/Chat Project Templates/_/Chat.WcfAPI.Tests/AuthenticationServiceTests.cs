using System;
using $ext_safeprojectname$.MicroServices.Authentication.Services;
using $ext_safeprojectname$.MicroServices.Authentication.Services.ServiceChannelFactories;
using Cqrs.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace $safeprojectname$
{
	/// <summary>
	/// A series of tests on <see cref="IAuthenticationService"/> using WCF for communication.
	/// </summary>
	[TestClass]
	public class AuthenticationServiceTests
	{
		/// <summary>
		/// Tests that a call to <see cref="IAuthenticationService.Login"/> with <see cref="AuthenticationService.LoginParameters">credentials</see> returns a valid result.
		/// </summary>
		[TestMethod]
		public void Login_TestCredentials_Succeeded()
		{
			// Arrange
			HttpAuthenticationServiceChannelFactory factory = new HttpAuthenticationServiceChannelFactory();
			IAuthenticationService service = factory.CreateChannel();

			// Act
			IServiceResponseWithResultData<Guid?> actual = service.Login(new ServiceRequestWithData<Guid, AuthenticationService.LoginParameters> { Data = new AuthenticationService.LoginParameters { EmailAddress = "john@domain.com", Password = "john123" } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.Succeeded, actual.State);
			Assert.IsNotNull(actual.ResultData);
			Assert.IsTrue(actual.ResultData != Guid.Empty);
		}

		/// <summary>
		/// Tests that a call to <see cref="IAuthenticationService.Login"/> with no <see cref="AuthenticationService.LoginParameters.EmailAddress"/> returns <see cref="ServiceResponseStateType.FailedValidation"/>.
		/// </summary>
		[TestMethod]
		public void Login_NoEmailAddress_FailedValidation()
		{
			// Arrange
			HttpAuthenticationServiceChannelFactory factory = new HttpAuthenticationServiceChannelFactory();
			IAuthenticationService service = factory.CreateChannel();

			// Act
			IServiceResponseWithResultData<Guid?> actual = service.Login(new ServiceRequestWithData<Guid, AuthenticationService.LoginParameters> { Data = new AuthenticationService.LoginParameters { Password = "john123" } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.FailedValidation, actual.State);
			Assert.IsNull(actual.ResultData);
		}

		/// <summary>
		/// Tests that a call to <see cref="IAuthenticationService.Login"/> with no <see cref="AuthenticationService.LoginParameters.Password"/> returns <see cref="ServiceResponseStateType.FailedValidation"/>.
		/// </summary>
		[TestMethod]
		public void Login_NoPassword_FailedValidation()
		{
			// Arrange
			HttpAuthenticationServiceChannelFactory factory = new HttpAuthenticationServiceChannelFactory();
			IAuthenticationService service = factory.CreateChannel();

			// Act
			IServiceResponseWithResultData<Guid?> actual = service.Login(new ServiceRequestWithData<Guid, AuthenticationService.LoginParameters> { Data = new AuthenticationService.LoginParameters { EmailAddress = "john@domain.com" } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.FailedValidation, actual.State);
			Assert.IsNull(actual.ResultData);
		}

		/// <summary>
		/// Tests that a call to <see cref="IAuthenticationService.Login"/> with invalid <see cref="AuthenticationService.LoginParameters">credentials</see> returns <see cref="ServiceResponseStateType.FailedAuthentication"/>.
		/// </summary>
		[TestMethod]
		public void Login_InvalidCredentials_FailedAuthentication()
		{
			// Arrange
			HttpAuthenticationServiceChannelFactory factory = new HttpAuthenticationServiceChannelFactory();
			IAuthenticationService service = factory.CreateChannel();

			// Act
			IServiceResponseWithResultData<Guid?> actual = service.Login(new ServiceRequestWithData<Guid, AuthenticationService.LoginParameters> { Data = new AuthenticationService.LoginParameters { EmailAddress = "john@domain.com", Password = "Password"} });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.FailedAuthentication, actual.State);
			Assert.IsNull(actual.ResultData);
		}
	}
}
'use strict';

window.chatApp.services.authService = function ($http, $rootScope)
{
	var ready = false;

	var factory = {
			loginPath: '/login',
			user: {
				isAuthenticated: function ()
				{
					var authCookie = Cookies.get("X-Token");
					var result = (authCookie != null && authCookie != "");
					if (!ready && result)
					{
						ready = true;
						// Start the connection.
						$.connection.hub.start({ withCredentials: false }).done(function () {
						});
					}
					else if (!result)
					{
						ready = false;
						// Stop the connection.
						$.connection.hub.stop();
					}
					return result;
				},
				roles: null
			}
		};

	factory.login = function (email, password)
	{
		return window.api.Authentication
			.Login({ EmailAddress: email, Password: password })
		.done
		(
			function (result, textStatus, jqXHR)
			{
				Cookies.set("X-Token", result.ResultData.xToken);
				return factory.user.isAuthenticated();
			}
		)
		.fail
		(
			function (jqXHR, textStatus, errorThrown)
			{
				console.error(textStatus, errorThrown);
			}
		);
	};

	factory.logout = function ()
	{
		return window.api.Authentication
			.Logout()
			.done
			(
				function (result, textStatus, jqXHR)
				{
					Cookies.set("X-Token", "");
					return factory.user.isAuthenticated();
				}
			)
			.fail
			(
				function (jqXHR, textStatus, errorThrown)
				{
					console.error(textStatus, errorThrown);
				}
			);
	};

	factory.redirectToLogin = function ()
	{
		$rootScope.$broadcast('redirectToLogin', null);
	};

	factory.processApiResponse = function (jqXHR, textStatus, errorThrown)
	{
		if (jqXHR.status === 401 || jqXHR.status === 403)
			factory.redirectToLogin();
	};

	return factory;
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.factory('authService', window.chatApp.services.authService);
	}
);
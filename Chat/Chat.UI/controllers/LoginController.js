'use strict';

window.chatApp.controllers.LoginController = function ($location, $routeParams, authService)
{
	var vm = this,
		path = '/';

	vm.email = null;
	vm.password = null;
	vm.errorMessage = null;

	vm.login = function ()
	{
		authService
			.login(vm.email, vm.password)
			.always
			(
				function (jqXHR, statusText, responseType)
				{
					if (statusText !== "success")
					{
						//$routeParams.redirect will have the route
						//they were trying to go to initially
						if (responseType === "Forbidden")
						{
							vm.errorMessage = 'Invalid credentials.';
							return;
						}
						if (responseType === "Precondition Failed")
						{
							vm.errorMessage = 'Please provide credentials.';
							return;
						}
						if (responseType !== "Accepted")
						{
							vm.errorMessage = 'Unable to login';
							return;
						}
					}

					if (status && $routeParams && $routeParams.redirect)
					{
						path = path + $routeParams.redirect;
					}

					$location.path(path);
				}
			);
	};
};

define
(
	['Scripts/app'],
	function (app) {
		app.register.controller('LoginController', window.chatApp.controllers.LoginController);
	}
);
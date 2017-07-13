'use strict';

window.chatApp.controllers.NavbarController = function ($scope, $location, authService)
{
	var vm = this,
		appTitle = 'My Chat App';

	vm.isCollapsed = false;
	vm.appTitle = appTitle;

	vm.highlight = function (path)
	{
		return $location.path().substr(0, path.length) === path;
	};

	function setLoginLogoutText()
	{
		vm.loginLogoutText = (authService.user.isAuthenticated()) ? 'Logout' : 'Login';
	}

	function redirectToLogin()
	{
		var path = '/login' + $location.$$path;
		$location.replace();
		$location.path(path);
	}

	vm.loginOrOut = function ()
	{
		setLoginLogoutText();
		var isAuthenticated = authService.user.isAuthenticated();
		if (isAuthenticated)
		{
			//logout 
			authService
				.logout()
				.always
				(
					function (jqXHR, statusText, responseType)
					{
						$location.path('/');
						return;
					}
				);
		}
		redirectToLogin();
	};

	$scope.$on('loginStatusChanged', function (loggedIn)
	{
		setLoginLogoutText(loggedIn);
	});

	$scope.$on('redirectToLogin', function ()
	{
		redirectToLogin();
	});

	setLoginLogoutText();
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.controller('NavbarController', window.chatApp.controllers.NavbarController);
	}
);
'use strict';

window.chatApp.controllers.NavbarController = function ($scope, $location, authService)
{
	var vm = this,
		appTitle = 'Customer Management';

	vm.isCollapsed = false;
	vm.appTitle = appTitle;

	vm.highlight = function (path)
	{
		return $location.path().substr(0, path.length) === path;
	};

	function setLoginLogoutText()
	{
		vm.loginLogoutText = (authService.user.isAuthenticated) ? 'Logout' : 'Login';
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
		var isAuthenticated = authService.user.isAuthenticated;
		if (isAuthenticated)
		{
			//logout 
			authService.logout().then(function ()
			{
				$location.path('/');
				return;
			});
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
	['scripts/app'],
	function (app)
	{
		var injectParams = ['$scope', '$location', 'authService'];

		window.chatApp.controllers.NavbarController.$inject = injectParams;

		//Loaded normally since the script is loaded upfront 
		//Dynamically loaded controller use app.register.controller
		app.controller('NavbarController', window.chatApp.controllers.NavbarController);

	}
);
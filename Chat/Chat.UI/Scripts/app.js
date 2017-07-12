'use strict';

define(['services/routeResolver'], function ()
{
	window.app = angular.module('chatApp', ['ngRoute', 'routeResolverServices']);

	window.app.config
	(
		[
			'$routeProvider', 'routeResolverProvider', '$controllerProvider', '$compileProvider', '$filterProvider', '$provide',
			function ($routeProvider, routeResolverProvider, $controllerProvider, $compileProvider, $filterProvider, $provide)
			{
				//Change default views and controllers directory using the following:
				routeResolverProvider.routeConfig.setBaseDirectories(window.chatApp.config.baseUrl + 'views/', window.chatApp.config.baseUrl + 'controllers/');

				window.app.register =
				{
					controller: $controllerProvider.register,
					directive: $compileProvider.directive,
					filter: $filterProvider.register,
					factory: $provide.factory,
					service: $provide.service
				};

				//Define routes - controllers will be loaded dynamically
				var route = routeResolverProvider.route;

				$routeProvider
					.when('/chats', route.resolve('Chats', null, true))
					.when('/chat/:chatRsn', route.resolve('Chat', null, false))
					.when('/login/:redirect*?', route.resolve('Login'))
					.otherwise({ redirectTo: '/chats' });
			}
		]
	);

	window.app.run
	(
		[
			'$q', '$rootScope', '$location', 'authService',
			function ($q, $rootScope, $location, authService)
			{
				$rootScope.$on("$routeChangeStart", function (event, next, current)
				{
					if (next && next.$$route && next.$$route.secure)
					{
						if (!authService.user.isAuthenticated)
						{
							authService.redirectToLogin();
						}
					}
				});
			}
		]
	);

	return window.app;
});
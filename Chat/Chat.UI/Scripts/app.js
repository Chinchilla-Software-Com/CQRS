'use strict';

define(['services/routeResolver'], function ()
{
	window.api.useXToken = true;

	// Declare a proxy to reference the hub.
	window.cqrsNotificationHub = $.connection.notificationHub;

	// Create a function that the hub can call to notify you when it is setup.
	cqrsNotificationHub.client.registered = function () {
		console.info("Now registered to receive notifications.");
	};

	// Create a function that the hub can call to broadcast messages.
	cqrsNotificationHub.client.notifyEvent = function (event)
	{
		console.info(event);
	};

	$.connection.hub.qs = { "X-Token": Cookies.get("X-Token") };
	$.connection.logging = false;

	window.cqrsNotificationHub.GlobalEventHandlers = {};

	// Create a function that the hub can call to broadcast messages.
	window.cqrsNotificationHub.client.notifyEvent = function (event)
	{
		console.info(event);
		var handlers = window.cqrsNotificationHub.GlobalEventHandlers[event.Type];
		if (handlers != null)
		{
			if (handlers.constructor === Array)
				for (var i = 0; i < handlers.length; i++)
					handlers[i](event);
			else
				handlers(event);
		}
	};

	window.app = angular.module('chatApp', ['ngRoute', 'routeResolverServices', 'wc.directives']);

	window.app.config
	(
		function ($routeProvider, routeResolverProvider, $controllerProvider, $compileProvider, $filterProvider, $provide, $locationProvider)
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
				//route.resolve() now accepts the convention to use (name of controller & view) as well as the 
				//path where the controller or view lives in the controllers or views folder if it's in a sub folder. 
				//For example, the controllers for customers live in controllers/customers and the views are in views/customers.
				//The controllers for orders live in controllers/orders and the views are in views/orders
				//The second parameter allows for putting related controllers/views into subfolders to better organize large projects
				//Thanks to Ton Yeung for the idea and contribution
				.when('/chat/:chatRsn', route.resolve('Chat', null, 'vm', true))
				.when('/chats', route.resolve('Chats', null, 'vm', true))
				.when('/about', route.resolve('About', null, 'vm'))
				.when('/login/:redirect*?', route.resolve('Login', null, 'vm'))
				.otherwise({ redirectTo: '/chats' });

			// use the HTML5 History API
			$locationProvider.html5Mode(true);
		}
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
						if (!authService.user.isAuthenticated())
						{
							$rootScope.$evalAsync(function () {
								authService.redirectToLogin();
							});
						}
					}
				});
			}
		]
	);

	return window.app;
});
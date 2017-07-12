'use strict';

define([], function ()
{
	var routeResolver = function ()
	{
		this.$get = function ()
		{
			return this;
		};

		this.routeConfig = function ()
		{
			var viewsDirectory = '/views/',
				controllersDirectory = '/controllers/',

			setBaseDirectories = function (viewsDir, controllersDir)
			{
				viewsDirectory = viewsDir;
				controllersDirectory = controllersDir;
			},

			getViewsDirectory = function ()
			{
				return viewsDirectory;
			},

			getControllersDirectory = function ()
			{
				return controllersDirectory;
			};

			return {
				setBaseDirectories: setBaseDirectories,
				getControllersDirectory: getControllersDirectory,
				getViewsDirectory: getViewsDirectory
			};
		}();

		this.route = function (routeConfig)
		{
			var resolveDependencies = function ($q, $rootScope, dependencies)
			{
				var defer = $q.defer();
				require(dependencies, function ()
				{
					defer.resolve();
					$rootScope.$apply();
				});

				return defer.promise;
			},

			resolve = function (baseName, path, controllerAs, secure)
			{
				if (!path) path = '';

				var routeDef = {};
				routeDef.templateUrl = routeConfig.getViewsDirectory() + path + baseName + '.html';
				routeDef.controller = baseName + 'Controller';
				if (controllerAs)
					routeDef.controllerAs = controllerAs;
				routeDef.secure = (secure) ? secure : false;
				routeDef.resolve = {
					load: ['$q', '$rootScope', function ($q, $rootScope)
					{
						var dependencies = [routeConfig.getControllersDirectory() + path + baseName + 'Controller.js'];
						return resolveDependencies($q, $rootScope, dependencies);
					}]
				};

				return routeDef;
			};

			return { resolve: resolve };
		}(this.routeConfig);

	};

	window.chatApp.services.resolver = angular.module('routeResolverServices', []);

	//Must be a provider since it will be injected into module.config()
	window.chatApp.services.resolver.provider('routeResolver', routeResolver);
});
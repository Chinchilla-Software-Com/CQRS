'use strict';

define(['Scripts/app'], function (app)
{
	app.config(function ($httpProvider)
	{
		var httpInterceptor401 = function ($q, $rootScope)
		{
			var error = function (res)
			{
				if (res.status === 401 || res.status === 403)
				{
					//Raise event so listener (navbarController) can act on it
					$rootScope.$broadcast('redirectToLogin', null);
					return $q.reject(res);
				}
				return $q.reject(res);
			};

			return {
				request: function (config)
				{
					return config || $q.when(config);
				},
				requestError: function (rejection)
				{
					return error(rejection);
				},
				response: function (response)
				{
					return response || $q.when(response);
				},
				responseError: function (rejection)
				{
					return error(rejection);
				}
			};
		};

		$httpProvider.interceptors.push(httpInterceptor401);
	});
});
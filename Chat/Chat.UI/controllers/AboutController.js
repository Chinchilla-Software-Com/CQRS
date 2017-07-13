'use strict';

window.chatApp.controllers.AboutController = function ()
{
}

define
(
	['scripts/app'],
	function (app) {
		app.register.controller('AboutController', ['$scope', window.chatApp.controllers.AboutController]);
	}
);
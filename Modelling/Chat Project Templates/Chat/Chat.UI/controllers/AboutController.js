'use strict';

window.chatApp.controllers.AboutController = function ()
{
}

define
(
	['Scripts/app'],
	function (app) {
		app.register.controller('AboutController', window.chatApp.controllers.AboutController);
	}
);
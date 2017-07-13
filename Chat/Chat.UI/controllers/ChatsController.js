'use strict';

window.chatApp.controllers.ChatsController = function ($scope)
{
	// scope variables
	$scope.chats = []; // collection of messages coming from server
};

define
(
	['scripts/app'],
	function (app)
	{
		app.register.controller('ChatsController', ['$scope', window.chatApp.controllers.ChatsController]);
	}
);
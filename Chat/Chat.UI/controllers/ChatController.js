'use strict';

window.chatApp.controllers.ChatController = function ($scope)
{
	// scope variables
	$scope.name = 'Guest'; // holds the user's name
	$scope.message = ''; // holds the new message
	$scope.messages = []; // collection of messages coming from server
	$scope.notificationHub = window.cqrsNotificationHub;

	window.cqrsNotificationHub.GlobalEventHandlers[""] = function (event)
	{
		var newMessage = event.data.name + ' says: ' + event.data.message;

		// push the newly coming message to the collection of messages
		$scope.messages.push(newMessage);
		$scope.$apply();
	};

	$scope.newMessage = function ()
	{
		// sends a new message to the server
		$scope.notificationHub.server.sendMessage($scope.name, $scope.message);

		$scope.message = '';
	};
};

define
(
	['scripts/app'],
	function (app)
	{
		app.register.controller('ChatController', ['$scope', window.chatApp.controllers.ChatController]);
	}
);
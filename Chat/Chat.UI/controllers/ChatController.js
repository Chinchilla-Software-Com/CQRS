'use strict';

window.chatApp.controllers.ChatController = function ($scope)
{
	// scope variables
	$scope.name = 'Guest'; // holds the user's name
	$scope.message = ''; // holds the new message
	$scope.messages = []; // collection of messages coming from server
	$scope.chatHub = null; // holds the reference to hub

	$scope.chatHub = $.connection.chatHub; // initializes hub
	$.connection.hub.start(); // starts hub

	// register a client method on hub to be invoked by the server
	$scope.chatHub.client.broadcastMessage = function (name, message)
	{
		var newMessage = name + ' says: ' + message;

		// push the newly coming message to the collection of messages
		$scope.messages.push(newMessage);
		$scope.$apply();
	};

	$scope.newMessage = function ()
	{
		// sends a new message to the server
		$scope.chatHub.server.sendMessage($scope.name, $scope.message);

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
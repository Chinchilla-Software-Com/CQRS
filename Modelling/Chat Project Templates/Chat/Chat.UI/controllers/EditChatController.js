'use strict';

window.chatApp.controllers.EditChatController = function ($scope, $routeParams, $timeout, modalService, $location)
{
	var vm = this,
		chatRsn = $routeParams.chatRsn,
		timer,
		onRouteChangeOff;

	vm.conversation = { "Rsn": chatRsn };
	vm.title = "";
	vm.buttonText = "";
	vm.requestMade = false;
	vm.updateStatus = false;
	vm.startStatus = false;
	vm.errorMessage = "";
	vm.messageCount = 0;
	vm.correlationId = "";

	function setVariables()
	{
		vm.title = (chatRsn !== undefined) ? 'Edit An Existing' : 'Start A New';
		vm.buttonText = (chatRsn !== undefined) ? 'Update' : 'Start';
	}

	function startTimer()
	{
		timer = $timeout(function ()
		{
			$timeout.cancel(timer);
			vm.errorMessage = "";
			vm.requestMade = false;
			vm.updateStatus = false;
			vm.startStatus = false;
		}, 3000);
	}

	window.cqrsNotificationHub
		.GlobalEventHandlers["$ext_safeprojectname$.MicroServices.Conversations.Events.ConversationStarted"] =
		function (event)
		{
			if (event.CorrelationId === vm.correlationId)
			{
				chatRsn = event.Data.Rsn;
				vm.conversation.Rsn = event.Data.Rsn;

				setVariables();

				vm.requestMade = false;
				vm.startStatus = true;
				startTimer();
				$scope.$apply();
			}
		};
	window.cqrsNotificationHub
		.GlobalEventHandlers["$ext_safeprojectname$.MicroServices.Conversations.Events.CommentPosted"] =
		function (event)
		{
			if (event.CorrelationId === vm.correlationId || event.Data.ConversationRsn === chatRsn)
			{
				vm.requestMade = false;
				vm.messageCount++;
				$scope.$apply();
			}
		};
	window.cqrsNotificationHub
		.GlobalEventHandlers["$ext_safeprojectname$.MicroServices.Conversations.Events.ConversationUpdated"] =
		function (event)
		{
			if (event.CorrelationId === vm.correlationId)
			{
				vm.requestMade = false;
				vm.updateStatus = true;
				startTimer();
				$scope.$apply();
			}
		};
	window.cqrsNotificationHub
		.GlobalEventHandlers["$ext_safeprojectname$.MicroServices.Conversations.Events.ConversationDeleted"] =
		function (event)
		{
			if (event.CorrelationId === vm.correlationId || event.Data.ConversationRsn === chatRsn)
			{
				onRouteChangeOff(); //Stop listening for location changes
				$location.path('/chats');
				$scope.$apply();
			}
		};

	function processSuccess(result)
	{
		$scope.editForm.$dirty = false;
		vm.requestMade = true;
		vm.correlationId = result.CorrelationId;
	}

	function processError(error)
	{
		vm.errorMessage = error;
		startTimer();
	}

	vm.saveConversation = function ()
	{
		if ($scope.editForm.$valid)
		{
			var operation;
			var parameters = { "name": vm.conversation.Name };

			if (!vm.conversation.Rsn)
			{
				operation = window.api.Conversations.StartConversation;
			}
			else
			{
				operation = window.api.Conversations.UpdateConversation;
				parameters.conversationRsn = vm.conversation.Rsn;
			}

			operation(parameters)
				.done
				(
					processSuccess
				)
				.fail
				(
					function (jqXHR, textStatus, errorThrown)
					{
						processError(textStatus);
					}
				);
		}
	};

	vm.deleteConversation = function ()
	{
		var modalOptions =
		{
			closeButtonText: 'Keep Conversation',
			actionButtonText: 'Delete Conversation',
			headerText: 'Delete ' + vm.conversation.Name + '?',
			bodyText: 'Are you sure you want to delete this conversation?'
		};

		modalService
			.showModal({}, modalOptions)
			.then
			(
				function (result)
				{
					if (result === 'ok')
					{
						window.api.Conversations
							.DeleteConversation({ "conversationRsn": vm.conversation.Rsn })
							.done
							(
								processSuccess
							)
							.fail
							(
								function (jqXHR, textStatus, errorThrown)
								{
									processError(textStatus);
								}
							);
					}
				}
			);
	};

	function getMessages()
	{
		if (chatRsn === undefined)
			return;
		window.api.Conversations
			.GetMessages({ "conversationRsn": chatRsn })
			.done
			(
				function (result, textStatus, jqXHR)
				{
					var data = result.ResultData;
					vm.messageCount = data.length;

					vm.conversation.Name = data[0].ConversationName;
				}
			)
			.fail
			(
				function (jqXHR, textStatus, errorThrown)
				{
					console.error(textStatus, errorThrown);
				}
			);
	}

	function routeChange(event, newUrl, oldUrl)
	{
		//Navigate to newUrl if the form isn't dirty
		if (!vm.editForm || !vm.editForm.$dirty) return;

		var modalOptions = {
			closeButtonText: 'Cancel',
			actionButtonText: 'Ignore Changes',
			headerText: 'Unsaved Changes',
			bodyText: 'You have unsaved changes. Leave the page?'
		};

		modalService.showModal({}, modalOptions).then(function (result)
		{
			if (result === 'ok')
			{
				onRouteChangeOff(); //Stop listening for location changes
				$location.path($location.url(newUrl).hash()); //Go to page they're interested in
			}
		});

		//prevent navigation by default since we'll handle it
		//once the user selects a dialog option
		event.preventDefault();
		return;
	}

	function init()
	{
		setVariables();
		getMessages();

		//Make sure they're warned if they made a change but didn't save it
		//Call to $on returns a "deregistration" function that can be called to
		//remove the listener (see routeChange() for an example of using it)
		onRouteChangeOff = $scope.$on('$locationChangeStart', routeChange);
	}

	init();
};

define
(
	['Scripts/app'],
	function (app) {
		app.register.controller('EditChatController', window.chatApp.controllers.EditChatController);
	}
);
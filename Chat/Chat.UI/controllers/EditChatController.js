'use strict';

window.chatApp.controllers.EditChatController = function ($scope, $routeParams, $timeout)
{
	var vm = this,
		chatRsn = $routeParams.chatRsn,
		timer,
		onRouteChangeOff;

	vm.conversation = { "Rsn": chatRsn };
	vm.title = (chatRsn !== undefined) ? 'Edit An Existing' : 'Start A New';
	vm.buttonText = (chatRsn !== undefined) ? 'Update' : 'Start';
	vm.updateStatus = false;
	vm.errorMessage = '';
	vm.messageCount = 0;

	window.cqrsNotificationHub
		.GlobalEventHandlers["Chat.MicroServices.Conversations.Events.CommentPosted"] =
		function (event)
		{
			if (event.Data.ConversationRsn === chatRsn)
			{
				vm.messages.push
				(
					{
						"Rsn": event.Data.Rsn,
						"ConversationRsn": event.Data.ConversationRsn,
						"ConversationName": event.Data.ConversationName,
						"UserRsn": event.Data.UserRsn,
						"UserName": event.Data.UserName,
						"Content": event.Data.Comment,
						"DatePosted": event.Data.DatePosted
					}
				);
			}
		}

	vm.saveConversation = function ()
	{
		if ($scope.editForm.$valid)
		{
			if (!vm.conversation.Rsn)
			{
				dataService.insertCustomer(vm.customer).then(processSuccess, processError);
			}
			else {
				dataService.updateCustomer(vm.customer).then(processSuccess, processError);
			}
		}
	};

	vm.deleteConversation = function ()
	{
		var modalOptions =
		{
			closeButtonText: 'Cancel',
			actionButtonText: 'Delete Conversation',
			headerText: 'Delete ' + vm.conversation.Name + '?',
			bodyText: 'Are you sure you want to delete this conversation?'
		};

		modalService.showModal({}, modalOptions).then(function (result) {
			if (result === 'ok') {
				dataService.deleteCustomer(vm.customer.id).then(function () {
					onRouteChangeOff(); //Stop listening for location changes
					$location.path('/customers');
				}, processError);
			}
		});
	};

	vm.postReply = function () {
		window.api.Conversations
			.PostComment({ "conversationRsn": chatRsn, "comment": vm.comment })
			.done
			(
				function (result, textStatus, jqXHR) {
					vm.comment = "";
				}
			)
			.fail
			(
				function (jqXHR, textStatus, errorThrown) {
					console.error(textStatus, errorThrown);
				}
			);
	}

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
		getMessages();

		//Make sure they're warned if they made a change but didn't save it
		//Call to $on returns a "deregistration" function that can be called to
		//remove the listener (see routeChange() for an example of using it)
		onRouteChangeOff = $scope.$on('$locationChangeStart', routeChange);
	}

	function startTimer()
	{
		timer = $timeout(function ()
		{
			$timeout.cancel(timer);
			vm.errorMessage = '';
			vm.updateStatus = false;
		}, 3000);
	}

	function processSuccess()
	{
		$scope.editForm.$dirty = false;
		vm.updateStatus = true;
		vm.title = 'Edit';
		vm.buttonText = 'Update';
		startTimer();
	}

	function processError(error)
	{
		vm.errorMessage = error.message;
		startTimer();
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
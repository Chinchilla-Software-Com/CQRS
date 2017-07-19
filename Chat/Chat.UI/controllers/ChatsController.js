'use strict';

window.chatApp.controllers.ChatsController = function ($scope, $filter, $timeout, $location, authService, modalService)
{
	var vm = this;

	vm.conversations = [];
	vm.filteredConversations = [];
	vm.filteredCount = 0;
	vm.orderby = 'Name';
	vm.reverse = false;
	vm.searchText = null;
	vm.cardAnimationClass = '.card-animation';
	vm.correlationId = "";

	//paging
	vm.totalRecords = 0;
	vm.pageSize = 10;
	vm.currentPage = 1;

	vm.DisplayModeEnum = {
		Card: 0,
		List: 1
	};

	vm.changeDisplayMode = function (displayMode)
	{
		switch (displayMode)
		{
			case vm.DisplayModeEnum.Card:
				vm.listDisplayModeEnabled = false;
				break;
			case vm.DisplayModeEnum.List:
				vm.listDisplayModeEnabled = true;
				break;
		}
	};

	function filterConversations(filterText)
	{
		vm.filteredConversations = $filter("ConversationNameFilter")(vm.conversations, filterText);
		vm.filteredCount = vm.filteredConversations.length;
	}

	vm.searchTextChanged = function ()
	{
		filterConversations(vm.searchText);
	};

	function getConversations()
	{
		window.api.Conversations
			.Get()
			.done
			(
				function (result, textStatus, jqXHR)
				{
					var data = result.ResultData;
					vm.totalRecords = data.length;
					vm.conversations = data;
					filterConversations('');

					$timeout(function ()
					{
						//Turn off animation since it won't keep up with filtering
						vm.cardAnimationClass = '';
					}, 1000);
					$scope.$apply();
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

	vm.pageChanged = function (page)
	{
		vm.currentPage = page;
		getConversations();
	};

	vm.navigate = function (url)
	{
		$location.path(url);
	};

	function getConversationByRsn(rsn)
	{
		for (var i = 0; i < vm.conversations.length; i++)
		{
			var conversation = vm.conversations[i];
			if (conversation.Rsn === rsn)
				return conversation;
		}
		return null;
	}

	vm.deleteConversation = function (rsn)
	{
		if (!authService.user.isAuthenticated)
		{
			$location.path(authService.loginPath + $location.$$path);
			return;
		}

		var conversation = getConversationByRsn(rsn);

		var modalOptions = {
			closeButtonText: 'Cancel',
			actionButtonText: 'Delete Conversation',
			headerText: 'Delete ' + conversation.Name + '?',
			bodyText: 'Are you sure you want to delete this conversation?'
		};

		modalService.showModal({}, modalOptions).then(function (result)
		{
			if (result === 'ok')
			{
				window.api.Conversations
					.DeleteConversation({ "conversationRsn": conversation.Rsn })
					.done
					(
						function (result, textStatus, jqXHR)
						{
							vm.correlationId = result.CorrelationId;
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
		});
	};

	function init()
	{
		getConversations();
	}

	window.cqrsNotificationHub
		.GlobalEventHandlers["Chat.MicroServices.Conversations.Events.ConversationDeleted"] =
		function (event)
		{
			if (event.CorrelationId === vm.correlationId)
				vm.correlationId = "";

			getConversations();
		};

	init();
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.register.controller('ChatsController', window.chatApp.controllers.ChatsController);
	}
);
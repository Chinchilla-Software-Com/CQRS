'use strict';

window.chatApp.controllers.ChatsController = function ($scope, $filter, $timeout, $location)
{
	var vm = this;

	vm.conversations = [];
	vm.filteredConversations = [];
	vm.filteredCount = 0;
	vm.orderby = 'Name';
	vm.reverse = false;
	vm.searchText = null;
	vm.cardAnimationClass = '.card-animation';


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

	function init()
	{
		getConversations();
	}

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
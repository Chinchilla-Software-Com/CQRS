'use strict';

window.chatApp.controllers.ChatController = function ($scope, $routeParams, $timeout)
{
	var vm = this,
		chatRsn = $routeParams.chatRsn;

	vm.messages = [];
	vm.cardAnimationClass = '.card-animation';
	vm.ConversationName = "";
	vm.comment = "";

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
				$scope.$apply();
			}
		};

	vm.formatContent = function (content)
	{
		return content.replace(/\r\n/g, '<br />\r\n');
	}

	vm.postReply = function ()
	{
		window.api.Conversations
			.PostComment({ "conversationRsn": chatRsn, "comment": vm.comment })
			.done
			(
				function (result, textStatus, jqXHR)
				{
					vm.comment = "";
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

	function getMessages()
	{
		window.api.Conversations
			.GetMessages({ "conversationRsn": chatRsn })
			.done
			(
				function (result, textStatus, jqXHR)
				{
					var data = result.ResultData;
					vm.totalRecords = data.length;
					vm.messages = data;

					vm.ConversationName = data[0].ConversationName;

					$timeout(function ()
					{
						//Turn off animation since it won't keep up with filtering
						vm.cardAnimationClass = '';
					}, 1000);
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

	function init()
	{
		getMessages();
	}

	init();
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.register.controller('ChatController', window.chatApp.controllers.ChatController);
	}
);
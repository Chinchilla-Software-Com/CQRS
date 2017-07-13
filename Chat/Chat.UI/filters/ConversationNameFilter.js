'use strict';

window.chatApp.filters.ConversationNameFilter = function ()
{
	return function (conversations, filterValue)
	{
		if (!filterValue || !conversations)
			return conversations;

		var matches = [];
		filterValue = filterValue.toLowerCase();
		for (var i = 0; i < conversations.length; i++)
		{
			var conversation = conversations[i];
			if (conversation.Name.toLowerCase().indexOf(filterValue) > -1)
				matches.push(conversation);
		}
		return matches;
	};
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.filter('ConversationNameFilter', window.chatApp.filters.ConversationNameFilter);
	}
);
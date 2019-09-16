'use strict';

window.chatApp.filters.ConversationContentFormatter = function ($sce)
{
	return function (content)
	{
		return $sce.trustAsHtml(content.replace(/\r\n/g, '<br />\r\n'));
	};
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.filter('ConversationContentFormatter', window.chatApp.filters.ConversationContentFormatter);
	}
);
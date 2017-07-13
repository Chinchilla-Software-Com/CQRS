'use strict';

window.chatApp.filters.DateFormattingFilter = function ()
{
	return function (dateValueAsString)
	{
		return new Date(Date.parse(dateValueAsString));
	};
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.filter('DateFormattingFilter', window.chatApp.filters.DateFormattingFilter);
	}
);
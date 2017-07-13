window.chatApp = {};
window.chatApp.controllers = {};
window.chatApp.services = {};
window.chatApp.filters = {};

window.chatApp.config = {
	baseUrl: '/Chat/UI',
	urlArgs: 'v=1.0'
};

require.config(window.chatApp.config);

require
(
	[
		'Scripts/app',
		'services/routeResolver',
		'services/authService',
		'services/httpInterceptors',
		'filters/ConversationNameFilter',
		'filters/DateFormattingFilter',
		'controllers/NavbarController'
	],
	function ()
	{
		angular.bootstrap(document, ['chatApp']);
	}
);
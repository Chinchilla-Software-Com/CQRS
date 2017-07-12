window.chatApp = {};
window.chatApp.controllers = {};
window.chatApp.services = {};

window.chatApp.config = {
	baseUrl: '/Chat/UI',
	urlArgs: 'v=1.0'
};

require.config(window.chatApp.config);

require
(
	[
		'scripts/app',
		'services/routeResolver',
		'services/authService'
],
	function ()
	{
		angular.bootstrap(document, ['chatApp']);
	}
);
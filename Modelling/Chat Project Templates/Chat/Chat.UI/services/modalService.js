"use strict";

window.chatApp.services.modalService = function ($uibModal)
{
	var modalDefaults = {
		backdrop: true,
		keyboard: true,
		modalFade: true,
		templateUrl: window.chatApp.config.baseUrl + 'partials/modal.html'
	};

	var modalOptions = {
		closeButtonText: 'Close',
		actionButtonText: 'OK',
		headerText: 'Proceed?',
		bodyText: 'Perform this action?'
	};

	this.showModal = function (customModalDefaults, customModalOptions)
	{
		if (!customModalDefaults) customModalDefaults = {};
		customModalDefaults.backdrop = 'static';
		return this.show(customModalDefaults, customModalOptions);
	};

	this.show = function (customModalDefaults, customModalOptions)
	{
		//Create temp objects to work with since we're in a singleton service
		var tempModalDefaults = {};
		var tempModalOptions = {};

		//Map angular-ui modal custom defaults to modal defaults defined in this service
		angular.extend(tempModalDefaults, modalDefaults, customModalDefaults);

		//Map modal.html $scope custom properties to defaults defined in this service
		angular.extend(tempModalOptions, modalOptions, customModalOptions);

		if (!tempModalDefaults.controller)
		{
			tempModalDefaults.controller = function ($scope, $uibModalInstance)
			{
				$scope.modalOptions = tempModalOptions;
				$scope.modalOptions.ok = function (result)
				{
					$uibModalInstance.close('ok');
				};
				$scope.modalOptions.close = function (result)
				{
					$uibModalInstance.close('cancel');
				};
			};
		}

		return $uibModal.open(tempModalDefaults).result;
	};
};

define
(
	['Scripts/app'],
	function (app)
	{
		app.service('modalService', window.chatApp.services.modalService);
	}
);
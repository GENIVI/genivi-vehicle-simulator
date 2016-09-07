'use strict';

angular.module('myApp.view2', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/select_additional', {
    templateUrl: 'view2/view2.html',
    controller: 'View2Ctrl',
      activetab: 'select_additional'
  });
}])

.controller('View2Ctrl', ['$scope', '$rootScope', function ($scope, $rootScope) {

    $rootScope.bgtype = "regular";

	$scope.sliders = [
		{title: "Music", value: 0.8},
		{title: "Foley", value: 0.8},
		{title: "Vehicle", value: 0.8}
	];
}]);
'use strict';

angular.module('myApp.view1', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/select_select', {
    templateUrl: 'view1/view1.html',
    controller: 'View1Ctrl',
    activetab: 'select_select'
  });
}])

.controller('View1Ctrl', ['$scope', '$rootScope', 'networkController', function($scope, $rootScope, networkController) {
    $rootScope.bgtype = "select";    

    $scope.cars = [
        {img : "images/Select/xj_thumb.png", name : "2014 Jaguar XJ", idx: 0},
        {img : "images/Select/L405_thumb.png", name : "2014 Range Rover", idx: 1},
        {img : "images/Select/LR4_thumb.png", name : "2014 Land Rover Discovery", idx: 2},
        {img : "images/Select/evoque_thumb.png", name : "2014 Land Rover Evoque", idx: 3},
        {img : "images/Select/ftypecoupe_thumb.png", name : "2014 Jaguar F-Type Coupe", idx: 4},
        {img : "images/Select/ftype_thumb.png", name : "2014 Jaguar F-Type", idx: 5}
    ];

    $scope.ClickCar = function(idx) {
        //TODO: watch state for allowed
        networkController.SelectCar(idx);
    }

    $scope.Back = function () {      
        if ($rootScope.selectingScene)
            networkController.SelectCancel();
    }

    $scope.scenes = [
        {img: "images/Select/YosThumb.png", name: "Scenic", idx: 0},
        {img: "images/Select/SFThumb.png", name: "Urban", idx: 1},
        {img: "images/Select/PCHThumb.png", name: "Coastal", idx: 2}
    ];


    $scope.ClickScene = function (idx) {
        networkController.SelectScene(idx);
    }

    $rootScope.$on('SELECT_CAR', function (event, args) {
        var newCar = args.int;
        if (newCar < 0) {
            $rootScope.selectingCar = true;
            $rootScope.selectingScene = false;
            $rootScope.currentScene = -1;
            $rootScope.selectedScene = -1;
        } else {
            $rootScope.selectingCar = false;
            $rootScope.selectingScene = true;
        }
        $rootScope.currentVehicle = newCar;
        $scope.selectedCar = newCar;

        $rootScope.$apply();

    });

    $rootScope.$on('SELECT_SCENE', function (event, args) {
        var newScene = args.int;
        $rootScope.currentScene = newScene;
        $rootScope.selectedScene = newScene;
        $rootScope.selectingCar = false;
        $rootScope.selectingScene = false;

        $rootScope.$apply();
    });







}]);
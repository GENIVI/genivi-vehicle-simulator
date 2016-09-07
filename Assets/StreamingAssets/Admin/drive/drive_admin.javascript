'use strict';

angular.module('myApp.Drive', ['ngRoute'])

.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/drive_admin', {
        templateUrl: 'drive/drive_admin.html',
        controller: 'DriveAdminCtrl',
        activetab: 'drive_admin'
    });

    $routeProvider.when('/drive_weather', {
        templateUrl: 'drive/drive_weather.html',
        controller: 'DriveWeatherCtrl',
        activetab: 'drive_weather'
    });

    $routeProvider.when('/drive_audio', {
        templateUrl: 'drive/drive_audio.html',
        controller: 'DriveAudioCtrl',
        activetab: 'drive_audio'
    });

    $routeProvider.when('/drive_map', {
        templateUrl: 'drive/drive_map.html',
        controller: 'DriveMapCtrl',
        activetab: 'drive_map'
    });

    $routeProvider.when('/drive_endscene', {
        templateUrl: 'drive/drive_endscene.html',
        controller: 'DriveEndSceneCtrl',
        activetab: 'drive_endscene'
    });

}])

.controller('DriveAdminCtrl', ['$scope', '$rootScope', 'networkController', function ($scope, $rootScope, networkController) {

    $rootScope.bgtype = "regular";

    $scope.$root = $rootScope;

    $scope.obstacles = [
        { title: "OBSTACLE ONE", id: 0 },
        { title: "OBSTACLE TWO", id: 1 },
        { title: "OBSTACLE THREE", id: 2 },
        { title: "OBSTACLE FOUR", id: 3 },
        { title: "OBSTACLE FIVE", id: 4 },
        { title: "DESTROY OBSTACLES", id: -1 },
    ];

    $scope.waypoints = [
        { title: "WAYPOINT ONE", id: 0 },
        { title: "WAYPOINT TWO", id: 1 },
        { title: "WAYPOINT THREE", id: 2 },
        { title: "WAYPOINT FOUR", id: 3 },
    ];

    $scope.predefinedPaths = [
        { title: "PREDEFINED PATH ONE", id: 0 },
        { title: "PREDEFINED PATH TWO", id: 1 },
        { title: "PREDEFINED PATH THREE", id: 2 },
        { title: "PREDEFINED PATH FOUR", id: 3 },
    ];

    $scope.ClickObstacle = function (idx) {
        networkController.TriggerObstacle(idx);
    }

    $scope.ClickWaypoint = function (idx) {
        networkController.TriggerWaypoint(idx);
    }

    $scope.ClickPredefinedPath = function (idx) {
        networkController.TriggerPredefinedPath(idx);
    }
    
    $scope.OnTrafficChange = function () {
        networkController.SetTraffic($rootScope.enableTraffic);
    }

    $scope.OnAutoPlayChange = function () {
        networkController.SetAutoPlay($rootScope.enableAutoPlay);
    }





}]).controller('DriveWeatherCtrl', ['$scope', '$rootScope', '$timeout', 'networkController', function ($scope, $rootScope, $timeout, networkController) {

    $rootScope.bgtype = "regular";

    $scope.$root = $rootScope;

    $scope.OnWeatherChange = function () {
        networkController.SetWeather($rootScope.currentWeather);
    }

    $scope.OnWetRoadsChange = function () {
        networkController.SetWetRoads($rootScope.wetRoads);
    }

    $scope.OnSunShaftsChange = function () {
        networkController.SetSunShafts($rootScope.sunShafts);
    }

    $scope.OnProgressTimeChange = function () {
        networkController.SetProgressTime($rootScope.progressTime);
    }

    $scope.timeout = false;
    $scope.timeoutwatcher = null;
    
    
    $scope.OnTimeOfDayChange = function () {
        console.log("ontodchange");
        if ($scope.timeoutwatcher != null)
            $timeout.cancel($scope.timeoutwatcher);

        $scope.timeoutwatcher = $timeout(function () {
            $scope.timeout = false;
        }, 2000);

        networkController.SetTimeOfDayLimited($scope.time);
    }

    $scope.time = $rootScope.timeOfDay;

    $rootScope.$on('SET_TIME_OF_DAY', function (event, args) {
        if ($scope.timeout === false) {
            $scope.time = $rootScope.timeOfDay;
            $scope.$apply();
        }
    });

    $scope.OnFovChange = function () {
        networkController.SetFoVLimited($rootScope.fov)
    }

    $scope.OnFarClipChange = function () {
        networkController.SetCamFarClipLimited($rootScope.camFarClip);
    }

}]).controller('DriveAudioCtrl', ['$scope', '$rootScope', function ($scope, $rootScope) {

    $rootScope.bgtype = "regular";

    $scope.sliders = [
        { title: "Music", value: 0.8 },
        { title: "Foley", value: 0.8 },
        { title: "Vehicle", value: 0.8 }
    ];
}]).controller('DriveMapCtrl', ['$scope', '$rootScope', function ($scope, $rootScope) {

    $rootScope.bgtype = "regular";
    
    $scope.mapImage = "images/MapYos.png"

    $scope.getTransform = function () {
        return "translate(" + ($rootScope.mapXPos * 763 - 6) + "px, " + ((1 - $rootScope.mapZPos) * 370 - 6) + "px)";
    }

    if ($rootScope.currentScene == 0) {
        $scope.mapImage = "images/MapYos.png";
    }
    else if ($rootScope.current == 1) {
        $scope.mapImage = "images/MapSF.png";
    }
    else if ($rootScope.current == 2) {
        $scope.mapImage = "images/PCH.png";
    }


}])
.controller('DriveEndSceneCtrl', ['$scope', '$rootScope', 'networkController', function ($scope, $rootScope, networkController) {

    $rootScope.bgtype = "regular";

    $scope.OnClickEndScene = function () {
        networkController.ConfirmEndScene();
    }

    $scope.OnClickCancel = function () {
        networkController.CancelEndScene();
    }

}]);
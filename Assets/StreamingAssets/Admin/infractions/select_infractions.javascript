'use strict';

angular.module('myApp.Infractions', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/select_infractions', {
    templateUrl: 'infractions/select_infractions.html',
    controller: 'SelectInfractionsCtrl',
      activetab: 'select_infractions'
  });
}])

.controller('SelectInfractionsCtrl', ['$scope', '$rootScope', 'networkController', function ($scope, $rootScope, networkController) {

    $rootScope.bgtype = "regular";

    $scope.selectedInfraction = null;

    $scope.infractions = $rootScope.infractions;

    $scope.OnClickInfraction = function (id) {
        for (var i = 0; i < $scope.infractions.length; i++) {
            if ($scope.infractions[i].id == id) {
                $scope.selectedInfraction = $scope.infractions[i];
                networkController.SelectInfraction(id);
            }
        }
    }

    $scope.GetImageUrl = function() {
        if($scope.selectedInfraction == null)
            return "";

        return networkController.GetScreenShotUrl($scope.selectedInfraction.id);

    }

    $scope.GetType = function () {
        if ($scope.selectedInfraction == null)
            return "";

        var infractionText = "";
        switch ($scope.selectedInfraction.type) {
            case "LANE":
                infractionText = "Lane Infraction";
                break;
            case "ENV":
                infractionText = "Environment Collision";
                break;
            case "TRAF":
                infractionText = "Vehicle Collision";
                break;
            case "OBS":
                infractionText = "Obstacle Collision";
                break;
            case "STOP":
                infractionText = "Stop Sign Infraction";
                break;
            case "LIGHT":
                infractionText = "Traffic Light Infraction";
                break;
            default:
                infractionText = "Infraction";
                break;
        }

        return infractionText;
    }

    $scope.GetLastScene = function () {
        if ($rootScope.lastScene == 0)
            return "Scenic";
        else if ($rootScope.lastScene == 1)
            return "Urban";
        else if ($rootScope.lastScene == 2)
            return "Coastal";

        return "None";
    }

    $scope.GetSystemTime = function (inf) {
        if (inf == null)
            return "";

        var d = new Date(inf.sysTime);
        var hrs = d.getHours();

        var mins = d.getMinutes();
        if (mins < 10)
            mins = "0" + mins;

        if (hrs <= 12)
            mins = mins + " am";
        else
            mins = mins + " pm";

        if (hrs > 12)
            hrs -= 12;

       return hrs + ":" + mins

    }

    $scope.GetSessionTime = function (inf) {

        if (inf == null)
            return "";

        var sec_num = parseInt(inf.sessionTime, 10); // don't forget the second param
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var seconds = sec_num - (hours * 3600) - (minutes * 60);

        if (hours < 10) { hours = "0" + hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        var time = hours + ':' + minutes + ':' + seconds;
        return time;
    }
    
    $scope.$on("$destroy", function () {
        networkController.ClearSelectedInfraction();
    });


}]);
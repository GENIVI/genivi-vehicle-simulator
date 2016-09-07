'use strict';

function NetworkController($rootScope, $location, $interval) {

    $rootScope.lastScene = -1;


    this.TriggerDriveScene = function () {
        $rootScope.drive = true;
        $location.path('/drive_admin')
    };

    this.TriggerSelectScene = function () {

        //store last scene for infractions
        $rootScope.lastScene = $rootScope.currentScene;

        $rootScope.drive = false;
        $rootScope.selectingCar = true;
        $rootScope.selectingScene = false;
        $rootScope.currentScene = -1;
        $rootScope.currentVehicle = -1;
        $rootScope.selectedScene = -1;
        $location.path('/select_select')
    };

    this.SelectCar = function (car) {
        websocket.send(this.GetMessage("SELECT_CAR", car));
    }

    this.SelectScene = function (scene) {
        websocket.send(this.GetMessage("SELECT_SCENE", scene));
    }

    this.SelectCancel = function () {
        websocket.send(this.GetMessage("SELECT_BACK", ""));
    }

    this.TriggerObstacle = function (obs) {
        websocket.send(this.GetMessage("TRIGGER_OBSTACLE", obs));
    }

    this.TriggerWaypoint = function (wp) {
        websocket.send(this.GetMessage("TRIGGER_WAYPOINT", wp));
    }

    this.TriggerPredefinedPath = function (path) {
        websocket.send(this.GetMessage("TRIGGER_PREDEFINED_PATH", path));
    }

    this.SetTraffic = function (traf) {
        websocket.send(this.GetMessage("SET_TRAFFIC", traf));
    }

    this.SetAutoPlay = function (auto) {
        websocket.send(this.GetMessage("SET_AUTOPLAY", auto));
    }

    this.SetMusicVolume = function (vol) {
        websocket.send(this.GetMessage("SET_MUSIC_VOL", vol));
    }

    this.SetFoleyVolume = function (vol) {
        websocket.send(this.GetMessage("SET_FOLEY_VOL", vol));
    }

    this.SetVehicleVolume = function (vol) {
        websocket.send(this.GetMessage("SET_VEHICLE_VOL", vol));
    }

    this.SetProgressTime = function (time) {
        websocket.send(this.GetMessage("SET_PROGRESS_TIME", time));
    }

    this.SetTimeOfDay = function (time) {
        websocket.send(this.GetMessage("SET_TIME_OF_DAY", time));
    }

    this.SetWeather = function (weather) {
        websocket.send(this.GetMessage("SET_WEATHER", weather));
    }

    this.SetWetRoads = function (time) {
        websocket.send(this.GetMessage("SET_WET_ROADS", time));
    }

    this.SetSunShafts = function (time) {
        websocket.send(this.GetMessage("SET_SUN_SHAFTS", time));
    }

    this.SetFoV = function (time) {
        websocket.send(this.GetMessage("SET_CAM_FOV", time));
    }

    this.SetCamFarClip = function (time) {
        websocket.send(this.GetMessage("SET_CAM_FARCLIP", time));
    }

    this.ConfirmEndScene = function () {
        websocket.send(this.GetMessage("END_DRIVE_SCENE", ""));
    }

    this.CancelEndScene = function () {
        $location.path('/drive_admin');
    }

    this.SelectInfraction = function (id) {
        websocket.send(this.GetMessage("SELECT_INFRACTION", id));
    }

    this.ClearSelectedInfraction = function () {
        websocket.send(this.GetMessage("CLEAR_INFRACTION", ""));
    }

    var newFarClip = -1;
    var newFoV = -1;
    var newTime = -1;

    this.SetTimeOfDayLimited = function (tod) {
        newTime = tod;
    }

    this.SetCamFarClipLimited = function (farClip) {
        newFarClip = farClip;
    }

    this.SetFoVLimited = function (fov) {
        newFoV = fov;
    }

    this.RepositionVehicle = function () {
        websocket.send(this.GetMessage("REPOSITION_VEHICLE", ""));
    }

    var controller = this;

    $interval(function () {
        if (newTime >= 0) {
            controller.SetTimeOfDay(newTime);
            newTime = -1;
        }

        if(newFarClip >= 0)
        {
            controller.SetCamFarClip(newFarClip);
            newFarClip = 0;
        }

        if(newFoV >= 0)
        {
            controller.SetFoV(newFoV);
            newFoV = 0;
        }

    }, 500);

    $rootScope.RepositionVehicle = function () {
        controller.RepositionVehicle();
    }


    this.GetMessage = function(msg, data) {
        return JSON.stringify({
            messageType: msg,
            data: data
        });
    }

    this.GetScreenShotUrl = function (id) {
        return "http://" + $location.host() + ":8087/Screenshots/" + id;
    }



    var url = "ws://" + $location.host() + ":8088/api";
    var websocket = new WebSocket(url);

    websocket.onopen = function (evt) {
        $rootScope.$broadcast('onopen', evt);
    };

    websocket.onclose = function (evt) {
        $rootScope.$broadcast('onclose', evt);
    };

    websocket.onmessage = function (evt) {
        console.log("onmsg " + evt.data);
        var message = angular.fromJson(evt.data);
        console.log('got message: ' + message.messageType);

        $rootScope.$broadcast(message.messageType, message.messageArgs);
    };

    websocket.onerror = function (evt) {
        console.log('Websocket error: ' + evt.error);
    };
}


// Declare app level module which depends on views, and components
angular.module('myApp', [
  'ngRoute',
  'myApp.view1',
  'myApp.view2',
  'myApp.Drive',
  'myApp.Infractions',
  'ui.slider'
]).
config(['$routeProvider', function ($routeProvider) {
    $routeProvider.otherwise({ redirectTo: '/select_select' });
}]).
service('networkController', ['$rootScope', '$location', '$interval', NetworkController]).
controller('RootController', ['$rootScope', '$route', 'networkController', function ($rootScope, $route, networkController) {

    $rootScope.drive = false;
    $rootScope.$route = $route;
    $rootScope.$on('TestMsg', function (event, args) {
        console.log("got test message");
    });

    $rootScope.$on('DRIVE_SCENE', function (event, args) {
        console.log("trigger drive scene");
        networkController.TriggerDriveScene();
        console.log("Drive: " + $rootScope.drive);
        $rootScope.$apply();
    });

    $rootScope.$on('END_DRIVE_SCENE', function (event, args) {

        $rootScope.infractions = args;

        networkController.TriggerSelectScene();
        $rootScope.$apply();
    });

    $rootScope.$on('START_DRIVE_SCENE', function (event, args) {
        networkController.TriggerDriveScene();

        $rootScope.enableTraffic = false;
        $rootScope.enableAutoPlay = false;
        $rootScope.currentWeather = 'clear';
        $rootScope.wetRoads = false;
        $rootScope.sunShafts = true;
        $rootScope.progressTime = true;
        $rootScope.timeOfDay = 12;

        $rootScope.$apply();
    });

    $rootScope.$on('REFRESH_STATE', function (event, args) {
        $rootScope.enableTraffic = args.enableTraffic === "True" ? true : false;
        $rootScope.enableAutoPlay = args.enableAutoPlay === "True" ? true : false;

        $rootScope.progressTime = args.progressTime === "True" ? true : false;;
        $rootScope.currentWeather = args.currentWeather;
        $rootScope.sunShafts = args.sunShafts === "True" ? true : false;;
        $rootScope.wetRoads = args.wetRoads === "True" ? true : false;;
        $rootScope.camFarClip = args.camFarClip;
        $rootScope.fov = args.camFoV;
        $rootScope.musicVolume = args.musicVol;
        $rootScope.foleyVolume = args.foleyVol;
        $rootScope.vehicleVolume = args.vehicleVol;
        $rootScope.mapXPos = args.mapXPos;
        $rootScope.mapZPos = args.mapZPos;

        if(args.settingTimeOfDay !== "True") {
            $rootScope.timeOfDay = args.timeOfDay;
            $rootScope.$broadcast('SET_TIME_OF_DAY');
        }

        $rootScope.$apply();

    });

    //Testing only, TODO: remove
    $rootScope.$on('AppState', function (event, args) {
        if (args.currentState == 'DRIVE') {
            networkController.TriggerDriveScene();
            $rootScope.currentScene = args.currentScene;
        } else if (args.currentState == 'SELECT') {
            networkController.TriggerSelectScene();
        }
    });

}]).
run(['networkController', function (networkController) {
    networkController.TriggerSelectScene();
}]);

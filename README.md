# Vehicle Simulator

<!-- <p align="center">
    <img src="https://vendor2.nginfotpdx.net/gitlab/free-ride/free-ride/blob/master/free-ride.gif" alt="free-ride">
</p> -->

## Overview
The project and the initial software code have been developed by Elements Design Group of San Francisco and the Jaguar Land Rover Open Software Technology Center in Portland, Oregon. The motivation was to provide an open-source, extensible driving simulator project for the development community. While there are multiple potential uses for the application, the primary goal was to create an application to assist in the development and testing of IVI systems related to driver distraction.

The project was created with Unity 5.3.4 and runs on Windows 10 (64 bit). To download the Unity IDE, please click here. Unity provides the ability to generate builds for Windows, Linux, and Mac OS X.

A more detailed overview can be found in the [wiki home page](https://vendor2.nginfotpdx.net/gitlab/free-ride/free-ride/wikis/home).

## License Information
This application is licensed under the [MPL License](https://www.mozilla.org/media/MPL/2.0/index.815ca599c9df.txt).

## How-To
*To learn how to operate the Vehicle Simulator at OSTC, visit this [tutorial page](https://vendor2.nginfotpdx.net/gitlab/free-ride/free-ride/wikis/how-to-run-vehicle-simulator-ostc-pdx)*.

## General Information
### Scenes
1. Application Load
2. Vehicle & Driving Scene Selection
3. Scenic Driving Scene (Yosemite)
4. Urban Driving Scene (San Francisco)
5. Coastal Driving Scene (Pacific Coast Highway)

### Vehicles
1. Jaguar XJ
2. Land Rover Range Rover L405
3. Jaguar F-Type (coming soon)
4. Jaguar F-Type Coupe (coming soon)
5. Land Rover Range Rover Evoque (coming soon)
6. Land Rover LR4 (coming soon)

### OBSTACLES
Obstacles may be triggered (by the admin) while driving. If the driver hits an obstacle, the event is logged as an infraction which can be reviewed after the driving session. Current obstacle types are:
- pedestrians
- animals
- rockfall
- large boulder
- stalled vehicles
- falling tree
- giant beach balls

### INFRACTION LOGGING
The infraction log is reviewable in the administration panel after each driving session and is also saved as xml file for various uses. The following infractions are currently logged by the system:
- Traffic violations (e.g. running a stop sign)
- Lane infractions (e.g. driving over double yellow lines)
- Environment collisions (e.g. colliding with a tree)
- Obstacle collisions (e.g. colliding with a bear)

### INFRACTION REVIEW
At the end of a driving session, the admin and driver can review infractions from the most recent session. Screenshots of the infraction along with vehicle data (speed, etc.) are displayed. Session data is timestamped and saved out as XML or CSV [to compare with In-Vehicle Infotainment (IVI) UI events.] ![IMG_0042](https://vendor2.nginfotpdx.net/gitlab/free-ride/free-ride/uploads/52696492e364090a232b5378eb23921b/Infraction_Log.png)

### Administration Interface
1. Infraction Review
2. Obstacle Instantiation
3. Vehicle Physics Calibration
4. Vehicle Selection
5. Driving Scene Selection
6. Repositioning Vehicle on Road
7. Waypoint Mapping and Positioning


## System Requirements
### Hardware
##### Simulator PC
- CPU – 5th Generation Core i7 GPU
- GPU - Single GTX 980 Ti, GTX Titan, or Quadro K6000 
- RAM – 12 GB 
- Motherboard - ASUS X99 E WS
- Operating System - Windows 10
- Networking - 10/100/1000 Ethernet adapter

##### Instrument Cluster PC
- Operating System - Windows 10
- Networking - 10/100/1000 Ethernet adapter

> See [this page](https://vendor2.nginfotpdx.net/gitlab/free-ride/free-ride/wikis/hardware-overview) for additional hardware information.

##### Displays
*Simulator Application*
The Simulator application shall support the following display configurations and resolutions:

- Single: 1920px X 1080px
- Double: 3840px X 1080px
- Triple: 5760px X 1080px
- Triple: (projection, stitched image) 4992px X 1080px

*Instrument Cluster Application*
The Instrument Cluster application supports a single display with a 1920px X 720px resolution.

JLR recognizes the bespoke nature of this resolution. Consequently, the IC application shall support displaying the IC at the same resolution on a single display (up to 1920 X 1080), letter-boxing the content when necessary.

### Software
##### Operating System
Windows 10 is the operating system for both PC's.

##### Integrated Development Environment
This simulator was developed using Unity 5.3.4.
System Requirements

Visual C++ Redistributable for Visual Studio 2015 - https://www.microsoft.com/en-us/download/details.aspx?id=48145

Powershell scripts notes for JLR sim room

PS Scripts are located inside repos/project_freeride directory

There are 4 powershell scripts which speed up the build/deployment process for the app. 
BuildDeployAndRun.ps
    This script builds, deploys and runs both applications on the Main machine as well as the IC.

BuildAndDeploy.ps
    This script builds and deploys both applications on main machine and IC
    
Run.ps
    Runs both applications (main app & IC)
Kill.ps
    Stops applications on main machine and IC
    
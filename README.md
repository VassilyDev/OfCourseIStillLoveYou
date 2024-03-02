# OfCourseIStillLoveYou

Forked from https://github.com/jrodrigv/OfCourseIStillLoveYou
Provides adaptive layout up to six cameras. Check the main mod repository for mod installation and requirements.

<img src="https://raw.githubusercontent.com/VassilyDev/OfCourseIStillLoveYou/main/screenshot.PNG" alt="Screenshot"/>

## Requirements:
* KSP 1.12.5
* NET 7 runtime https://dotnet.microsoft.com/en-us/download/dotnet/7.0
* Latest HullcamVDS https://github.com/linuxgurugamer/HullcamVDSContinued/releases

## Highly recommended mods:
* Physics Range Extender
* Scatterer 0.0838 or newer https://github.com/LGhassen/Scatterer/releases
* If you want to use TUFX you need to use this version -> TUFX JR edition https://github.com/jrodrigv/TUFX/releases 

## Mod Installation:
* Download the zip file for Windows, Linux or Mac.
* Copy the GameData folder into your KSP root folder

## Mod Configuration:
Inside the settings.cfg file you can modify the Cameras resolution and server connection

```Settings
{
  EndPoint = localhost
  Port = 5077
  Width = 768
  Height = 768
}
```
## Desktop & Server app setup:
* Unzip the OfCourseIStillLoveYou.Server.zip and OfCourseIStillLoveYou.DesktopClient.zip
* By default the mod, the server and the desktop client will connect to localhost:5077 but you can modify it:
  * Server: *OfCourseIStillLoveYou.Server.exe --endpoint 192.168.1.8  --port 5001* .
  * DesktopClient: Open the settings.json inside OfCourseIStillLoveYou.DesktopClient and modify the endpoint and port.
  * Mod: Inside the mod folder there is a settings.cfg file with the endpoint and port.
* Execute the OfCourseIStillLoveYou.Server.exe first, then OfCourseIStillLoveYou.DesktopClient.exe and finally start KSP

## Running the server as a Docker Container
* Pull the image *docker pull jrodrigv/ofcourseistillloveyou:latest*
* Create a new container - example overriding endpoint to listen everything and from port 5000: *docker run -d -p 192.168.0.14:5000:5000 ofcourseistillloveyou:server_v1.0 --port 5000 --endpoint 0.0.0.0*

## Desktop Client usage
* To hide all the UI controls & telemetry, double click on the camera image view
* To move the window, click and drag from the camera image view
* To resise the window, click and drag from the resize texture corner (bottom - right corner)
* To close the UI, press the "X" button

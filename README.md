# Mars Trek VR Application
## Table of Contents
* [Requirements](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#requirements)
  * [Hardware](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#hardware)
  * [Software](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#software)
* [Development Setup](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#development-setup)
  * [Prerequisites](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#prerequisites)
  * [Repository Setup](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#repository-setup)
  * [Running the Unity Project](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#running-the-unity-project)
* [Controls](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#controls)
  * [HTC Vive](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#htc-vive)


## Requirements
### Hardware
#### VR Headset
* HTC Vive (Vive Pro should also work, but has not yet been tested)
* Occulus Rift is currently unsupported.
#### PC Requirements
* [Check here](https://store.steampowered.com/app/323910/SteamVR_Performance_Test/)
### Software
* Steam and SteamVR
* Windows operating system, other operating systems may yield unpredictable results.

## Development Setup
This section contains instructions on how to clone and setup the project for development, or to just run the application without building/compiling. Each section should be completed in order of apperance.
### Prerequisites
The following hardware/software is required:
* All of the requirments as listed in the [requirements](https://github.com/alvinquach/jpl-trek-vr/blob/master/README.md#requirements) section.
* [Unity Editor](https://unity3d.com/get-unity/download/archive) (currently being developed on 2018.2.6f1)
* Git and [Git LFS](https://git-lfs.github.com/)
### Repository Setup
1. Clone this repository.

   Example:
   ```
   git clone https://github.com/alvinquach/jpl-trek-vr.git
   ```

2. Init the submodules. This repository contains one submodule: [jpl-trek-vr-ext-assets](https://github.com/alvinquach/jpl-trek-vr-ext-assets)

   Example:
   ```
   cd jpl-trek-vr
   git submodule init
   git submodule update
   ```
   
   This make take a couple of minutes, since the textures submodule contains some relatively large files.
### Running the Unity Project
1. Start the Unity Editor
2. In the Open Project dialog box, click Open. Then, navigate to the project folder (`jpl-trek-vr` if you followed the example commands in the previous section) and click Select Folder.
3. Give Unity some time to automatically build some files.
4. Click on the play button on top to run the application.

## Controls
### HTC Vive
Controls specific to the HTC Vive headset.
#### Controller
* Currently only one controller can be used at a time.
* The touchpad allows you to move forwards and backwards relative to where the controller is pointed. Click the upper half of the touchpad to move in the direction that the controller is pointed, and click the bottom half of the touchpad to move away from the direcction that the controller is pointed.

   For example, to move foward, point the controller towards your front and click the top portion of the touchpad. Click the bottom half of the touchpad to move backwards instead.
   
   To move sideways, point the controller to the side, and then click the touchpad.

* The grip button(s) allow you to grab the planet and spin it around.

* The menu button opens up a VR menu containing a list of bookmarks. Click on the menu button again to close the menu.

  Clicking on a bookmark using the trigger button will rotate the planet such that the bookmarked area is facing you.
  
  When the menu is open, the bookedmarked areas will also appear on the planet with a pin and label.
  
* Clicking the trigger button when the controller is pointed to the planet will rotate the planet such that the area that is being pointed at is facing you.

## Thrid-Party Credits
This projects uses assets libraries from third party sources.
### Assets Store
Thrid party assets from the Unity Asset Store are located in an external repository ([jpl-trek-vr-ext-assets](https://github.com/alvinquach/jpl-trek-vr-ext-assets)). The list of assets can be found in the README of that repository.
### Thrid Party DLLs
Third party .NET DLL files can be found in the `Assets/Plugins` folder of this repository. The list of thrid party DLLs can be found [here](https://github.com/alvinquach/jpl-trek-vr/new/master/Assets/Plugins/README.md).
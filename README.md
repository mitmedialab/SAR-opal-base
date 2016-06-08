# SAR-opal-base 

Opal is a generalized Unity game builder designed for use in child-robot interactions. Easily load different graphics for games requiring similar mechanics, all using ROS.

## Build and Run
This game was built and tested with Unity 5.3.5 and MonoDevelop 5.9.6.

## Configuration

The gaem uses configuration options listed in the websocket\_config file. There
is an example file located in Assets/Resources/.

### On OS X

When running the game on OS X from the Unity editor, the game will check for
"Assets/Resources/websocket\_config.txt".

### On Android

When running the game on Android (e.g., on a tablet), the game will first check
for "mnt/sdcard/edu.mit.media.prg.sar.opal.base/websocket\_config.txt". This is
    because once the game is packaged up for Android, you can't change stuff in
    the package, so we needed a location for the config file that could be
    easily edited after the game is installed. If that file doesn't exist, or
    connecting fails with the values listed in that file, the game will try the
    file packaged in "Assets/Resources/websocket\_config.txt". 

### Configuration options 

- server: [string] the IP address or hostname of the ROS server
- port: [string] port number to use
- toucan: [boolean] whether or not you want a toucan sidekick in the game

#### Server & port On startup, the game will try to connect to the specified IP

address or host name with the specified port. The server listed should be the
IP address or hostname of the machine running roscore and the
rosbridge\_server.

You can start the rosbridge\_server with the command `roslaunch rosbridge\_server rosbridge\_websocket.launch`.

If the specified server address does not exist on the network, there is a 90s
timeout before it'll give up trying (hardcoded in the websocket library, so one
could patch the library to change the timeout length if one so desires). This
will manifest as the application hanging, unresponsive, for the duration of the
timeout.

If the server address does exist but if you've forgotten to start rosbridge\_server, the connection will be refused.

#### Toucan

If you set the toucan option to 'true', a toucan sidekick character will be
loaded into the game (provided you have the toucan graphics in your Assets
folder). You will then be able to send the sidekick commands, such as to play
back sound or animations. If set to false, no toucan will be present.

## SAR Opal messages

The game subscribes to the ROS topic "opal\_tablet\_command" to receive messages of type "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalCommand".

The game publishes /std\_msgs/String messages to the ROS topic "opal\_tablet".

The game publishes "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalAction" to the ROS topic "opal\_tablet\_action". See [/sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs") for more info.

The game publishes "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalScene" to the ROS topic "opal\_tablet\_scene". Usually this message will only be published after receiving a "request keyframe" command - see [/sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs") for more info. 

The game publishes "/std\_msgs/Bool" to the ROS topic "opal\_tablet\_audio", to
indicate whether the sidekick character is done playing back an audio file.

## Submodules

You don't need to pull in these submodules for the main project to run (the
necessary scripts or dlls have been copied into the Assets/Plugins folder), but
if you want their source, extra examples, prefabs, etc, then you can.

### TouchScript

[TouchScript] (https://github.com/TouchScript/TouchScript "TouchScript") makes
it easy to detect and respond to touch events, such as taps and drags. See the wiki [here] (https://github.com/TouchScript/TouchScript/wiki "TouchScript wiki") for more information. 

We built a unitypackage from the TouchScript 8.1 source, which has been
imported into the game in the Assets folder.

Instructions on building TouchScript's unitypackage from source are online
[here] (https://github.com/TouchScript/TouchScript/wiki/How-to-Contribute "How
to Contribute"). For 8.1, the steps are:

- init and update TouchScript's git submodules
- init and update any submodules of those submodules
- run `Build/build_external.sh`
- run `Build/package.sh`
- import the generated TouchScript.unitypackage file in the Unity editor 

Note that the MainCamera and the Moveables Camera in the Unity scene each need
a CameraLayer2D component attached. The camera layer is used to "see" which
objects in the scene can be touched - see [Layers]
(https://github.com/TouchScript/TouchScript/wiki/Layers "TouchScript Layers").
If you don't have a camera layer of some kind attached to the MainCamera,
TouchScript will automatically add one, but the default is a CameraLayer that
handles 3D objects and 3D colliders. Since Opal is a 2D game, we need to use
the CameraLayer2D, which is for 2D objects and 2D colliders.  (Emphasizing this
extra because it can cause needless headache.)

The TouchScript game object should have a Touch Manager component attached,
which will list the different camera layers in the scene.

### LeanTween
[LeanTween] (https://github.com/dentedpixel/LeanTween/ "LeanTween git") is a library for animating sprites ([docs here] (http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html "LeanTween docs")).

If you pull in the submodule, you can get the examples, prefabs, etc. The necessary .cs file is in the SAR-opal-base Assets/Plugins folder already.

### websocket-sharp

[Websocket-sharp] (https://github.com/sta/websocket-sharp "websocket-sharp
git") is a .Net implementation of websockets, and is used to communicate with
the ROS rosbridge\_server.

Note that if you try to build this project, the Newtonsoft.Json dll appears to
be missing, so I copied over the prebuilt dll from bin/Debug.

### MiniJSON

[MiniJSON] (https://gist.github.com/darktable/1411710 "miniJSON") is a pretty
basic C# JSON encoder and decoder. It can serialize and deserialize JSON
strings, which are sent to and received from the ROS rosbridge\_server. I've
used the code mostly as-is, with one or two of the bug fixes listed in the
comments on the github gist page added in.


## Miscellaneous Notes

- When adding new images to the project, make sure to set each image as
  'Advanced' and check the 'read/write' box in the Unity editor. If you don't
  do this, when images are programatically loaded as PlayObjects, the polygon
      colliders won't be generated to properly fit the image's shape/outline.
      When deploying the game, you'll probably get the error "Sprite outline
      generation failed - could not read texture pixel data. Did you forget to
      make the texture readable?" whenever you dynamically add a polygon
      collider to an object. Something about textures/images not being readable
      by scripts by default, the polygon collider needing to read the texture
      to figure out the outline to make the collider the right shape, but not
      being able to, and thus the collider ending up the wrong shape and making
      collisions happen weird... 

- Only a small set of "demo" graphics are included in this repository. The full
  set is available from the Personal Robots Group - email students in the group
  to inquire. Add the full set to the "Resources\/base\_images" folder. 

## Version Notes

The Year 3 SAR study was run using Opal version 1.0.3.

The Cyber4 study was run using Opal version 2.0.0.

## Demo Version

To build and deploy the demo version, do the following:
1. In Unity > Build Settings > Scenes in build, check all the demo scenes and uncheck basic-scene and all other non-demo scenes.
2. In Unity > Build Settings > Player Settings, change the deployed name of the game to be "SAR Opal Demo" and the bundle identifier to be "demo" instead of "base".
3. In the MainGameController, set the flag "demo" to true.
4. Build and deploy. 

The demo version of the game requires some graphics that are not included in the demo. 

## Storybook version

To build and deploy the Frog Where Are You storybook, do the following:
1. Get the Frog Where Are You graphics and put them in the "Resources/graphics/base-images/frogwhereareyou/" directory.
1. In Unity > Build Settings > Scenes in build, check the frog-where-are-you scene and uncheck the other scenes.
2. In Unity > Build Settings > Player Settings, change the deployed name of the game to be "Frog Where Are You" and the bundle identifier to be "fway" instead of "base".
3. In the MainGameController, set the flag "story" to true.
4. Build and deploy.

You can use the Frog Where Are You book as an example for how to load your own set of images for a storybook.

## Bugs and known issues

Games made with Unity 5 cannot be deployed to non-neon devices (i.e., Android
tablets that have tegra boards, such as many of the older Samsung Galaxy
tablets), because Unity 5 no longer supports these devices. Thus, Opal cannot
be deployed to these devices.

## TODO

- 'next page' appears to not work when touch disabled
- Add filepath to SceneObjectProperties so objects can be loaded from outside build directory
- LoadSpriteFromFile does not fail when a non-image file, such as a text file, is loaded, but should fail
- Add guidelines for where to put images on tablet for loading images from outside build directory and test with tablet
- Load sounds from a folder outside build directory
- Create demo version of the game that only uses graphics that are checked in to the repository.
- Move 'highlight' object with transformer2D, currently does not follow drag path very well
- Log all log messages locally to tablet
- Objects can leave the viewable screen on drag, changes margins (this is because we used TouchScript's Transformer2D for drag, which doesn't have the margins for where not to go)
- Consider using SimplePan to get the position to log, not Pan
- Add some way of easily seeing which objects in the scene are draggable or able to be interacted with; used to do the grow-shrink pulse motion, but that caused havoc with the collision detection
- Right now, the sidekick configuration is a simple true/false; you can't start out without a sidekick and add it in later. Consider adding a sar\_opal\_msg that enables or disables the sidekick, so that it can appear or disappear as needed.
- If you try to send a message with sar\_opal\_sender to move an object but send a json file that doesn't have the right stuff in it for a move command, throws error, need to fix. More generally: we don't check that the command number matches the arguments in the json file. Should do that during message decoding.
- Update to load images and audio from folders on the tablet's sdcard (currently, must load from the Resources folder, which gets compiled in to the Unity game)
- Add capability to load an image on the "top left" or "bottom right" of the screen without specifying exact coordinates. Adjust the loaded image's position until no collisions are detected so it does not overlap with other images.
- Adapt to different screen sizes (currently size of images in most games is hard-coded, not scaling - social stories game is the only one that scales properly)
- Look into ROS .NET for C#. Possible replacement for websocket connection?
- Add demo and story options to config file so you don't have to recompile and redeploy (and/or a start screen that lets you pick whether you want demo mode, story mode, etc when you start the game - use "load scene" function to pick the right one).


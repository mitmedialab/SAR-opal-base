# SAR-opal-base 

A generalized Unity game template designed for use in a child-robot interaction
that helps the child learn a second language.

## Build and Run
This tablet app was built and tested with Unity 5.0.0 and MonoDevelop 4.0.1.

## Configuration

The app uses configuration options listed in the websocket\_config file. There
is an example file located in Assets/Resources/.

### On OS X

When running the app on OS X from the Unity editor, the app will check for
"Assets/Resources/websocket\_config.txt".

### On Android

When running the game on Android (e.g., on a tablet), the app will first check
for "mnt/sdcard/edu.mit.media.prg.sar.opal.base/websocket\_config.txt". This is
    because once the app is packaged up for Android, you can't change stuff in
    the package, so we needed a location for the config file that could be
    easily edited after the app is installed. If that file doesn't exist, or
    connecting fails with the values listed in that file, the app will try the
    file packaged in "Assets/Resources/websocket\_config.txt". 

### Configuration options 

- server: [string] the IP address or hostname of the ROS server
- port: [string] port number to use
- toucan: [boolean] whether or not you want a toucan sidekick in the game

#### Server & port On startup, the app will try to connect to the specified IP

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
it easy to detect and respond to touch events, such as taps and drags. You can
build it from source following the instructions [here]
(https://github.com/TouchScript/TouchScript/wiki/Building-TouchScript "Building
TouchScript").

If you build from source, you can copy
TouchScript/UnityPackages/TouchScript.Android to SAR-opal-base/Assets/ to
access everything (like prefabs and examples) from within the Unity editor.
That said, the only really important thing is the TouchScript dll in the
Plugins folder (which is in the SAR-opal-base Assets/Plugins folder already).

Note that the MainCamera in the Unity scene needs a CameraLayer2D component
attached. The camera layer is used to "see" which objects in the scene can be
touched - see [Layers] (https://github.com/TouchScript/TouchScript/wiki/Layers
"TouchScript Layers"). If you don't have a camera layer of some kind attached
to the MainCamera, TouchScript will automatically add one, but the default is a
CameraLayer that handles 3D objects and 3D colliders. Since Opal is a 2D game,
we need to use the CameraLayer2D, which is for 2D objects and 2D colliders.
(Emphasizing this extra because it can cause needless headache.)

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

- If you are using Unity 4.6.2: When adding new audio to the project, make sure each audio clip is set as a 2D sound. This will ensure it plays without the default volume rolloff that 3D sounds have (and thus, is audible when played). Note that the Force2DAudio script in Assets/Editor will automatically set any audio files that Unity imports from the Assets folder as 2D sounds, so you probably won't have to worry about this. (This isn't an issue in Unity 5.0.0.)

- When adding new images to the project, make sure to set each image as 'Advanced' and check the 'read/write' box in the Unity editor. If you don't do this, when images are programatically loaded as PlayObjects, the polygon colliders won't be generated to properly fit the image's shape/outline. When deploying the app, you'll probably get the error "Sprite outline generation failed - could not read texture pixel data. Did you forget to make the texture readable?" whenever you dynamically add a polygon collider to an object. Something about textures/images not being readable by scripts by default, the polygon collider needing to read the texture to figure out the outline to make the collider the right shape, but not being able to, and thus the collider ending up the wrong shape and making collisions happen weird... 

- Only a small set of "demo" graphics are included in this repository. The full set is available from the Personal Robots Group - email students in the group to inquire. Add the full set to the "Resources\/base\_images" folder. 

## Version Notes

The Year 3 SAR study was run using Opal version 1.0.3.

The Cyber4 study was run using Opal version 2.0.0.

## Demo Version

To build and deploy the demo version, do the following:
1. In Unity > Build Settings > Scenes in build, check all the demo scenes and uncheck basic-scene and all other non-demo scenes.
2. In Unity > Build Settings > Player Settings, change the deployed name of the app to be "SAR Opal Demo" and the bundle identifier to be "demo" instead of "base".
3. In the MainGameController, set the flag "demo" to true.
4. Build and deploy. 

The demo version of the game requires some graphics that are not included in the demo. 

## Storybook version

To build and deploy the Frog Where Are You storybook, do the following:
1. Get the Frog Where Are You graphics and put them in the "Resources/graphics/base-images/frogwhereareyou/" directory.
1. In Unity > Build Settings > Scenes in build, check the frog-where-are-you scene and uncheck the other scenes.
2. In Unity > Build Settings > Player Settings, change the deployed name of the app to be "Frog Where Are You" and the bundle identifier to be "fway" instead of "base".
3. In the MainGameController, set the flag "story" to true.
4. Build and deploy.

You can use the Frog Where Are You book as an example for how to load your own set of images for a storybook.

## Bugs and problems

Apps made with Unity 5 cannot be deployed to Android tablets that have tegra
boards (such as Samsung Galaxy tablets), because Unity 5 no longer supports
these tablets. Thus, Opal cannot be deployed to tablets with tegra boards. 

## TODO

- Create demo version of the game that only uses graphics that are checked in to the repository.
- Move 'highlight' object with transformer2D, currently does not follow drag path very well
- Log all log messages locally to tablet
- Objects can leave the viewable screen on drag, changes margins (this is because we used TouchScript's Transformer2D for drag, which doesn't have the margins for where not to go)
- Consider using SimplePan to get the position to log, not Pan
- Add some way of easily seeing which objects in the scene are draggable or able to be interacted with; used to do the grow-shrink pulse motion, but that caused havoc with the collision detection
- Right now, the sidekick configuration is a simple true/false; you can't start out without a sidekick and add it in later. Consider adding a sar\_opal\_msg that enables or disables the sidekick, so that it can appear or disappear as needed.
- If you try to send a message with sar\_opal\_sender to move an object but send a json file that doesn't have the right stuff in it for a move command, throws error, need to fix. More generally: we don't check that the command number matches the arguments in the json file. Should do that during message decoding.
- Figure out how to put all graphics and sounds in a folder outside of the compiled app \(right now they are in the 'Resources' folder so Unity knows where they are more easily\), so that we can add new graphics and sounds more easily. 
- Update to load images and audio from folders on the tablet's sdcard (currently, must load from the Resources folder, which gets compiled in to the Unity app)
- Add capability to load an image on the "top left" or "bottom right" of the screen without specifying exact coordinates. Adjust the loaded image's position until no collisions are detected so it does not overlap with other images.
- Adapt to different tablet screen sizes (currently size of images is hard-coded, not scaling)
- Look into ROS .NET for C#. Possible replacement for websocket connection?
- Add demo and story options to config file so you don't have to recompile and redeploy


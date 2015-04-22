# SAR-opal-base
A generalized Unity game template designed for use in a child-robot interaction that helps the child learn a second language.

## Build and Run
This tablet app was built and tested with Unity 4.6.2 and MonoDevelop 4.0.1.

## Configuration
On startup, the tablet app will try to connect to the IP address or host name specified in the websocket\_config file. There is an example file located in Assets/Resources/. This IP address should be the address of the machine running roscore and the rosbride\_server (start the server with the command "roslaunch rosbridge\_server rosbridge\_websocket.launch").

When running the app on OS X from the Unity editor, the app will check for "Assets/Resources/websocket\_config.txt".

When running the game on Android (e.g., on a tablet), the app will first check for "mnt/sdcard/edu.mit.media.prg.sar.opal.base/websocket\_config.txt". This is because once the app is packaged up for Android, you can't change stuff in the package, so we needed a location for the websocket config file that could be easily edited after the app is installed. If that file doesn't exist, or connecting fails with the values listed in that file, the app will try the file packaged in "Assets/Resources/websocket\_config.txt". 

Note that if the specified address does not exist on the network, there is a 90s timeout before it'll give up trying (hardcoded in the library, so one could patch the library to change the timeout length if one so desires). This will manifest as the application hanging, unresponsive, for the duration of the timeout.

If the address does exist but if you've forgotten to start rosbridge\_server, the connection will be refused.

## SAR Opal messages
The game subscribes to the ROS topic "opal\_tablet\_command" to receive messages of type "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalCommand".

The game publishes /std\_msgs/String messages to the ROS topic "opal\_tablet".

The game publishes "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalAction" to the ROS topic "opal\_tablet\_action". See [/sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs") for more info.

The game publishes "/[sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs")/OpalScene" to the ROS topic "opal\_tablet\_scene". Usually this message will only be published after receiving a "request keyframe" command - see [/sar\_opal\_msgs] (https://github.com/personal-robots/sar_opal_msgs "/sar\_opal\_msgs") for more info. 

The game publishes "/std\_msgs/Bool" to the ROS topic "opal\_tablet\_audio", to indicate whether the sidekick character is done playing back an audio file.

## Submodules
You don't need to pull in these submodules for the main project to run (the necessary scripts or dlls have been copied into the Assets/Plugins folder), but if you want their source, extra examples, prefabs, etc, then you can.

### TouchScript
[TouchScript] (https://github.com/TouchScript/TouchScript "TouchScript") makes it easy to detect and respond to touch events, such as taps and drags. You can build it from source following the instructions [here] (https://github.com/TouchScript/TouchScript/wiki/Building-TouchScript "Building TouchScript").

If you build from source, you can copy TouchScript/UnityPackages/TouchScript.Android to SAR-opal-base/Assets/ to access everything (like prefabs and examples) from within the Unity editor. That said, the only really important thing is the TouchScript dll in the Plugins folder (which is in the SAR-opal-base Assets/Plugins folder already).

Note that the MainCamera in the Unity scene needs a CameraLayer2D component attached. The camera layer is used to "see" which objects in the scene can be touched - see [Layers] (https://github.com/TouchScript/TouchScript/wiki/Layers "TouchScript Layers"). If you don't have a camera layer of some kind attached to the MainCamera, TouchScript will automatically add one, but the default is a CameraLayer that handles 3D objects and 3D colliders. Since Opal is a 2D game, we need to use the CameraLayer2D, which is for 2D objects and 2D colliders. (Emphasizing this extra because it can cause needless headache.)

### LeanTween
[LeanTween] (https://github.com/dentedpixel/LeanTween/ "LeanTween git") is a library for animating sprites ([docs here] (http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html "LeanTween docs")).

If you pull in the submodule, you can get the examples, prefabs, etc. The necessary .cs file is in the SAR-opal-base Assets/Plugins folder already.

### websocket-sharp
[Websocket-sharp] (https://github.com/sta/websocket-sharp "websocket-sharp git") is a .Net implementation of websockets, and is used to communicate with the ROS rosbridge\_server.

Note that if you try to build this project, the Newtonsoft.Json dll appears to be missing, so I copied over the prebuilt dll from bin/Debug.

### MiniJSON
[MiniJSON] (https://gist.github.com/darktable/1411710 "miniJSON") is a pretty basic C# JSON encoder and decoder. It can serialize and deserialize JSON strings, which are sent to and received from the ROS rosbridge\_server. I've used the code mostly as-is, with one or two of the bug fixes listed in the comments on the github gist page added in.


## Miscellaneous Notes
- When adding new audio to the project, make sure each audio clip is set as a 2D sound. This will ensure it plays without the default volume rolloff that 3D sounds have (and thus, is audible when played). Note that the Force2DAudio script in Assets/Editor will automatically set any audio files that Unity imports from the Assets folder as 2D sounds, so you probably won't have to worry about this.

## Demo Version
To build and deploy the demo version, do the following:
- In Unity > Build Settings > Scenes in build, check all the demo scenes and uncheck basic-scene and all other non-demo scenes.
- In Unity > Build Settings > Player Settings, change the deployed name of the app to be SAR Opal Demo and the bundle identifier to be "demo" instead of "base".
- In the MainGameController, set the flag "demo" to true.
- Build and deploy. 





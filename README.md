# SAR-opal-base
A generalized Unity game template designed for use in a child-robot interaction that helps the child learn a second language.


## Submodules
You don't need to pull in these submodules for the main project to run, but if you want their source, extra examples, prefabs, etc, then you can.

### TouchScript
[TouchScript] (https://github.com/TouchScript/TouchScript "TouchScript") makes it easy to detect and respond to touch events, such as taps and drags. You can build it from source following the instructions [here] (https://github.com/TouchScript/TouchScript/wiki/Building-TouchScript "Building TouchScript").

If you build from source, you can copy TouchScript/UnityPackages/TouchScript.Android to SAR-opal-base/Assets/ to access everything (like prefabs and examples) from within the Unity editor. That said, the only really important thing is the TouchScript dll in the Plugins folder (which is in the SAR-opal-base Assets/Plugins folder already).

Note that the MainCamera in the Unity scene needs a CameraLayer2D component attached. The camera layer is used to "see" which objects in the scene can be touched - see [Layers] (https://github.com/TouchScript/TouchScript/wiki/Layers "TouchScript Layers"). If you don't have a camera layer of some kind attached to the MainCamera, TouchScript will automatically add one, but the default is a CameraLayer that handles 3D objects and 3D colliders. Since Opal is a 2D game, we need to use the CameraLayer2D, which is for 2D objects and 2D colliders. (Emphasizing this extra because it can cause needless headache.)

### LeanTween
[LeanTween] (https://github.com/dentedpixel/LeanTween/tree/master "LeanTween git") is a library for animating sprites ([docs here] (http://dentedpixel.com/LeanTweenDocumentation/classes/LeanTween.html "LeanTween docs").

If you pull in the submodule, you can get the examples, prefabs, etc. The necessary .cs file is in the SAR-opal-base Assets/Plugins folder already.

## Miscellaneous Notes
- When adding new audio to the project, make sure each audio clip is set as a 2D sound. This will ensure it plays without the default volume rolloff that 3D sounds have (and thus, is audible when played). Note that the Force2DAudio script in Assets/Editor will automatically set any audio files that Unity imports from the Assets folder as 2D sounds, so you probably won't have to worry about this.




# SAR-opal-base
A generalized Unity game template designed for use in a child-robot interaction that helps the child learn a second language.


## TouchScript
[TouchScript] (https://github.com/TouchScript/TouchScript "TouchScript") makes it easy to detect and respond to touch events, such as taps and drags. You can build it from source following the instructions here: https://github.com/TouchScript/TouchScript/wiki/Building-TouchScript

If you build from source, copy TouchScript/UnityPackages/TouchScript.Android to SAR-opal-base/Assets/libs. That'll let you access everything (like prefabs and examples) from within the Unity editor, though the only really important thing is the TouchScript dll in the Plugins folder.

Note that the MainCamera in the Unity scene needs a CameraLayer2D component attached. The camera layer is used to "see" which objects in the scene can be touched - see [Layers] (https://github.com/TouchScript/TouchScript/wiki/Layers "TouchScript Layers"). If you don't have a camera layer of some kind attached to the MainCamera, TouchScript will automatically add one, but the default is a CameraLayer that handles 3D objects and 3D colliders. Since Opal is a 2D game, we need to use the CameraLayer2D, which is for 2D objects and 2D colliders. (Emphasizing this extra because it can cause needless headache.)




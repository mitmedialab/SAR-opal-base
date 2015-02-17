using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Constants 
{
    /** where images to load as sprites are located in Resources */
    public const string GRAPHICS_FILE_PATH = "graphics/base-images/";
    public const string AUDIO_FILE_PATH = "audio/";

    /** tag applied to all playobjects */
    public const string TAG_PLAY_OBJECT = "PlayObject";
    public const string TAG_LIGHT = "Light";
    public const string TAG_GESTURE_MAN = "GestureManager";

    /** messages we can receive */
    public const string DISABLE_TOUCH = "disable";
    public const string ENABLE_TOUCH = "enable";
    public const string RELOAD = "reload";
    public const string SIDEKICK_SAY = "sidekick-say";
    public const string SIDEKICK_DO = "sidekick-do";
    public const string LOAD_OBJECT = "load";
    public const string LOAD_BACKGROUND = "background";
    


}
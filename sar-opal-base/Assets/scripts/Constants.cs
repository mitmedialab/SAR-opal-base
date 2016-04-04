using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace opal
{
    // log message event -- fire when you want to log something
    // so others who do logging can listen for the messages
    public delegate void LogEventHandler(object sender,LogEvent logme);

    // object to be moved
    public struct MoveObject
    {
        public string name;
        public Vector3 destination;
    }

    // configuration
    public struct GameConfig
    {
        public string server;
        public string port;
        public bool sidekick;
    }

    public static class Constants
    {
        /** where images to load as sprites are located in Resources */
        public const string GRAPHICS_FILE_PATH = "graphics/base-images/";
        public const string AUDIO_FILE_PATH = "audio/";
        public const string FROG_FILE_PATH = "frogwhereareyou/";

        /// <summary>
        /// tag applied to all playobjects
        /// </summary>
        public const string TAG_PLAY_OBJECT = "PlayObject";
        public const string TAG_LIGHT = "Light";
        public const string TAG_GESTURE_MAN = "GestureManager";
        public const string TAG_BACKGROUND = "Background";
        public const string TAG_FOREGROUND = "Foreground";
        public const string TAG_SIDEKICK = "Sidekick";
        public const string TAG_SIDEKICK_LIGHT = "SidekickLight";
        public const string TAG_BACK = "GoBack";
        public const string TAG_FADER = "Fader";
        public const string TAG_FADER_ALL = "FaderAll";
        public const string TAG_DIRECTOR = "Director";
        public const string TAG_CAMERA = "MainCamera";
        public const string TAG_GO_NEXT = "GoNext";
    
        // DEMO - scene numbers (by index -- see list of scenes in build settings)
        public const int SCENE_DEMO_INTRO = 0;
        public const int SCENE_1_PACK = 1;
        public const int SCENE_2_ZOO = 2;
        public const int SCENE_3_PICNIC = 3;
        public const int SCENE_4_PARK = 4;
        public const int SCENE_5_ROOM = 5;
        public const int SCENE_6_BATH = 6;
        public const int SCENE_7_PARTY = 7;
        public const int SCENE_8_BYE = 8;
        
        // DEMO - names for scenes
        public const string NAME_1_PACK = "Session1";
        public const string NAME_2_ZOO = "Session2";
        public const string NAME_3_PICNIC = "Session3";
        public const string NAME_4_PARK = "Session4";
        public const string NAME_5_ROOM = "Session5";
        public const string NAME_6_BATH = "Session6";
        public const string NAME_7_PARTY = "Session7";
        public const string NAME_8_BYE = "Session8";
    
        // layers
        public const int LAYER_MOVEABLES = 10;
        public const int LAYER_STATICS = 8;
    
        // edges of screen - used to make sure objects aren't dragged off the screen
        // screen is 1280x768, minus the menu bar
        public const int LEFT_SIDE = -620; // -640
        public const int RIGHT_SIDE = 620; //640
        public const int TOP_SIDE = 360; //384
        public const int BOTTOM_SIDE = -330; //-384

        /** messages we can receive */
        public const int DISABLE_TOUCH = 1;
        public const int ENABLE_TOUCH = 2;
        public const int RESET = 0;
        public const int SIDEKICK_SAY = 4;
        public const int SIDEKICK_DO = 3;
        public const int LOAD_OBJECT = 5;
        public const int CLEAR = 6;
        public const int MOVE_OBJECT = 7;
        public const int HIGHLIGHT_OBJECT = 8;
        public const int REQUEST_KEYFRAME = 9;
        public const int FADE_SCREEN = 10;
        public const int UNFADE_SCREEN = 11;
        public const int NEXT_PAGE = 12;
        public const int PREV_PAGE = 13;
        
        /** next page and previous page */
        public const bool NEXT = true;
        public const bool PREVIOUS = false;
    
        /** sidekick animations */
        // name of each animation, from unity editor
        // we can receive rosmsgs with name of animation to play
        public const string ANIM_DEFAULT = "Default";
        public const string ANIM_SPEAK = "BeakOpenClose";
        public const string ANIM_FLAP = "FlapWings";
        public const string ANIM_FLAP_BEAKOPEN = "FlapBeakOpen";
        // flags for playing each animation (animator parameters)
        public static readonly Dictionary<string, string> ANIM_FLAGS = new Dictionary<string, string>
        {
            { ANIM_SPEAK, "Speak" },
            { ANIM_FLAP, "Fly" },
            { ANIM_FLAP_BEAKOPEN, "FlyBeakOpen"}
        };
        
        // DEMO sidekick speech
        public static string[] DEMO_SIDEKICK_SPEECH = new string[] { "ImAToucan", 
            "ImFromSpain", "AdiosSeeYouNext", ""};
        
        /** Websocket config file path */
        // if playing in unity on desktop:
        public const string OPAL_CONFIG = "opal_config.txt";
        public const string CONFIG_PATH_OSX = @"/Resources/";
        // if playing on tablet:
        public const string CONFIG_PATH_ANDROID = "mnt/sdcard/edu.mit.media.prg.sar.opal.base/";
    
    
        /** ROS-related constants: topics and message types */
        // general string log messages (e.g., "started up", "error", whatever)
        public const string LOG_ROSTOPIC = "/opal_tablet";
        public const string LOG_ROSMSG_TYPE = "std_msgs/String";
        // messages about actions taken on tablet (e.g., tap occurred on object x at xyz)
        // contains: 
        //  string object: name
        //  string action_type: tap
        //  float[] position: xyz
        public const string ACTION_ROSTOPIC = "/opal_tablet_action";
        public const string ACTION_ROSMSG_TYPE = "/sar_opal_msgs/OpalAction";
        // messages logging the entire current scene
        // contains:
        //  string background
        //  objects[] { name posn tag }
        public const string SCENE_ROSTOPIC = "/opal_tablet_scene";
        public const string SCENE_ROSMSG_TYPE = "/sar_opal_msgs/OpalScene";
        // commands from elsewhere that we should deal with
        public const string CMD_ROSTOPIC = "/opal_tablet_command";
        public const string CMD_ROSMSG_TYPE = "/sar_opal_msgs/OpalCommand";   
        // messages to tell the game node when we're done playing audio
        // contains:
        //   bool done playing
        public const string AUDIO_ROSTOPIC = "/opal_tablet_audio";
        public const string AUDIO_ROSMSG_TYPE = "/std_msgs/Bool";     
    }
}
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

    public static class Constants
    {
        /** where images to load as sprites are located in Resources */
        public const string GRAPHICS_FILE_PATH = "graphics/base-images/";
        public const string AUDIO_FILE_PATH = "audio/";

        /// <summary>
        /// tag applied to all playobjects
        /// </summary>
        public const string TAG_PLAY_OBJECT = "PlayObject";
        public const string TAG_LIGHT = "Light";
        public const string TAG_GESTURE_MAN = "GestureManager";
        public const string TAG_BACKGROUND = "Background";
        public const string TAG_SIDEKICK = "Sidekick";
    
        // edges of screen - used to make sure objects aren't dragged off the screen
        public const int LEFT_SIDE = -640;
        public const int RIGHT_SIDE = 640;
        public const int TOP_SIDE = 390;
        public const int BOTTOM_SIDE = -390;

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
        public const int GOT_TO_GOAL = 10;
    
        /** Websocket config file path */
        // if playing in unity on desktop:
        public const string WEBSOCKET_CONFIG = "websocket_config.txt";
        public const string CONFIG_PATH_OSX = @"/Resources/";
        // if playing on tablet:
        public const string CONFIG_PATH_ANDROID = "mnt/sdcard/edu.mit.media.prg.sar.opal.base/";
    
    
        // ROS-related constants: topics and message types
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
        public const string CMD_ROSTOPIC = "/opal_command";
        public const string CMD_ROSMSG_TYPE = "/sar_opal_msgs/OpalCommand";        
    }
}
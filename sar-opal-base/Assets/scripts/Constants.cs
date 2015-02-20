using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    /** messages we can receive */
    public const int DISABLE_TOUCH = 1;
    public const int ENABLE_TOUCH = 2;
    public const int RESET = 0;
    public const int SIDEKICK_SAY = 4;
    public const int SIDEKICK_DO = 3;
    public const int LOAD_OBJECT = 5;
    
    // TODO ROS
    public const string OUR_ROSTOPIC = "/opal_tablet";
    public const string CMD_ROSTOPIC = "/opal_command";
    
    public const string ROS_PUBLISH = @"{""op"":""advertise"",""topic"":""/opal"
		+ "_tablet"",""type"":""std_msgs/String""}";
    public const string ROS_SUBSCRIBE = @"{""op"":""subscribe"",""topic"":""/opal"
		+ "_command"",""type"":""sar_opal_msgs/OpalCommand""}";
	public const string ROS_TEST = @"{""op"":""publish"",""topic"":""/opal"
		+ "_tablet"",""msg"":{""data"":""Hello World!""}}";


	


}
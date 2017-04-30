// Jacqueline Kory Westlund
// June 2016
//
// The MIT License (MIT)
// Copyright (c) 2016 Personal Robots Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using MiniJSON;
using UnityEngine;

namespace opal
{
    public static class Utilities
    {

		public static void WriteConfig (string path, 
			string contents)
		{
			Logger.LogError ("writing config test...");
			File.WriteAllText (path,contents);

		}
        /// <summary>
        /// Decodes the JSON config file
        /// </summary>
        /// <param name="path">Path to config file</param>
        /// <param name="server">Server IP address for websocket connection</param>
        /// <param name="port">Port for websocket connection </param>
        /// <param name="toucan">Whether the Toucan should be present or not</param> 
        public static bool ParseConfig (string path, 
                                      out GameConfig gameConfig)
        {
            gameConfig.server = "";
            gameConfig.port = "";
            gameConfig.sidekick = false;
            gameConfig.logDebugToROS = false;
            gameConfig.opalActionTopic = "";
            gameConfig.opalAudioTopic = "";
            gameConfig.opalCommandTopic = "";
            gameConfig.opalLogTopic = "";
            gameConfig.opalSceneTopic = "";
            
            //if (!File.Exists(path))
            //{
            //    Logger.LogError("ERROR: can't find websocket config file at " +
            //              path);
            //    return;
            //}
            string config = "";

            try {
                config = File.ReadAllText(path);
                Logger.Log("got config: " + config);
                config.Replace("\n", "");
        
                Dictionary<string, object> data = null;
                data = Json.Deserialize(config) as Dictionary<string, object>;
                if(data == null) {   
                    Logger.LogError("Could not parse JSON config file!");
                    return false;
                }
                Logger.Log("deserialized " + data.Count + " objects from JSON!");
        
                // if the config file doesn't have all parts, consider it invalid
                if(!(data.ContainsKey("server")
                    && data.ContainsKey("port")
                    && data.ContainsKey("toucan")
                    && data.ContainsKey("log_debug_to_ros")
                    && data.ContainsKey("opal_action_topic")
                    && data.ContainsKey("opal_scene_topic")
                    && data.ContainsKey("opal_command_topic")
                    && data.ContainsKey("opal_audio_topic")
                    && data.ContainsKey("opal_log_topic")))
                {
                    Logger.LogError("Did not get a valid config file!");
                    return false;
                }
        
                // get configuration options
                gameConfig.server = (string)data["server"];
                gameConfig.port = (string)data["port"];
                gameConfig.sidekick = (bool)data["toucan"];
                gameConfig.logDebugToROS = (bool)data["log_debug_to_ros"];
                gameConfig.opalActionTopic = (string)data["opal_action_topic"];
                gameConfig.opalAudioTopic = (string)data["opal_audio_topic"];
                gameConfig.opalCommandTopic = (string)data["opal_command_topic"];
                gameConfig.opalSceneTopic = (string)data["opal_scene_topic"];
                gameConfig.opalLogTopic = (string)data["opal_log_topic"];
                
                Logger.Log("server: " + gameConfig.server + "\nport: " + gameConfig.port 
                          + "\nsidekick: " + gameConfig.sidekick + "\nlog_debug_to_ros: " 
                          + gameConfig.logDebugToROS + "\nopal_action_topic: "
                          + gameConfig.opalActionTopic + "\nopal_audio_topic: "
                          + gameConfig.opalAudioTopic + "\nopal_command_topic: "
                          + gameConfig.opalCommandTopic + "\nopal_log_topic: "
                          + gameConfig.opalLogTopic + "\nopal_scene_topic: "
                          + gameConfig.opalSceneTopic);
                return true;
        
            } catch(Exception e) {
                Logger.LogError("Could not read config file! File path given was " 
                    + path + "\nError: " + e);
                return false;
            }
    
        }
        
        /// <summary>
        /// Loads an image file from the specified filepath into a Texture2D, 
        /// then creates a sprite from the texture.
        /// </summary>
        /// <returns>Sprite from the loaded image</returns>
        /// <param name="filepath">Filepath to image</param>
        public static Sprite LoadSpriteFromFile(string filepath)
        {
            try 
            {        
                // read the image in as a byte array
                byte[] data = File.ReadAllBytes(filepath);
                // does not matter what size this texture is, as loadimage replaces it
                // with the actueal size of hte image
                Texture2D texture = new Texture2D(2,2);
                
                if (texture.LoadImage(data))
                {
                    texture.name = Path.GetFileNameWithoutExtension(filepath);
                    // transform texture into a sprite. Setting the rectangle position to 0,0 and
                    // the size to the texture's size will put the whole image into the sprite.
                    // The third vector is the pivot point, which is at the center of the image if
                    // you use (.5,.5) 
                    Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                        new Vector2(0.5f,0.5f));
                        
                    if (sp != null) return sp;
                    else Logger.LogWarning("Could not create sprite for file: " + filepath);
                }
                else {
                    Logger.LogWarning("Could not create texture from file: " + filepath);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Could not load image from file: " + filepath +
                    "\nError: " + e.Message 
                + e.StackTrace);
            }
            
            return null;
            // TODO This does not fail when a non-image file, such as a text file, is loaded
            // not sure why - need to look into this
        }


        /// <summary>
        /// Scale all camera views to the current screen size.
        /// </summary>
        /// <returns>True if we should scale to the height (i.e., aspect ratio
        /// of the screen is wider than it is tall, so we are using the full
        /// screen height), false if we should scale to the width (i.e., we are
        /// using the full screen width). </returns>
        public static bool ScaleCameraViewToScreen()
        {
            // The target aspect ratio for the game is 16:9. If the screen has
            // a different default aspect ratio than this, we will show the game
            // with empty/black bars filling the extra space on the top or sides
            // (so the game is shown in the desired aspect ratio). The reason we
            // set the aspect ratio for our game is because the graphics we are
            // using fit best at that ratio.
            float targetAspectRatio = 16.0f / 9.0f;

            // Determine the game screen's current aspect ratio.
            float currentAspectRatio = (float)Screen.width / (float)Screen.height;

            // We want the game to use the target aspect ratio, so we will scale
            // the camera's viewport height so the camera view's aspect ratio
            // matches the target aspect ratio.
            float scaleHeight = currentAspectRatio / targetAspectRatio;

            // However, we also want the entire game scene to be viewable on
            // screen. So we have to check whether scaling the height to the
            // target aspect ratio would be taller than would fit on the device
            // screen, and if so, shrink the camera view to fit on the device
            // screen. We create a new rectangle that will be the camera's view
            // rectangle of the desired size and aspect ratio by scaling the
            // current camera view rectangle.
            // We scale the view for each camera we have.
            foreach (Camera c in Camera.allCameras)
            {
                Rect rect = c.rect;
                if (scaleHeight < 1.0f)
                {
                    // The height to scale to is smaller than the current
                    // screen height , so we use that height, set the
                    // corresponding width, and set the top left coordinate of
                    // where to put the camera rectangle on the screen. This is
                    // how we get evenly distributed empty/black bars above and
                    // below the game window.
                    rect.width = 1.0f;
                    rect.height = scaleHeight;
                    rect.x = 0;
                    rect.y = (1.0f - scaleHeight) / 2.0f;
                    // Now that we have our rectangle, change the camera view.
                    c.rect = rect;
                    // We return whether or not we should scale to the height--
                    // that is, is the aspect ratio of the screen wider than it
                    // is tall, and thus are we using the full screen height?
                    // Otherwise, the screen is taller than it is wide, so we
                    // are using the full screen width instead.
                    return false;
                }
                else
                {
                    // Otherwise, the height to scale to is bigger than the
                    // current screen height, so we set the height as tall as
                    // we can and scale the width to be smaller and fit too. In
                    // this case, we set the top left coordinate such that we
                    // have empty/black bars to the left and right of the game
                    // window.
                    rect.width = (1.0f / scaleHeight);
                    rect.height = 1.0f;
                    rect.x = (1.0f - (1.0f / scaleHeight)) / 2.0f;
                    rect.y = 0;
                    // Now that we have our rectangle, change the camera view.
                    c.rect = rect;
                    // Return true to indicate that we should scale based on the
                    // height, since we are using the full screen height. 
                    return true;
                    // Note that we could also set the orthographic size of the
                    // camera (which is half the vertical height of the view) to
                    // be the half the height of the screen. But this would not
                    // necessarily keep our desired aspect ratio, since Unity
                    // would adjust the horizontal size based on the viewport's
                    // aspect ratio, which is not necessarily something we
                    // control.
                }
            }
            // Default return something, but we will always have a camera, so we
            // should never get here.
            return true;
        }
    }
}

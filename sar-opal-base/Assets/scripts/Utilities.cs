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
                if(!(data.ContainsKey("server") && data.ContainsKey("port") 
                    && data.ContainsKey("toucan") && data.ContainsKey("log_debug_to_ros"))) {
                    Logger.LogError("Did not get a valid config file!");
                    return false;
                }
        
                // get configuration options
                gameConfig.server = (string)data["server"];
                gameConfig.port = (string)data["port"];
                gameConfig.sidekick = (bool)data["toucan"];
                gameConfig.logDebugToROS = (bool)data["log_debug_to_ros"];

                Logger.Log("server: " + gameConfig.server + "  port: " + gameConfig.port 
                          + "  sidekick: " + gameConfig.sidekick + "  log_debug_to_ros: " 
                          + gameConfig.logDebugToROS);
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
                Logger.LogError("Could not load image from file: " + filepath + "\nError: " + e.Message 
                + e.StackTrace);
            }
            
            return null;
            // TODO This does not fail when a non-image file, such as a text file, is loaded
            // not sure why - need to look into this
        }

    }
}

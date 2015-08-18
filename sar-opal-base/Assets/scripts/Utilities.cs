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
            
            //if (!File.Exists(path))
            //{
            //    Debug.LogError("ERROR: can't find websocket config file at " +
            //              path);
            //    return;
            //}
            string config = "";
            try {
                config = File.ReadAllText(path);
                Debug.Log("got config: " + config);
                config.Replace("\n", "");
        
                Dictionary<string, object> data = null;
                data = Json.Deserialize(config) as Dictionary<string, object>;
                if(data == null) {   
                    Debug.LogError("Could not parse JSON config file!");
                    return false;
                }
                Debug.Log("deserialized " + data.Count + " objects from JSON!");
        
                // if the config file doesn't have all parts, consider it invalid
                if(!(data.ContainsKey("server") && data.ContainsKey("port") 
                    && data.ContainsKey("toucan"))) {
                    Debug.LogError("Did not get a valid config file!");
                    return false;
                }
        
                // get server and port
                gameConfig.server = (string)data["server"];
                gameConfig.port = (string)data["port"];
                gameConfig.sidekick = (bool)data["toucan"];
                Debug.Log("server: " + gameConfig.server + "  port: " + gameConfig.port 
                          + "  sidekick: " + gameConfig.sidekick);
                return true;
        
            } catch(Exception e) {
                Debug.LogError("Could not read config file! File path given was " 
                    + path + "\nError: " + e);
                return false;
            }
    
        }

    }
}

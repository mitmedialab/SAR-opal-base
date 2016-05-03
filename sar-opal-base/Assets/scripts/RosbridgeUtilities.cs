using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using MiniJSON;
using UnityEngine;

namespace opal
{
    public static class RosbridgeUtilities
    {
        /// <summary>
        /// Builds a JSON string message to publish over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to publish on </param>
        /// <param name="message">The string message to send</param>
        public static string GetROSJsonPublishStringMsg (string topic, string message)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosPublish = new Dictionary<string, object>();
            rosPublish.Add("op", "publish");
            rosPublish.Add("topic", topic);
            Dictionary<string,object> rosMessage = new Dictionary<string, object>();
            rosMessage.Add("data", message);
            rosPublish.Add("msg", rosMessage);
        
            return Json.Serialize(rosPublish);
        }
        
        /// <summary>
        /// Builds a JSON audio message to publish over rosbridge
        /// </summary>
        /// <returns>The ROS json publish audio message.</returns>
        /// <param name="topic">ROS topic to publish on.</param>
        /// <param name="message">If set to <c>true</c> audio is done playing.</param>
        public static string GetROSJsonPublishAudioMsg(string topic, bool message)
        {
            // build dictionary of things to include in message
            Dictionary<string,object> rosPublish = new Dictionary<string, object>();
            rosPublish.Add("op","publish");
            rosPublish.Add("topic", topic);
            Dictionary<string,object> rosMessage = new Dictionary<string, object>();
            rosMessage.Add("data", message);
            rosPublish.Add("msg", rosMessage);
            
            return Json.Serialize(rosPublish);
            
        }
    
        /// <summary>
        /// Builds a JSON metrics log message to publish over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to publish on</param>
        /// <param name="objectName">Name of object that has current goal</param>
        /// <param name="distance">Distance to object's goal</param>
        /// <param name="timestamp">Time of the action</param>
        public static string GetROSJsonPublishMetricsMsg (string topic, string objectName,
        float distance, double timestamp)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosPublish = new Dictionary<string, object>();
            rosPublish.Add("op", "publish");
            rosPublish.Add("topic", topic);
            Dictionary<string,object> rosMessage = new Dictionary<string, object>();
            rosMessage.Add("objectName", objectName);
            rosMessage.Add("distance", distance);
            rosMessage.Add("timestamp", timestamp);
            rosPublish.Add("msg", rosMessage);
        
            return Json.Serialize(rosPublish);
        }
    
        /// <summary>
        /// Builds a JSON action log message to publish over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to publish on</param>
        /// <param name="objectName">Name of object that was acted upon</param>
        /// <param name="distance">Position of the object</param>
        /// <param name="timestamp">Time of the action</param>
        public static string GetROSJsonPublishActionMsg (string topic, string objectName,
            string objectTwoName, string action, float[] position, float[] positionTwo)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosPublish = new Dictionary<string, object>();
            rosPublish.Add("op", "publish");
            rosPublish.Add("topic", topic);
            Dictionary<string,object> rosMessage = new Dictionary<string, object>();
            rosMessage.Add("objectName", objectName);
            rosMessage.Add("objectTwoName", objectTwoName);
            rosMessage.Add("position", position);
            rosMessage.Add("positionTwo", positionTwo);
            rosMessage.Add("action", action);
            rosPublish.Add("msg", rosMessage);
        
            return Json.Serialize(rosPublish);
        }
    
        /// <summary>
        /// Builds a JSON scene 'keyframe' log message to publish over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to publish on</param>
        /// <param name="background">Name of current background image</param>
        /// <param name="objects">array of objects present in scene</param>
        /// <param name="timestamp">Time of the action</param>
        public static string GetROSJsonPublishSceneMsg (string topic, LogEvent.SceneObject[] objects)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosPublish = new Dictionary<string, object>();
            rosPublish.Add("op", "publish");
            rosPublish.Add("topic", topic);
            Dictionary<string,object> rosMessage = new Dictionary<string, object>();
            
            // make array of JSON strings from the SceneObjects to add
            string[] objs = new string[objects.Length];
            for(int i=0; i<objects.Length; i++) 
            {
                Dictionary<string,object> objd = new Dictionary<string, object>();
                objd.Add("name", objects[i].name);
                objd.Add("position", objects[i].position);
                objd.Add("tag", objects[i].tag);
                objd.Add("scale",objects[i].scale);
                objd.Add("draggable", objects[i].draggable);
                objd.Add("audioFile", objects[i].audio);
                objs[i] = Json.Serialize(objd);
            }     
        
            rosMessage.Add("objects", objs);
            rosPublish.Add("msg", rosMessage);
        
            return Json.Serialize(rosPublish);
        }

        /*public static string GetROSJsonSceneObject(string name, float[] position,
                                               string state)
    {
        // build a dictionary of things to include in the message
        Dictionary<string,object> rosPublish = new Dictionary<string, object>();
        rosPublish.Add("op", "publish");
        rosPublish.Add("topic", topic);
        Dictionary<string,object> rosMessage = new Dictionary<string, object>();
        rosMessage.Add("objectName", objectName);
        rosMessage.Add("position", position);
        rosMessage.Add("tag", tag);
        rosPublish.Add("msg", rosMessage);
        
        return Json.Serialize(rosPublish);
    }*/

    
        /// <summary>
        /// Build a JSON string message to subscribe to a rostopic over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="messageType">The rosmsg type of the topic</param>
        public static string GetROSJsonSubscribeMsg (string topic, string messageType)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosSubscribe = new Dictionary<string, object>();
            rosSubscribe.Add("op", "subscribe");
            rosSubscribe.Add("topic", topic);
            rosSubscribe.Add("type", messageType);
        
            return Json.Serialize(rosSubscribe);
        }
    
   
        /// <summary>
        /// Build a JSON string message to advertise a rostopic over rosbridge
        /// </summary>
        /// <returns>A JSON string to send</returns>
        /// <param name="topic">The topic to advertise</param>
        /// <param name="messageType">The rosmsg type of the topic</param>
        public static string GetROSJsonAdvertiseMsg (string topic, string messageType)
        {
            // build a dictionary of things to include in the message
            Dictionary<string,object> rosAdvertise = new Dictionary<string, object>();
            rosAdvertise.Add("op", "advertise");
            rosAdvertise.Add("topic", topic);
            rosAdvertise.Add("type", messageType); 
        
            return Json.Serialize(rosAdvertise);
        }
    
        /// <summary>
        /// Decode a ROS JSON command message
        /// </summary>
        /// <param name="msg">the message received</param>
        /// <param name="command">the command received</param>
        /// <param name="properties">command properties received</param>
        public static void DecodeROSJsonCommand (string rosmsg, out int command,
                                            out object properties)
        {
            // set up out objects
            command = -1;
            properties = null;
            // parse data, see if it's valid
            //
            // messages might look like:
            // {"topic": "/opal_command", "msg": {"command": 5, "properties": 
            // "{\"draggable\": \"true\", \"initPosition\": {\"y\": \"300\", \"x\":
            //  \"-300\", \"z\": \"0\"}, \"name\": \"ball2\", \"scale\": 
            // "{\"y\": \"100\", \"x\": \"100\", \"z\": \"100\"}", \"audioFile\": 
            // \"chimes\"}"}, "op": "publish"}
            //
            // or:
            // "topic": "/opal_command", "msg": {"command": 2, 
            //  "properties": ""}, "op": "publish"
            //
            // should be valid json, so we try parsing the json
            Dictionary<string, object> data = null;
            data = Json.Deserialize(rosmsg) as Dictionary<string, object>;
            if(data == null) {   
                Debug.LogWarning("Could not parse JSON message!");
                return;
            }
            Debug.Log("deserialized " + data.Count + " objects from JSON!");
        
            // message sent over rosbridge comes with the topic name and what the
            // operation was
            //
            // TODO should we check that the topic matches one that we're subscribed
            // to before parsing further? Would need to keep a list of subscriptions. 
            //
            // if the message doesn't have all three parts, consider it invalid
            if(!data.ContainsKey("msg") && !data.ContainsKey("topic") && !data.ContainsKey("op")) {
                Debug.LogWarning("Did not get a valid message!");
                return;
            }
        
            Debug.Log("Got " + data["op"] + " message on topic " + data["topic"]);
        
            // parse the actual message
            Debug.Log("parsing message: " + data["msg"]);
            Dictionary<string, object> msg = data["msg"] as Dictionary<string, object>;
        
            // get the command
            if(msg.ContainsKey("command")) {
                Debug.Log("command: " + msg["command"]);
                try {
                    command = Convert.ToInt32(msg["command"]);
                } catch(Exception ex) {
                    Debug.LogError("Error! Could not get command: " + ex);
                }
            }
            
            // if the properties are missing or there aren't any properties, 
            // we're done, return command only
            if(!msg.ContainsKey("properties") || 
                ((string)msg["properties"]).Equals("")) {
                Debug.Log("no properties found, done parsing");
                return;
            }
        
            // otherwise, we've got properties, decode them.
            Debug.Log("properties: " + msg["properties"]);
        
            // parse data, see if it's valid json
            Dictionary<string, object> props = null;
            props = Json.Deserialize((string)msg["properties"]) as Dictionary<string, object>;
            // if we can't deserialize the json message, return
            if(props == null) {   
                Debug.Log("Could not parse JSON properties! Could just be a string.");
            
                // so properties could be just a string (e.g. if command is SIDEKICK_DO)
                if(msg["properties"] is String) {
                    properties = (string)msg["properties"];
                } else {
                    Debug.LogWarning("Could not parse as a string either!");
                    properties = "";
                }
                return;
            }
            // otherwise, we got properties!
            Debug.Log("deserialized " + props.Count + " properties from JSON!");
        
            // if the properties contain the tag "play object", we're loading a 
            // play object, so build up a properties object
            if(props.ContainsKey("tag") &&
                ((string)props["tag"]).Equals(Constants.TAG_PLAY_OBJECT)) 
            {
                PlayObjectProperties pops = new PlayObjectProperties();
            
                pops.SetTag((string)props["tag"]);
                
                if(props.ContainsKey("name"))
                    pops.SetName((string)props["name"]);
                
                try {
                    if(props.ContainsKey("draggable"))
                        pops.draggable = Convert.ToBoolean(props["draggable"]);
                } catch(Exception ex) {
                    Debug.LogError("Error! Could not determine if draggable: " + ex);
                }
        
                if(props.ContainsKey("audioFile"))
                {
                    try {
                        pops.SetAudioFile((string)props["audioFile"]);
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not get audio file: " + ex);
                    }
                }
            
                if(props.ContainsKey("position")) 
                {
                    // this is the weird way of converting an object back into
                    // an int array .. not as straightforward as it should be!
                    try {
                        int[] posn = ObjectToIntArray(props["position"] as IEnumerable);
                        pops.SetInitPosition(new Vector3(posn[0], posn[1], posn[2]));
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not get initial position: " + ex);
                    }
                }
                
                if(props.ContainsKey("scale")) 
                {
                    // same weird way of converting an object back to int array
                    try {
                        int[] posn = ObjectToIntArray(props["scale"] as IEnumerable);
                        pops.SetScale(new Vector3(posn[0], posn[1], posn[2]));
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not get initial position: " + ex);
                    }
                }
                
                if(props.ContainsKey("slot"))
                {
                    try {
                        pops.SetSlot(Convert.ToInt32(props["slot"]));
                    }
                    catch(Exception ex) {
                        Debug.LogError("Error! Could not get slot number: " + ex);
                    }
                }
            
                if(props.ContainsKey("answerSlot"))
                {
                    try {
                        pops.isAnswerSlot = Convert.ToBoolean(props["answerSlot"]);
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not determine if slot is answer or scene: " + ex);
                    }
                }
                
                if(props.ContainsKey("correctSlot"))
                {
                    try {
                        pops.SetSlot(Convert.ToInt32(props["correctSlot"]));
                    }
                    catch(Exception ex) {
                        Debug.LogError("Error! Could not get correct slot number: " + ex);
                    }
                }
                
                
                // get end positions
                // NOTE: We are not using the end position property!
                // Leaving code here, commented out, in case we add it back...
                /*if(props.ContainsKey("endPositions")) {
                    try {
                        IEnumerable en = props["endPositions"] as IEnumerable;
                        foreach(IEnumerable element in en) {
                            int[] posn = ObjectToIntArray(element);
                            pops.AddEndPosition(new Vector3(posn[0], posn[1], posn[2]));
                        }
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not get end position: " + ex);
                    }
                }*/
                properties = pops; // return the properties
                Debug.Log(props);
            }
            // if we are loading a background object, build up its properties instead
            else if(props.ContainsKey("tag") && 
                (((string)props["tag"]).Equals(Constants.TAG_BACKGROUND) ||
                ((string)props["tag"]).Equals(Constants.TAG_FOREGROUND))) {
                BackgroundObjectProperties bops = new BackgroundObjectProperties();
                bops.SetTag((string)props["tag"]);
                if(props.ContainsKey("name"))
                    bops.SetName((string)props["name"]);
                if(props.ContainsKey("position")) {
                    try {
                        // this is the weird way of converting an object back into
                        // an int array .. not as straightforward as it should be!
                        int[] posn = ObjectToIntArray(props["position"] as IEnumerable);
                        //Debug.Log("posn: " + posn);
                        bops.SetInitPosition(new Vector3(posn[0], posn[1], posn[2]));
                    } catch(Exception ex) {
                        Debug.LogError("Error! Could not get initial position: " + ex);
                    }
                }
                properties = bops; // return the background object properties
            }
        
            // if we are going to move an object to some destination, build a struct
            else if(props.ContainsKey("destination")) 
            {
                MoveObject mo = new MoveObject();
                if(props.ContainsKey("name"))
                    mo.name = ((string)props["name"]);
            
                // and try to get the destination
                try {
                    // this is the weird way of converting an object back into
                    // an int array .. not as straightforward as it should be!
                    int[] posn = ObjectToIntArray(props["destination"] as IEnumerable);
                    Debug.Log("posn: " + posn);
                    mo.destination = new Vector3(posn[0], posn[1], posn[2]);
                } catch(Exception ex) {
                    Debug.LogError("Error! Could not get destination: " + ex);
                } 
                properties = mo; // return the move object properties
            }
            
            // if we are setting objects as "correct" or "incorrect", build a struct
            else if (props.ContainsKey("correct"))
            {
                SetCorrectObject sco = new SetCorrectObject();
                sco.correct = ObjectToStringArray(props["correct"] as IEnumerable);
                if (props.ContainsKey("incorrect"))
                    sco.incorrect = ObjectToStringArray(props["incorrect"] as IEnumerable);
                
                properties = sco; // return the set correct object properties
            }
        
        }
    
        /// <summary>
        /// convert an object to an int array
        /// </summary>
        /// <returns>int array</returns>
        /// <param name="en">object that is secretly an int array</param>
        private static int[] ObjectToIntArray (IEnumerable en)
        {
            // C# is weird about conversions from object to arrays
            // so this is a hack way of converting an object into an
            // IEnumerable so we can then convert each element of the
            // array to a number, so we can then make an array.....
            int[] posn = {0,0,0};
            if(en != null) 
            {
                int count = 0;
                foreach(object el in en) 
                {
                    posn[count] = Convert.ToInt32(el);
                    count++;
                }
            }
            return posn;
        }
        
        /// <summary>
        /// convert object to a string array
        /// </summary>
        /// <returns>string array.</returns>
        /// <param name="en">object that is secretly a string array</param>
        private static string[] ObjectToStringArray (IEnumerable en)
        {
            // C# is weird about conversions from object to arrays
            // so this is a hack way of converting an object into an
            // IEnumerable so we can then convert each element of the
            // array to a string, so we can then make an array.....
            string[] s;
            if (en != null)
            {
                // get length of array
                int count = 0;
                foreach(object el in en) 
                {
                    count++;
                }
                // make a destination array of the right size 
                s = new string[count];
                
                // reset counter
                count = 0;
                
                // convert each element to a string
                foreach(object el in en) 
                {
                    s[count] = Convert.ToString(el);
                    count++;
                }
                return s;
            }
            return null;
        }
    
    }
}


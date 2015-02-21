using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using UnityEngine;

public static class RosbridgeUtilities
{
    /// <summary>
    /// Build a JSON string message to publish over rosbridge
    /// </summary>
    /// <returns>A JSON string to send</returns>
    /// <param name="topic">The topic to publish on </param>
    /// <param name="message">The message to send</param>
    public static string GetROSJsonPublishMsg(string topic, string message)
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
    /// Build a JSON string message to subscribe to a rostopic over rosbridge
    /// </summary>
    /// <returns>A JSON string to send</returns>
    /// <param name="topic">The topic to subscribe to</param>
    /// <param name="messageType">The rosmsg type of the topic</param>
    public static string GetROSJsonSubscribeMsg(string topic, string messageType)
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
    public static string GetROSJsonAdvertiseMsg(string topic, string messageType)
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
    public static void DecodeROSJsonCommand(string rosmsg)
    {
        // parse data, see if it's valid
        //
        // messages might look like:
        // {"topic": "/opal_command", "msg": {"command": 5, "properties": 
        // "{\"draggable\": \"true\", \"initPosition\": {\"y\": \"300\", \"x\":
        //  \"-300\", \"z\": \"0\"}, \"name\": \"ball2\", \"endPositions\": 
        // \"null\", \"audioFile\": \"chimes\"}"}, "op": "publish"}
        //
        // or:
        // "topic": "/opal_command", "msg": {"command": 2, 
        //  "properties": ""}, "op": "publish"
        //
        // should be valid json, so we try parsing the json
        Dictionary<string, object> data = null;
        //try {
        data = Json.Deserialize(rosmsg) as Dictionary<string, object>;
        if (data == null)
        {   
            Debug.Log ("Could not parse json!");
            return;
        }
        
        Debug.Log ("deserialized " + data.Count + " objects from json!");
        
        // message sent over rosbridge comes with the topic name and what the
        // operation was - so we check that the topic matches one that we're
        // subscribed to before parsing further TODO
        // TODO keep list of topics we've subscribed to?
        if (data.ContainsKey("topic"))
            Debug.Log("topic: " + data["topic"]);
            
        if (data.ContainsKey("op"))
            Debug.Log("op: " + data["op"]); // should be a publish message
        
        // parse the actual message
        if (data.ContainsKey("msg"))
        {
            Debug.Log("msg: " + data["msg"]);
            
            Dictionary<string, object> msg = data["msg"] as Dictionary<string, object>;
            
            // get the command
            if (msg.ContainsKey("command"))
                Debug.Log("command: " + msg["command"]);
                
            // get the properties, if any
            if (msg.ContainsKey("properties"))
            {
                Debug.Log("properties: " + msg["properties"]);
                
                // if there are properties, decode them
                if (msg["properties"] != "")
                {
                    // parse data, see if it's valid json
                    Dictionary<string, object> props = null;
                    props = Json.Deserialize(msg["properties"]) as Dictionary<string, object>;
                    if (props != null)
                    {
                        Debug.Log("got properties!");
                    }
                }
            }
                
        }
            

        
}
                    
    }
    
    

}

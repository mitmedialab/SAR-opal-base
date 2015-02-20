using System.Collections.Generic;
using MiniJSON;

public static class RosbridgeUtilities
{
    /// <summary>
    /// Build a JSON string message to publish over rosbridge
    /// </summary>
    /// <returns>A JSON string to send</returns>
    /// <param name="message">The message to send</param>
    public string GetROSJsonPublishMsg(string message)
    {
        Dictionary<string,object> rosPublish = new Dictionary<string, object>();
        rosPublish.Add("op", "publish");
        rosPublish.Add("topic", "/opal_tablet");
        Dictionary<string,object> rosMessage = new Dictionary<string, object>();
        rosMessage.Add("data","Opal tablet checking in");
        rosPublish.Add("msg", rosMessage);
        
        return "";
    }
    
    /// <summary>
    /// Build a JSON string message to subscribe to a rostopic over rosbridge
    /// </summary>
    /// <returns>A JSON string to send</returns>
    /// <param name="message">The topic to subscribe to</param>
    public string GetROSJsonSubscribeMsg(string topic)
    {
        // set up ros messages
        Dictionary<string,object> rosSubscribe = new Dictionary<string, object>();
        rosSubscribe.Add("op", "subscribe");
        rosSubscribe.Add("topic", "/opal_command");
        rosSubscribe.Add("type", "sar_opal_msgs/OpalCommand");
        
        return "";
    }
    
   
    /// <summary>
    /// Build a JSON string message to advertise a rostopic over rosbridge
    /// </summary>
    /// <returns>A JSON string to send</returns>
    /// <param name="topic">The topic to advertise</param>
    public string GetROSJsonAdvertiseMsg(string topic)
    {
        // set up ros messages
        Diictionary<string,object> rosAdvertise = new Dictionary<string, object>();
        rosAdvertise.Add("op", "advertise");
        rosAdvertise.Add("topic", "/opal_tablet");
        rosAdvertise.Add("type", "std_msgs/String"); // TODO make OpalLog tablet msg
        
        return "";
    }
    
    /// <summary>
    /// Decode a ROS JSON command message
    /// </summary>
    /// <param name="msg">the message received</param>
    public void DecodeROSJsonCommand(string msg)
    {
        // parse data, see if it's valid
        // should be valid json, so we try parsing the json
        Dictionary<String, object> data = null;
        //try {
        data = Json.Deserialize(msg) as Dictionary<String, object>;
        if (data == null)
        {   
            Debug.Log ("Could not parse json!");
            return;
        }
        
        Debug.Log ("deserialized objects from json!");
        
        // might be {"topic": "/opal_command", "msg": {"command": 5, "properties": 
        // "{\"draggable\": \"true\", \"initPosition\": {\"y\": \"300\", \"x\": \"-300\",
        //  \"z\": \"0\"}, \"name\": \"ball2\", \"endPositions\": \"null\", \"audioFile\": 
        // \"chimes\"}"}, "op": "publish"}
                    
        // or might be "topic": "/opal_command", "msg": {"command": 2, 
        //  "properties": ""}, "op": "publish"}
                    
    }
    
    

}

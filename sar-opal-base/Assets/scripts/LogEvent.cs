using UnityEngine;

namespace opal
{
/// <summary>
/// Log event. Use when firing log events. Anyone who listens
/// for log events can pull all the relevant information out
/// of this object for whatever kind of message is being
/// logged.
/// </summary>
    public class LogEvent
    {

        // log message event -- fire when you want to log something
        // so others who do logging can listen for the messages
        public delegate void LogEventHandler(object sender,
                            LogEvent logme);
        
        /// <summary>
        /// the Logger can use the log message type to
        /// figure out which fields matter for that type
        /// </summary>
        public enum EventType
        {
            Action,
            Scene,
            Message
            
    }
        ;

        /// <summary>
        /// the type of this log message
        /// </summary>
        public EventType? type = null;
    
        /// <summary>
        /// name of the relevant game object - e.g., the object on which the
        /// action occurred or the background object
        /// </summary>
        public string name = "";
        
        /// <summary>
        /// name of the relevant game object - e.g., the object on which the
        /// action occurred or the background object
        /// </summary>
        public string nameTwo = "";
    
        /// <summary>
        /// name of action that occurred
        /// </summary>
        public string action = "";
    
        /// <summary>
        /// x,y,z position of object
        /// </summary>
        public Vector3? position = null;
        
        /// <summary>
        /// x,y,z position of object
        /// </summary>
        public Vector3? positionTwo = null;
    
        /// <summary>
        /// state of object or miscellanous string message
        /// </summary>
        public string message = "";
    
        /// <summary>
        /// state of an object in the scene
        /// </summary>
        public struct SceneObject
        {
            public string name;
            public float[] position;
            public string tag;
            public bool draggable;
            public string audio;
            public float[] scale;
            public int correctSlot;
            public bool isCorrect;
            public bool isIncorrect;
        }
    
        public SceneObject[] sceneObjects = null;
    

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="state">State or miscellanous string message</param>
        public LogEvent(EventType type, string message)
        {
            this.type = type;
            this.message = message;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="background">Name of current background image or "" if none</param>
        /// <param name="sceneObjects">Array of current objects in scene</param>
        public LogEvent(EventType type, SceneObject[] sceneObjects)
        {
            this.type = type;
            this.sceneObjects = sceneObjects;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="name">Name.</param>
        /// <param name="action">Action.</param>
        /// <param name="position">Position.</param>
        public LogEvent(EventType type, string name, string action, Vector3? position)
        {
            this.type = type;
            this.name = name;
            this.action = action;
            this.position = position;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class.
        /// </summary>
        /// <param name="type">Event Type.</param>
        /// <param name="name">Name.</param>
        /// <param name="action">Action.</param>
        /// <param name="position">Position.</param>
        /// <param name="state">State.</param>
        public LogEvent(EventType type, string name, string action, Vector3? position,
            string message)
        {
            this.type = type;
            this.name = name;
            this.action = action;
            this.position = position;
            this.message = message;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="opal.LogEvent"/> class.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="name">Name of first object involved in the action.</param>
        /// <param name="nameTwo">Name of second object</param>
        /// <param name="action">Action.</param>
        /// <param name="position">Position of first object.</param>
        /// <param name="positionTwo">Position of second object</param>
        /// <param name="state">State.</param>
        public LogEvent(EventType type, string name, string nameTwo, string action, 
            Vector3? position, Vector3? positionTwo)
        {
            this.type = type;
            this.name = name;
            this.action = action;
            this.position = position;
            this.positionTwo = positionTwo;
            this.nameTwo = nameTwo;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="opal.LogEvent"/> class.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="name">Name.</param>
        /// <param name="nameTwo">Name two.</param>
        /// <param name="action">Action.</param>
        /// <param name="position">Position.</param>
        /// <param name="positionTwo">Position two.</param>
        /// <param name="message">Message.</param>
        public LogEvent(EventType type, string name, string nameTwo, string action, 
                        Vector3? position, Vector3? positionTwo, string message)
        {
            this.type = type;
            this.name = name;
            this.action = action;
            this.position = position;
            this.positionTwo = positionTwo;
            this.nameTwo = nameTwo;
            this.message = message;
        }

    }
}

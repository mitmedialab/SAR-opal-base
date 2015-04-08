using System;
using UnityEngine;

namespace opal
{
    /// <summary>
    /// Manage collision events
    /// </summary>
    public class CollisionManager : MonoBehaviour
    {
        // for logging stuff
        public event LogEventHandler logEvent;
        
        /// <summary>
        /// Called on start, use to initialize stuff
        /// </summary>
        void Start ()
        {
        }
        
		 /// <summary>
		 /// Called when a trigger object enters the collider of another object
		 /// </summary>
		 /// <param name="other">Other.</param>
	    void OnTriggerEnter2D (Collider2D other)
	    {
            Debug.Log("TRIGGER " + other.name + " entered " + this.gameObject.name);
            // fire event indicating a collision occurred
            if(this.logEvent != null) {
                // send action log event
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                    this.name, other.gameObject.name, "collide", this.transform.position,
                    other.gameObject.transform.position));
            }
	    }
        
        /// <summary>
        /// Called when a trigger object exits the collider of another object
        /// </summary
        /// <param name="other">Other.</param>
        void OnTriggerExit2D (Collider2D other)
        {
            Debug.Log("TRIGGER " + other.name + " exited " + this.gameObject.name);
            // fire event indicating a collision occurred
            if(this.logEvent != null) {
                // send action log event
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                    this.name, other.gameObject.name, "collideEnd", this.transform.position,
                    other.gameObject.transform.position));
            }
        }
        
    }
}


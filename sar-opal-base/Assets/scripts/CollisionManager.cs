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
        
        // Are we playing a social stories game where the scenes are out of order?
        // If so, will track when scenes collide with slots so we can determine
        // when the scenes are dragged into the correct order
        public bool scenesInOrder = true;
        
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
            Debug.Log("COLLISION BEGIN " + other.name + " entered " + this.gameObject.name);
            // fire event indicating a collision occurred
            if(this.logEvent != null) {
                // send action log event
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                                                 this.name, other.gameObject.name, "collide", this.transform.position,
                                                 other.gameObject.transform.position));
            }
            
            // if social stories and scenes not in order, check whether this object
            // is a scene that collided with its correct slot
            if (!this.scenesInOrder)
            {
                // get saved properties so we can check
                SavedProperties sp = this.GetComponent<SavedProperties>();
                if(ReferenceEquals(sp, null))
                {
                    Debug.LogWarning("Tried to check collisions for " + this.name 
                    + " but could not find any saved properties.");
                } 
                // does the collided-with other's name contain the number of our slot?
                // and is it a scene slot?
                else if (other.name.Contains(sp.correctSlot) 
                        // TODO if other smaller object for collisions, change name we check
                        && other.name.Contains(Constants.SCENE_SLOT))
                        // TODO set up 2nd smaller object as collision detector
                        // since barely have to touch to officially collide
                {
                    // if so, yay! we've collided with the correct slot!
                    
                    // TODO show correct visual feedback for this slot briefly?
                    
                    // stop moving -- remove draggable and rigid body
                    Destroy(this.GetComponent<TouchScript.Behaviors.Transformer2D>());
                    Destroy(this.GetComponent<Rigidbody2D>());
                    
                    // Stop detecting collisions -- remove collision manager
                    Destroy(this.GetComponent<CollisionManager>());
                    
                    // snap-to place on top of slot
                    this.transform.position = new Vector3(other.transform.position.x,
                                                    other.transform.position.y,
                                                    this.transform.position.z);
                }
            }
	    }
        
        /// <summary>
        /// Called when a trigger object exits the collider of another object
        /// </summary
        /// <param name="other">Other.</param>
        void OnTriggerExit2D (Collider2D other)
        {
            Debug.Log("COLLISION END " + other.name + " exited " + this.gameObject.name);
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


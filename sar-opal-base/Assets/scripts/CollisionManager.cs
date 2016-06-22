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
                // and is it a scene collision slot? (we have a second smaller object
                // that is not the slot that we use to detect collision with the slot,
                // since you barely have to touch an object to officially collide
                else if (other.name.Contains(sp.correctSlot.ToString()) 
                        && other.name.Contains(Constants.SCENE_COLLIDE_SLOT))
                {
                    // if so, yay! we've collided with the correct slot!
                    
                    // TODO show correct visual feedback for this slot briefly?
                    
                    // stop moving -- remove draggable and rigid body
                    Destroy(this.GetComponent<TouchScript.Behaviors.Transformer>());
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


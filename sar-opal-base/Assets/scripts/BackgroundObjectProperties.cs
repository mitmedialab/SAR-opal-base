using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace opal
{
/**
 * properties of the background object, such as its position
 */
    public class BackgroundObjectProperties : SceneObjectProperties
    {
        /** initial position in the world */
        protected new Vector3 initPosn = new Vector3 (0,0,2);
    
        /// <summary>
        /// Sets the initial position - checks to make sure play objects
        /// are in front of the background plane
        /// </summary>
        /// <param name="posn">position</param>
        public new void SetInitPosition (Vector3 posn)
        {
            // 0 is the plane of the background image
            // negative numbers are toward the camera, in front of the background
            // so we want to make sure play objects are in front of the background plane
            // so we set the background plane as a positive number
            if(posn.z <= 0)
                this.initPosn = new Vector3(posn.x, posn.y, 2);
            
            
        }
    }
}

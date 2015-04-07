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
        /// <summary>
        /// Sets the initial position - checks to make sure play objects
        /// are in front of the background plane
        /// </summary>
        /// <param name="posn">position</param>
        public new void SetInitPosition (Vector3 posn)
        {
            base.SetInitPosition(posn);
            // 0 is the plane of the background image
            // negative numbers are toward the camera, in front of the background
            // so we want to make sure play objects are in front of the background plane
            if(this.initPosn.z <= 0)
                this.initPosn = new Vector3(this.initPosn.x, this.initPosn.y, 1);
            
            
        }
    }
}

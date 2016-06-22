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
        protected new Vector3 initPosn = new Vector3 (0,0,Constants.Z_BACKGROUND);
    
        /** initial scale of the background image */
        protected Vector3 scale = new Vector3(100,100,100);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="opal.BackgroundObjectProperties"/> class.
        /// </summary>
        public BackgroundObjectProperties()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="opal.BackgroundObjectProperties"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="scale">Scale.</param>
        public BackgroundObjectProperties(string name, string tag, Vector3 scale)
        {
            this.SetName(name);
            this.SetTag(tag);
            this.SetScale(scale);
        }
    
    
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
            this.initPosn = new Vector3(posn.x, posn.y, Constants.Z_BACKGROUND);
        }
        
        public void SetScale(Vector3 scale)
        {           
            if (scale.x > 0 && scale.y > 0 && scale.z > 0)
                this.scale = scale;
        }
        
        /** get scale */
        public Vector3 Scale ()
        {
            return this.scale;
        }
    }
}

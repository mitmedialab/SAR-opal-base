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
 * properties of an object to load, such as initial position
 */
    public class SceneObjectProperties
    {
        /** name of the image/texture to load */
        protected string objName = "";
    
        /** tag of object */
        protected string mytag = "";
    
        /** initial position in the world */
        protected Vector3 initPosn = Vector3.zero;
    
        /** constructor */
        public SceneObjectProperties()
        {
        }
    
        /* set all properties */
        public void setAll (string name, string tag, Vector3 initPosn)
        {
            this.SetName(name);
            this.SetTag(tag);
            this.initPosn = initPosn;
        }
    
        /** set object name */
        public void SetName (string name)
        {
            if(name != "")
                this.objName = name;
        }
    
        /** get object name */
        public string Name ()
        {
            return this.objName;
        }
    
        /** set object tag */
        public void SetTag (string tag)
        {
            if(tag != null)
                this.mytag = tag;
        }
    
        /** get object tag */
        public string Tag ()
        {
            return this.mytag;
        }
    
        /** set object initial position - checks that the desired initial
        * position is within the screen */
        public void SetInitPosition (Vector3 posn)
        {

            // Since we are scaling graphics to fit on the screen, the screen
            // size may not correspond to the camera view, depending on the
            // screen's aspect ratio. So this check to make sure new objects are
            // on the screen may not actually work to keep objects on screen,
            // and furthermore, may give the wrong answer in terms of screen
            // coordinates vs. world coordinates. TODO: revisit this and fix.
            //this.initPosn = this.CheckOnScreen(posn);
            this.initPosn = posn;
        }
    
        /** get object initial position */
        public Vector3 InitPosition ()
        {
            return this.initPosn;
        }
    
        /** checks that the position is within the screen */
        protected Vector3 CheckOnScreen (Vector3 posn)
        {
            // check if x,y position is on the screen
            // center of screen is 0,0
            if(posn.x > Screen.width / 2)
                posn.x = Screen.width / 2;
            else if(posn.x < -Screen.width / 2)
                posn.x = -Screen.width / 2;
            if(posn.y > Screen.height / 2)
                posn.y = Screen.height / 2;
            else if(posn.y < -Screen.height / 2)
                posn.y = -Screen.height / 2;
            return posn;
        }
    }
}
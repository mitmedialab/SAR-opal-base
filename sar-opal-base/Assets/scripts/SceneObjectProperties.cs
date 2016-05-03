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
        
            this.initPosn = this.CheckOnScreen(posn);
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
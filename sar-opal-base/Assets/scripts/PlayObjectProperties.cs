using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace opal
{

/**
 * properties of the play object, such as initial position and
 * whether the object is draggable
 */
    public class PlayObjectProperties : SceneObjectProperties
    {
        /** the object is draggable (true) or stationary (false) */
        public bool draggable = true;

        /** sound file to play on tap */
        private string audioFile = null;

        /** scale of object */
        private Vector3 scale =  new Vector3(1,1,1);

        /** constructor */
        public PlayObjectProperties()
        {
        }
    
        /** constructor */
        public PlayObjectProperties(string name, string tag, bool draggable, 
                                string audioFile, Vector3 initPosn, 
                                Vector3 scale)
        {
            this.SetName(name);
            this.SetTag(tag);
            this.draggable = draggable;
            this.audioFile = audioFile;
            this.SetInitPosition(initPosn);
            this.scale = scale;
            
        }

        /** set all properties - name of object, whether it is a draggable
     object, audio file to attach or null if none, initial position,
     end positions or null if none (stationary object or doesn't matter) */
        public void setAll (string name, string tag, bool draggable, 
                       string audioFile, Vector3 initPosn, Vector3 scale)
        {
            this.SetName(name);       
            this.SetTag(tag);
            this.draggable = draggable;
            this.audioFile = audioFile;
            this.SetInitPosition(initPosn);
            this.scale = scale;
        }
    
         /// <summary>
         /// Set scale of object
         /// </summary>
         /// <param name="scale">Scale.</param>
        public void SetScale (Vector3 scale)
        {
            // TODO check if scale is reasonable?
            if (scale.x > 0 && scale.y > 0 && scale.z > 0)
                this.scale = scale;
        }
   
        /** get end positions */
        public Vector3 Scale ()
        {
            return this.scale;
        }

        /** set audio file */
        public void SetAudioFile (string audioFile)
        {
            // TODO audio file - do some kind of error check on this?
            this.audioFile = audioFile;
        }
    
        /** get audio file */
        public string AudioFile ()
        {
            return this.audioFile;
        }

        /// <summary>
        /// Sets the initial position - checks to make sure play objects
        /// are in front of the background plane
        /// </summary>
        /// <param name="posn">position</param>
        public new void SetInitPosition (Vector3 posn)
        {
            base.SetInitPosition(posn);
            // 2 is the plane of the background image
            // zero and negative numbers are toward the camera, in front of the background
            // so we want to make sure play objects are in front of the background plane
            if(this.initPosn.z > 1)
                this.initPosn.z = 1;
        }
    
    
    }
}
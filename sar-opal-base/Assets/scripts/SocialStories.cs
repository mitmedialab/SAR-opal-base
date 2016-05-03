using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace opal
{
   /**
    * Social Stories
    *
    */
    public class SocialStories : MonoBehaviour
    {
        MainGameController mgc;
        
        /// <summary>
        /// Start this instance.
        /// </summary>
        void Start ()
        {
            // find maingamecontroller
            this.mgc = (MainGameController)GameObject.FindGameObjectWithTag(
                Constants.TAG_DIRECTOR).GetComponent<MainGameController>();
            if(this.mgc == null) {
                Debug.Log("ERROR: Could not find main game controller!");
            } else {
                Debug.Log("Got main game controller");
            }
            
            // TODO setup demo game using this?
            // load background, story scene slots, and answer slots
            //this.mgc.SetupSocialStoryScene(4, false, 5);
        }
       
        void OnEnable ()
        {  
        }
        
        void OnDisable ()
        {   
        }
        
        void Update ()
        {
        }
        
    }
}

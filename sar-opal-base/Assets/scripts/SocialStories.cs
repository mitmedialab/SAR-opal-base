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
            
            // load background, story scene slots, and answer slots
            SetupSocialStoryScene();
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
        
        
        /// <summary>
        /// Sets up the social story scene.
        /// </summary>
        void SetupSocialStoryScene()
        {
            // set up camera sizes so the viewport is the size of the screen
            // TODO move to MainGameController, adapt all scaling etc throughout to scale
            // propertly for screen size....
            foreach (Camera c in Camera.allCameras)
            {
                c.orthographicSize = Screen.height/2;
            }
            
            // load background image
            Debug.Log ("Loading background");
            Sprite bk = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + "WhiteBackground.png");
            BackgroundObjectProperties bops = new BackgroundObjectProperties(
                "WhiteBackground", Constants.TAG_BACKGROUND, 
                new Vector3((float)Screen.width, (float)Screen.width, (float)Screen.width));
            mgc.InstantiateBackground(bops, bk);
            
            // load story scene slots
            // find the image files for the scenes
            Sprite[] sprites = Resources.LoadAll<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                    + Constants.SOCIAL_STORY_FILE_PATH
                                                    + Constants.SS_SCENES_PATH);

            int counter = 0;
            foreach (Sprite s in sprites)
            {
                PlayObjectProperties pops = new PlayObjectProperties(
                    s.name, // name
                    Constants.TAG_PLAY_OBJECT, // tag
                    false, // draggable
                    null, // audio
                    new Vector3 (
                        // left edge + offset to first item + counter * width/count
                        (-Screen.width/2) 
                            + (Screen.width / (sprites.Length * 2)) 
                            + (counter * Screen.width / (sprites.Length)),
                        // near top of screen
                        Screen.height * 0.6f, 0f),
                    // scale to one portion of the screen width
                    new Vector3(60,60,60) // TODO scale not perfect
                    );
                
                // instantiate the scene slot
                if (GameObject.Find (s.name) == null) // skip duplicates
                {
                    this.mgc.InstantiatePlayObject(pops, s);
                }
                counter++;
            }
            
            // load answer slots
            // find the image files for the scenes
            Sprite[] asprites = Resources.LoadAll<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                         + Constants.SOCIAL_STORY_FILE_PATH
                                                         + Constants.SS_ANSWERS_PATH);
                                                         
            int acounter = 0;
            foreach (Sprite s in asprites)
            {
                PlayObjectProperties pops = new PlayObjectProperties(
                    s.name, // name
                    Constants.TAG_PLAY_OBJECT, // tag
                    false, // draggable
                    null, // audio
                    new Vector3 (
                        // left edge + offset to first item + counter * width/count
                        (-Screen.width/2) 
                        + (Screen.width / (asprites.Length * 2)) 
                        + (acounter * Screen.width / (asprites.Length)),
                        // near botton of screen
                        -Screen.height * 0.6f, 0f),
                    // scale to one portion of the screen width
                    new Vector3(60,60,60) // TODO scale not perfect
                    );
                
                // instantiate the scene slot
                if (GameObject.Find (s.name) == null) // skip duplicates
                {
                    this.mgc.InstantiatePlayObject(pops, s);
                }
                acounter++;
            }
        }
    }
}

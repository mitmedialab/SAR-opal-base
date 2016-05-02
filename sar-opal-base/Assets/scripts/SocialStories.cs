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
            SetupSocialStoryScene(4, false, 5);
            this.mgc.ToggleCorrect(true);
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
        /// <param name="num_scenes">Number of scenes in this story</param>
        /// <param name="scenes_in_order">If set to <c>true</c> scenes are in order.</param>
        /// <param name="num_answers">Number of answer options for this story</param>
        void SetupSocialStoryScene(int num_scenes, bool scenes_in_order, int num_answers)
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
            Sprite bk = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + "WhiteBackground");
            BackgroundObjectProperties bops = new BackgroundObjectProperties(
                "WhiteBackground", Constants.TAG_BACKGROUND, 
                new Vector3((float) Screen.width / bk.bounds.size.x, 
                        (float)Screen.width / bk.bounds.size.x, 
                        (float)Screen.width / bk.bounds.size.x));
            mgc.InstantiateBackground(bops, bk);

            // need to scale scene/answer slots to evenly fit in the screen
            // they can be bigger if there are fewer slots
            // but never make them taller than a one-third the screen height
            float slot_width = (float) (Screen.width / num_scenes * 0.75);
            if (slot_width > Screen.height / 3) slot_width = (float) (Screen.height / 3);
            
            // load the number of slots needed for this story
            for (int i = 0; i < num_scenes; i++)
            {
                Sprite s = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                       + Constants.SOCIAL_STORY_FILE_PATH
                                                       + Constants.SS_SCENESLOT_PATH
                                                       + Constants.SS_SLOT_NAME
                                                       + (scenes_in_order ? "" : (i+1).ToString()));
                if (s == null)
                {
                    Debug.LogError("Could not load scene slot image!" );
                    continue;
                }
                
                PlayObjectProperties pops = new PlayObjectProperties(
                    "scene" + i, // name
                    Constants.TAG_PLAY_OBJECT, // tag
                    false, // draggable
                    null, // audio
                    new Vector3 (
                    // left edge + offset to first item + counter * width/count
                    (-Screen.width/2) 
                    + (Screen.width / (num_scenes * 2)) 
                    + (i * Screen.width / (num_scenes)),
                    // near top of screen
                    Screen.height * 0.25f, 0f),
                    // scale slot to one portion of the screen width
                    new Vector3(slot_width / s.bounds.size.x,
                            slot_width / s.bounds.size.x,
                            slot_width / s.bounds.size.x)
                    );
                
                // instantiate the scene slot
                this.mgc.InstantiatePlayObject(pops, s);
            }
            
            // load answer slots
            // find the image files for the scenes
            // load the number of answer slots needed for this story
            // all answer slots look the same so load one graphic and reuse it
            Sprite ans = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                              + Constants.SOCIAL_STORY_FILE_PATH
                                              + Constants.SS_ANSWERS_PATH
                                              + Constants.SS_SLOT_NAME);
                                              
            Sprite feedc = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                + Constants.SOCIAL_STORY_FILE_PATH
                                                + Constants.SS_FEEDBACK_PATH
                                                + Constants.SS_CORRECT_FEEDBACK_NAME);  
                                                
            Sprite feedic = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                  + Constants.SOCIAL_STORY_FILE_PATH
                                                  + Constants.SS_FEEDBACK_PATH
                                                  + Constants.SS_INCORRECT_FEEDBACK_NAME);                                       
                                              
            for (int i = 0; i < num_answers; i++)
            {   
                // create answer slot
                PlayObjectProperties pops = new PlayObjectProperties(
                    "answer" + i, // name
                    Constants.TAG_PLAY_OBJECT, // tag
                    false, // draggable
                    null, // audio
                    new Vector3 (
                        // left edge + offset to first item + counter * width/count
                        (-Screen.width/2) 
                        + (Screen.width / (num_answers * 2)) 
                        + (i * Screen.width / (num_answers)),
                        // near botton of screen
                        -Screen.height * 0.25f, 0f),
                    // scale to one portion of the screen width
                    new Vector3(slot_width / ans.bounds.size.x,
                            slot_width / ans.bounds.size.x,
                            slot_width / ans.bounds.size.x)
                    );
                
                // instantiate the scene slot
                this.mgc.InstantiatePlayObject(pops, ans);
                
                // also load answer feedback graphics for answer slots
                // we know only one answer will be correct, so load 1 correct, x incorrect
                // like with the highlight, keep reference to the answer feedback graphics
                // but set them as not visible
                PlayObjectProperties pobps = new PlayObjectProperties(
                    (i < num_answers - 1 ? "feedback-incorrect" + i : "feedback-correct"), // name
                    (i < num_answers - 1 ? Constants.TAG_INCORRECT_FEEDBACK : 
                        Constants.TAG_CORRECT_FEEDBACK), // tag
                    false, // draggable
                    null, // audio
                    new Vector3 (
                    // left edge + offset to first item + counter * width/count
                    (-Screen.width/2) 
                    + (Screen.width / (num_answers * 2)) 
                    + (i * Screen.width / (num_answers)),
                    // near botton of screen
                    -Screen.height * 0.25f, -1f),
                    // scale to one portion of the screen width
                    new Vector3(slot_width / (i < num_answers - 1 ? feedic : feedc).bounds.size.x,
                        slot_width / (i < num_answers - 1 ? feedic : feedc).bounds.size.x,
                        slot_width / (i < num_answers - 1 ? feedic : feedc).bounds.size.x)
                    );
                
                // instantiate the scene slot
                this.mgc.InstantiatePlayObject(pobps, (i < num_answers - 1 ? feedic : feedc));
            }
            
            
           
            
            
        }
    }
}

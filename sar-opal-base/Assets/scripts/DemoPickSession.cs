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
using UnityEngine.SceneManagement;
using System;
using TouchScript.Gestures;
using TouchScript.Behaviors;
using TouchScript.Hit;

namespace opal
{
    /**
     * scene lets user pick which story scene to go to next
     *
     * */
    public class DemoPickSession : MonoBehaviour
    {
        /**
    	 * Initialize stuff
    	 **/
        void OnEnable()
        {
            // subscribe to tap gesture events
            GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
            foreach(GameObject go in gos) {
    	        // add a tap gesture component if one doesn't exist
    	        TapGesture tg = go.GetComponent<TapGesture>();
    	        if(tg == null) {
    	            tg = go.AddComponent<TapGesture>();
    	        }
    	        // checking for null anyway in case adding the component didn't work
    	        if(tg != null) {
    	            tg.Tapped += tappedHandler; // subscribe to tap events
    	            Logger.Log(go.name + " subscribed to tap events");
    	        }

                // and start pulsing
                go.GetComponent<GrowShrinkBehavior>().StartPulsing();
            }

            // also subscribe for the sidekick
            GameObject sk = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
            // add a tap gesture component if one doesn't exist
            TapGesture tapg = sk.GetComponent<TapGesture>();
            if(tapg == null) {
                tapg = sk.AddComponent<TapGesture>();
            }
            // checking for null anyway in case adding the component didn't work
            if(tapg != null) {
                tapg.Tapped += tappedHandler; // subscribe to tap events
                Logger.Log(sk.name + " subscribed to tap events");
            }
        }

        void OnDestroy()
        {
            // unsubscribe from tap events
            GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
            foreach(GameObject go in gos) {
                TapGesture tg = go.GetComponent<TapGesture>();
                if(tg != null) {
                    tg.Tapped -= tappedHandler;
                    Logger.Log(go.name + " unsubscribed from tap events");
                }
            }

            // also unsubscribe for the sidekick
            GameObject gob = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
            if (gob != null)
            {
                TapGesture tapg = gob.GetComponent<TapGesture>();
                if(tapg != null) {
                    tapg.Tapped -= tappedHandler;
                    Logger.Log(gob.name + " unsubscribed from tap events");
                }
            }
        }

        /**
    	 * update
    	 **/
        void Update()
        {
            // if user presses escape or 'back' button on android, exit program
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        /// <summary>
        /// Handle all tap events - log them and trigger actions in response
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void tappedHandler (object sender, EventArgs e)
        {
            // get the gesture that was sent to us
            // this gesture will tell us what object was touched
            TapGesture gesture = sender as TapGesture;
            HitData hit = gesture.GetScreenPositionHitData();
            // get info about where the hit object was located when the gesture was
            // recognized - i.e., where on the object (in screen dimensions) did
            // the tap occur?
            Logger.Log("TAP on " + gesture.gameObject.name + " at " + hit.Point);

            // load a scene if its object is touched
            if (gesture.gameObject.tag.Contains(Constants.TAG_PLAY_OBJECT))
            {
                LoadNext(gesture.gameObject.name);
            }

            // play sidekick animation if it is touched
            else if (gesture.gameObject.tag.Contains(Constants.TAG_SIDEKICK))
            {
                // tell the sidekick to animate
                gesture.gameObject.GetComponent<Sidekick>().SidekickDo(Constants.ANIM_FLAP);

            }
        }

        /**
    	 * load next scene
    	 **/
        void LoadNext(string toLoad)
        {
            Logger.Log("attempting to load next scene...");

            switch (toLoad)
            {
            case Constants.NAME_1_PACK:
                Logger.Log(">> Loading packing scene");
                SceneManager.LoadScene(Constants.SCENE_1_PACK); // load next scene
                break;
            case Constants.NAME_2_ZOO:
                Logger.Log(">> Loading zoo scene");
                SceneManager.LoadScene(Constants.SCENE_2_ZOO); // load next scene
                break;
            case Constants.NAME_3_PICNIC:
                Logger.Log(">> Loading picnic scene");
                SceneManager.LoadScene(Constants.SCENE_3_PICNIC); // load next scene
                break;
            case Constants.NAME_4_PARK:
                Logger.Log (">> Loading park scene");
                SceneManager.LoadScene(Constants.SCENE_4_PARK);
                break;
            case Constants.NAME_5_ROOM:
                Logger.Log (">> Loading room scene");
                SceneManager.LoadScene(Constants.SCENE_5_ROOM);
                break;
            case Constants.NAME_6_BATH:
                Logger.Log (">> Loading bath scene");
                SceneManager.LoadScene(Constants.SCENE_6_BATH);
                break;
            case Constants.NAME_7_PARTY:
                Logger.Log (">> Loading party scene");
                SceneManager.LoadScene(Constants.SCENE_7_PARTY);
                break;
            case Constants.NAME_8_BYE:
                Logger.Log (">> Loading goodbye scene");
                SceneManager.LoadScene(Constants.SCENE_8_BYE);
                break;
            }

        }
    }
}


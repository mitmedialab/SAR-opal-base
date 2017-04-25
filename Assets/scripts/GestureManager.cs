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

    /// <summary>
    /// Manage gesture events and actions taken as a result of
    /// gestures (e.g., play sound, show highlight)
    /// </summary>
    public class GestureManager : MonoBehaviour
    {
        // allow touch? if false, doesn't react to touch events
        public bool allowTouch = true;
        
        // light for highlighting objects
        private GameObject highlight = null; 
        
        // main camera
        private GameObject mainCam = null;
        
        // for logging stuff
        public event LogEventHandler logEvent;
        
        // track the most recently dragged game object so we can
        // check that it stays on the screen in the Update loop
        private GameObject mostRecentlyDraggedGO = null;

        // rectangle holds the camera boundaries so we don't have
        // to re-compute these every time we want to check whether
        // an object is still within the viewable area
        private Rect cameraRect;

        // DEMO VERSION
        public bool demo = false;
        private int demospeech = 0;
        
        // STORYBOOK VERSION
        public bool story = true;
        public int pagesInStory = 0;
        
        // SOCIAL STORIES VERSION
        public bool socialStories = false;

		public MainGameController gameController;

		public int currentStoryPage = -1;
        
        /// <summary>
        /// Called on start, use to initialize stuff
        /// </summary>
        void Start ()
        {
			
			//gameController = GameObject.FindGameObjectWithTag (Constants.TAG_DIRECTOR);
            // set up light
            this.highlight = GameObject.FindGameObjectWithTag(Constants.TAG_LIGHT);
            if(this.highlight != null) {
                this.LightOff();
                Logger.Log("Got light: " + this.highlight.name);
            } else {
                Logger.LogError("ERROR: No light found");
            }
            
            // if in story mode, we need the main camera
            if (this.story)
            {
	            // find main camera
				this.mainCam = GameObject.Find("Main Camera");
				if (this.mainCam != null)
				{
					Logger.Log ("Got main camera!");
					this.mainCam.transform.position = new Vector3(0,0,-1);
				} else {
					Logger.LogError("ERROR: Couldn't find main camera!");
				}
			}

            // get the camera boundaries so we don't have to re-compute these
            // every time we want to check whether an object is still within
            // the viewable area
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(
                Camera.main.pixelWidth, Camera.main.pixelHeight));
            this.cameraRect = new Rect(bottomLeft.x,
                                       bottomLeft.y,
                                       topRight.x - bottomLeft.x,
                                       topRight.y - bottomLeft.y);
        }

        /// <summary>
        /// On enable, initialize stuff
        /// </summary>
        private void OnEnable ()
        {  
			
//            // subscribe to gesture events
			try{
            	GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);

            	foreach(GameObject go in gos) {
                	AddAndSubscribeToGestures(go, true, false);
            	}
			}catch(UnityException e){
				Logger.LogError ("No game objects found.....: " + e.Message);
			}
			//do try and catch 

            if (this.demo)
            {
                GameObject arrow = GameObject.FindGameObjectWithTag(Constants.TAG_BACK);
                if (arrow != null) AddAndSubscribeToGestures(arrow, false, false);
                
                // also subscribe for the sidekick
                
                GameObject sk = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
                // add a tap gesture component if one doesn't exist
                if (sk != null)
                {
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
                else { 
                	Logger.Log ("Gesture manager could not find sidekick!"); 
                }
            }
            
            if (this.story)
            {
				
            	// subscribe to gestures for next/previous arrows
				GameObject arrow = GameObject.FindGameObjectWithTag(Constants.TAG_BACK);
				if (arrow != null) AddAndSubscribeToGestures(arrow, false, false);
				
				GameObject arrow2 = GameObject.FindGameObjectWithTag(Constants.TAG_GO_NEXT);
				if (arrow2 != null) AddAndSubscribeToGestures(arrow2, false, false);
            }
        } 
    
        /// <summary>
        /// On destroy, disable some stuff
        /// </summary>
        private void OnDestroy ()
        {
            // unsubscribe from gesture events
            GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
            foreach(GameObject go in gos) {
                TapGesture tg = go.GetComponent<TapGesture>();
                if(tg != null) {
                    tg.Tapped -= tappedHandler;
                    Logger.Log(go.name + " unsubscribed from tap events");
                }
                TransformGesture trg = go.GetComponent<TransformGesture>();
                if(trg != null) {
                    trg.Transformed -= transformedHandler;
                    trg.TransformCompleted -= transformCompleteHandler;
                    trg.TransformStarted -= transformStartedHandler;
                    Logger.Log(go.name + " unsubscribed from pan events");
                }
                PressGesture prg = go.GetComponent<PressGesture>();
                if(prg != null) {
                    prg.Pressed -= pressedHandler;
                    Logger.Log(go.name + " unsubscribed from press events");
                }
                ReleaseGesture rg = go.GetComponent<ReleaseGesture>();
                if(rg != null) {
                    rg.Released -= releasedHandler;
                    Logger.Log(go.name + " unsubscribed from release events");
                }
                FlickGesture fg = go.GetComponent<FlickGesture>();
                if(fg != null) {
                	fg.Flicked -= flickHandler;
                	Logger.Log (go.name + " unsubscribed from flick events");
                }
                
            }
            
            //if (this.demo)
            //{
                // also unsubscribe for the sidekick
                GameObject gob = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
                if (gob != null)
                {
                    TapGesture tapg = gob.GetComponent<TapGesture>();
                    if(tapg != null) {
                        tapg.Tapped -= tappedHandler;
                        Logger.Log(gob.name + " unsubscribed from tap events");
                    }
                    
                    PressGesture prg = gob.GetComponent<PressGesture>();
                    if(prg != null) {
                        prg.Pressed -= pressedHandler;
                        Logger.Log(gob.name + " unsubscribed from press events");
                    }
                    ReleaseGesture rg = gob.GetComponent<ReleaseGesture>();
                    if(rg != null) {
                        rg.Released -= releasedHandler;
                        Logger.Log(gob.name + " unsubscribed from release events");
                    }
                }
            //}
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        public void Update()
        {
            // Check whether the most recently moved game object is within the
            // screen boundaries -- if not, move it so it is.
            if (this.mostRecentlyDraggedGO != null)
            {
                if (this.mostRecentlyDraggedGO.transform.position.x
                        < this.cameraRect.xMin
                    || this.mostRecentlyDraggedGO.transform.position.x
                        > this.cameraRect.xMax
                    || this.mostRecentlyDraggedGO.transform.position.y
                        < this.cameraRect.yMin
                    || this.mostRecentlyDraggedGO.transform.position.y
                        > this.cameraRect.yMax)
            {
                // if the game object is not within the screen boundaries, move
                // it so that it is
                Logger.Log(this.mostRecentlyDraggedGO.name +
                    " is going off screen! Keeping it on screen...");
                this.mostRecentlyDraggedGO.transform.position = new Vector3(
                    Mathf.Clamp(this.mostRecentlyDraggedGO.transform.position.x,
                        this.cameraRect.xMin, this.cameraRect.xMax),
                    Mathf.Clamp(this.mostRecentlyDraggedGO.transform.position.y,
                        this.cameraRect.yMin, this.cameraRect.yMax),
                    this.mostRecentlyDraggedGO.transform.position.z);
            }
            }

			//update current story page
			this.currentStoryPage  =(int)this.mainCam.transform.position.z+1;
        }

        /// <summary>
        /// Subscribes a play object to all relevant gestures - tap, pan,
        /// press, release
        /// </summary>
        /// <param name="go">Game object</param>
        /// <param name="draggable">If set to <c>true</c> is a draggable object.</param>
        public void AddAndSubscribeToGestures (GameObject go, bool draggable, bool storypage)
        {
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
            // if this object is draggable, handle pan events
            if(draggable) {
                // add pan gesture component if one doesn't exist yet
                TransformGesture trg = go.GetComponent<TransformGesture>();
                if(trg == null) {
                    trg = go.AddComponent<TransformGesture>();
                    trg.Type = TouchScript.Gestures.Base.TransformGestureBase.TransformType.Translation;
                }
                if(trg != null) {
                    trg.CombineTouchesInterval = 0.2f;
                    trg.TransformStarted += transformStartedHandler;
                    trg.Transformed -= transformedHandler;
                    trg.TransformCompleted += transformCompleteHandler;
                    Logger.Log(go.name + " subscribed to pan events");
                }
                
                // make sure we do have a transformer if we're draggable
                Transformer tr = go.GetComponent<Transformer>();
                if (tr == null) {
                    tr = go.AddComponent<Transformer>();
                }
            }
           
            PressGesture prg = go.GetComponent<PressGesture>();
            if(prg == null) {
                prg = go.AddComponent<PressGesture>();
            }
            if(prg != null) {
                prg.Pressed += pressedHandler;
                Logger.Log(go.name + " subscribed to press events");
            }
            ReleaseGesture rg = go.GetComponent<ReleaseGesture>();
            if(rg == null) {
                rg = go.AddComponent<ReleaseGesture>();
            }
            if(rg != null) {
                rg.Released += releasedHandler;
                Logger.Log(go.name + " subscribed to release events");
            }
           
            
            // if this is a story page, handle swipe/flick events
            if (storypage)
            {
				// add flick gesture component if one doesn't exist yet
				FlickGesture fg = go.GetComponent<FlickGesture>();
				if(fg == null) {
					fg = go.AddComponent<FlickGesture>();
				}
				if(fg != null) {
					fg.Flicked += flickHandler;
					fg.AddFriendlyGesture(tg);
					fg.MinDistance = 0.4f;
					fg.FlickTime = 0.5f;
					fg.MovementThreshold = 0.1f;
					Logger.Log(go.name + " subscribed to flick events");
				}
            }
            
            
        }

        
        #region gesture handlers
        /// <summary>
        /// Handle all tap events - log them and trigger actions in response
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void tappedHandler (object sender, EventArgs e)
        {
			
			Logger.Log("!!!!!!!!!!!!!tapped handler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");



			// get the gesture that was sent to us
            // this gesture will tell us what object was touched
            TapGesture gesture = sender as TapGesture;
            TouchHit hit;
            // get info about where the hit object was located when the gesture was
            // recognized - i.e., where on the object (in screen dimensions) did
            // the tap occur?
            if(gesture.GetTargetHitResult(out hit)) {
                // want the info as a 2D point 
                Logger.Log("TAP registered on " + gesture.gameObject.name + " at " + hit.Point);
            
                // fire event indicating that we received a message
                if(this.logEvent != null) {
                    // only send subset of msg that is actual message
                    this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                    gesture.gameObject.name, "tap", hit.Point));
                }
                
				// if this is a story, use arrows to go next/back in pages
				if(this.story && gesture.gameObject.tag.Contains(Constants.TAG_BACK))
				{
					ChangePage(Constants.PREVIOUS);
				
				}
				else if (this.story && gesture.gameObject.tag.Contains(Constants.TAG_GO_NEXT))
				{
					ChangePage(Constants.NEXT);
				}
				
                // if this is the demo app, and if the tap was on the back arrow,
                // go back to the demo intro scene
                else if(this.demo && gesture.gameObject.tag.Contains(Constants.TAG_BACK))
                {
                    SceneManager.LoadScene(Constants.SCENE_DEMO_INTRO);
                }
 
                // if this is a demo app, play sidekick animation if it is touched
                else if (this.demo && gesture.gameObject.tag.Contains(Constants.TAG_SIDEKICK))
                {   
                    // tell the sidekick to animate
                    if (Constants.DEMO_SIDEKICK_SPEECH[this.demospeech].Equals(""))
                    {
                        gesture.gameObject.GetComponent<Sidekick>().SidekickDo(Constants.ANIM_FLAP);
                    }
                    else {
                        gesture.gameObject.GetComponent<Sidekick>().SidekickSay(
                            Constants.DEMO_SIDEKICK_SPEECH[this.demospeech]);
                    }
                    this.demospeech = (this.demospeech + 1) % Constants.DEMO_SIDEKICK_SPEECH.Length;
                    
                }
                
                // trigger sound on tap
                //Logger.Log("going to play a sound for " + gesture.gameObject.name);
                //if(this.allowTouch) PlaySoundAndPulse(gesture.gameObject);
            } else {
                // this probably won't ever happen, but in case it does, we'll log it
                Logger.LogWarning("!! could not register where TAP was located!");
            }
        }


        /// <summary>
        /// Handle press events - log and turn on highlight
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void pressedHandler (object sender, EventArgs e)
        {
			Logger.Log("!!!!!!!!!!!!!presshandler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            // get the gesture that was sent to us, which will tell us 
            // which object was pressed
            PressGesture gesture = sender as PressGesture;
            TouchHit hit;
            // get info about where the hit object was located when the gesture was
            // recognized - i.e., where on the object (in screen dimensions) did
            // the press occur?
            if(gesture.GetTargetHitResult(out hit)) 
            {
                Logger.Log("PRESS on " + gesture.gameObject.name + " at " + hit.Point);
            
                // fire event to logger to log this action
                if(this.logEvent != null) 
                {
                    // if this is a social stories game, log additional info about
                    // what object was pressed
                    if (this.socialStories)
                    {
                        // log the press plus whether or not the pressed object was a YES or NO
                        // button, or whether the object was a CORRECT or INCORRECT object
                        this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                            gesture.gameObject.name, "press", hit.Point,
                            (gesture.gameObject.name.Contains("start_button") ? "START" 
                            : (gesture.gameObject.name.Contains("no_button") ? "NO"
                            // If the game object doesn't have SavedProperties component, don't add
                            // an additional message. Otherwise, log whether it was correct or not.
                            : gesture.gameObject.GetComponent<SavedProperties>() == null ? ""
                            : (gesture.gameObject.GetComponent<SavedProperties>().isCorrect ? "CORRECT"
                            : (gesture.gameObject.GetComponent<SavedProperties>().isIncorrect ? "INCORRECT"
                            : ""))))));
                    }
                    else
                    {
                        // log the press
                        this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                            gesture.gameObject.name, "press", hit.Point));
                    }
                }
            
                
				
                // move highlighting light and set active
                // don't highlight touches in a story
                if(this.allowTouch)// && !this.story)
                {
                    // send the light the z position of the pressed object because
                    // the 'hit2d' point doesn't have the right z position (is always
                    // just zero)
                    //LightOn(1, new Vector3(hit2d.Point.x, hit2d.Point.y, 
                    //    gesture.gameObject.transform.position.z));
                    
                    // NEW trying out centering light behind the pressed object,
                    // regardless of where on the pressed object the touch was
                    LightOn(1, gesture.gameObject.transform.position);
                }
                    
                // trigger sound on press
                if(this.allowTouch && gesture.gameObject.tag.Contains(Constants.TAG_PLAY_OBJECT)) 
                {
                    Logger.Log("going to play a sound for " + gesture.gameObject.name);
                    PlaySoundAndPulse(gesture.gameObject);
                }

            } else {
                // this probably won't ever happen, but in case it does, we'll log it
                Logger.LogWarning("!! could not register where PRESS was located!");
            }
        }

        /// <summary>
        /// Handle released events - when object released, stop highlighting object
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void releasedHandler (object sender, EventArgs e)
        {
			Logger.Log("PRESS COMPLETE");
            if (this.allowTouch)// && !this.story)
            {
            	LightOff();
            }
        
            // fire event indicating that we received a message
            if(this.logEvent != null) {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                        "", "release", null));
            }
        }
     
     
        /// <summary>
        /// Handle all pan/drag events - log them, trigger actions in response
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void transformStartedHandler (object sender, EventArgs e)
        {
            // get the gesture that was sent to us, which will tell us 
            // which object was being dragged
            TransformGesture gesture = sender as TransformGesture;
            TouchHit hit;
            // get info about where the hit object was located when the gesture was
            // recognized - i.e., where on the object (in screen dimensions) did
            // the drag occur?
            if(gesture.GetTargetHitResult(out hit)) {
                Logger.Log("PAN STARTED on " + gesture.gameObject.name + " at " + hit.Point);
                // move this game object with the drag
                // note that hit2d.Point sets the z position to 0! does not keep
                // track what the z position actually was! so we adjust for this when
                // we check the allowed moves
                if(this.allowTouch)
                {
                    // The transformer component moves object on drag events, but we
                    // have to check that the object stays within the screen boundaries.
                    // This happens in the Update loop; here we just save the most
                    // recently dragged object.
                    this.mostRecentlyDraggedGO = gesture.gameObject;

                   // the transformer component moves object on pan events
                    // send the light the z position of the panned object so it
                    // shows up in the right plane 
                    LightOn(1, new Vector3(hit.Point.x, hit.Point.y, 
                                           gesture.gameObject.transform.position.z));
                }
                // fire event indicating that we received a message
                if(this.logEvent != null) {
                    // only send subset of msg that is actual message
                    this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                                                     gesture.gameObject.name, "pan", hit.Point));
                }
                
            } else {
                // this probably won't ever happen, but in case it does, we'll log it
                Logger.LogWarning("could not register where PAN was located!");
            }
            
        }
     
        /// <summary>
        /// Handle all pan/drag events - log them, trigger actions in response
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void transformedHandler (object sender, EventArgs e)
        {
            // get the gesture that was sent to us, which will tell us 
            // which object was being dragged
            TransformGesture gesture = sender as TransformGesture;
            TouchHit hit;
            // get info about where the hit object was located when the gesture was
            // recognized - i.e., where on the object (in screen dimensions) did
            // the drag occur?
            
            if(gesture.GetTargetHitResult(out hit)) {
                Logger.Log("PAN on " + gesture.gameObject.name + " at " + hit.Point);
                // move this game object with the drag
                if(this.allowTouch)
                {
                    // send the light the z position of the panned object so it shows
                    // up in the right plane
                    LightOn(1, new Vector3(hit.Point.x, hit.Point.y, 
                                           gesture.gameObject.transform.position.z));
                // fire event indicating that we received a message
                }
                if(this.logEvent != null) {
                    // only send subset of msg that is actual message
                    // note that the hit2d.Point may not have the correct z position
                    this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                        gesture.gameObject.name, "pan", hit.Point));
                }

            } else {
                // this probably won't ever happen, but in case it does, we'll log it
                Logger.LogWarning("could not register where PAN was located!");
            }

        }
        
        
        /// <summary>
        /// Handle pan complete events - when drag is done, stop highlighting object 
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void transformCompleteHandler (object sender, EventArgs e)
        {
            Logger.Log("PAN COMPLETE");
            LightOff();
        
            // fire event indicating that an action occurred
            if(this.logEvent != null) {
                // only send relevant data
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                        "", "pancomplete", null));
            }      
        }
        
        /// <summary>
        /// Handle flick events
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
		void flickHandler (object sender, EventArgs e)
		{
			// get the gesture that was sent to us, which will tell us 
			// which object was flicked
			FlickGesture gesture = sender as FlickGesture;
			
			TouchHit hit;
			// get info about where the hit object was located when the gesture was
			// recognized - i.e., where on the object (in screen dimensions) did
			// the flick occur?
			if(gesture.GetTargetHitResult(out hit)) {
				Logger.Log("FLICK on " + gesture.gameObject.name + " at " + hit.Point);
				
				// fire event to logger to log this action
				if(this.logEvent != null) {
					// log the flick
					this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
					              gesture.gameObject.name, "flick", hit.Point));
				}
				
				
				if(this.allowTouch)
				{	
					// if flick/swipe was to the right, advance page
					if (gesture.ScreenFlickVector.x < 0)
					{
						ChangePage(Constants.NEXT);
					}
					
					// if to the left, go back a page
					else if (gesture.ScreenFlickVector.x > 0)
					{
						ChangePage(Constants.PREVIOUS);
					}
				}
				
				// trigger sound on flick as feedback?
				//if(this.allowTouch && !gesture.gameObject.tag.Contains(Constants.TAG_SIDEKICK)) 
				//{
				//	Logger.Log("going to play a sound for " + gesture.gameObject.name);
				//	PlaySoundAndPulse(gesture.gameObject);
				//}
				
			} else {
				// this probably won't ever happen, but in case it does, we'll log it
				Logger.LogWarning("!! could not register where FLICK was located!");
			}
			
		}
		
        
    #endregion
    
    #region utilities
    
        /// <summary>
        /// Sets light object active in the specified position and with the specified scale
        /// </summary>
        /// <param name="posn">Posn.</param>
        public void LightOn (Vector3 posn)
        {
            LightOn(1, posn);
        }

        public void LightOn (int scaleBy, Vector3 posn)
        {
            if(this.highlight != null && this.highlight.GetComponent<Renderer>() != null) 
            {
                this.highlight.GetComponent<Renderer>().enabled = true;
                this.highlight.transform.position = new Vector3(posn.x, posn.y, posn.z + 1);
                Vector3 sc = this.highlight.transform.localScale;
                sc.x *= scaleBy;
                this.highlight.transform.localScale = sc;
            } else {
                Logger.Log("Tried to turn light on ... but light is null!");
            }
        }

        public void LightOn (Vector3 scale, Vector3 posn)
        {
            if(this.highlight != null && this.highlight.GetComponent<Renderer>() != null) 
            {
                this.highlight.GetComponent<Renderer>().enabled = true;
                this.highlight.transform.position = new Vector3(posn.x, posn.y, posn.z + 1);
                this.highlight.transform.localScale = scale;
            } else {
                Logger.Log("Tried to turn light on ... but light is null!");
            }
        }   
   
        /// Deactivates light, returns to specified scale   
        public void LightOff ()
        {
            LightOff(1);
        }
        
        /// <summary>
        /// Deactivates light, returns to specified scale
        /// </summary>
        /// <param name="scaleBy">Scale by.</param>
        public void LightOff (int scaleBy)
        {
            if(this.highlight != null && this.highlight.GetComponent<Renderer>() != null) 
            {
                Vector3 sc = this.highlight.transform.localScale;
                sc.x /= scaleBy;
                this.highlight.transform.localScale = sc;
                this.highlight.GetComponent<Renderer>().enabled = false;
            } else {
                Logger.Log("Tried to turn light off ... but light is null!");
            }
        }
  
		/// <summary>
		/// Goes to a specific page in the story book
		/// </summary>
		/// <param name="pageNum"> the specific page number </param>
		public void GoToPage(int pageNum){
			if (pageNum <= this.pagesInStory && pageNum >= 0) {
				this.mainCam.transform.position = new Vector3 (0, 0, pageNum - 1);
				GameObject tb = GameObject.FindGameObjectWithTag (Constants.TAG_BACK);
				GameObject tn = GameObject.FindGameObjectWithTag (Constants.TAG_GO_NEXT);
				tb.transform.position = new Vector3 (tb.transform.position.x, tb.transform.position.y, pageNum);
				tn.transform.position = new Vector3 (tn.transform.position.x, tn.transform.position.y, pageNum);
			} else {
				Logger.LogError ("invalid page number: "+pageNum.ToString());
			}
		}

		public void GoToFirstPage(){
			this.mainCam.transform.position = new Vector3(0,0,-1);
			GameObject tb = GameObject.FindGameObjectWithTag(Constants.TAG_BACK);
			GameObject tn = GameObject.FindGameObjectWithTag(Constants.TAG_GO_NEXT);
			tb.transform.position = new Vector3(tb.transform.position.x,tb.transform.position.y,0);
			tn.transform.position = new Vector3(tn.transform.position.x,tn.transform.position.y,0);
		}
  
  		/// <summary>
  		/// Changes the page.
  		/// </summary>
  		/// <param name="next">If set to <c>true</c> next page, otherwise, previous page</param>
  		public void ChangePage (bool next)
  		{
			Logger.Log("!!!!!!!!!!!!!Change page!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			// fire event to logger to log this action
			if(this.logEvent != null) {
				// log the flick
				this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
				                                 "", "flick", new Vector3(0,0,0)));
			}

			//before changing the page, hide the current page
			gameController.storyImageObjects[this.currentStoryPage].GetComponent<Renderer>().enabled = false;
			
			if(this.allowTouch)
			{	
				// if flick/swipe was to the right, advance page
				if (next)
				{
					if (this.mainCam != null) 
					{
						Logger.Log ("swiping right...");
						// don't go past end of story
						if (this.mainCam.transform.position.z < this.pagesInStory-1)
						{
							
							this.mainCam.transform.Translate(new Vector3(0,0,1));
							GameObject.FindGameObjectWithTag(Constants.TAG_GO_NEXT).transform.Translate(new Vector3(0,0,1));
							GameObject.FindGameObjectWithTag(Constants.TAG_BACK).transform.Translate(new Vector3(0,0,1));
						}
						else // this is the end page, loop back to beginning of story
						{
							//GoToFirstPage ();
						}
					}
					else {
						Logger.Log ("no main cam! can't change page!");
					}
				}
				
				else
                {
                    if (this.mainCam != null) 
                    {
                        Logger.Log("swiping left...");
                        // don't go before start of story
                        if (this.mainCam.transform.position.z > -1)
                        {
                            this.mainCam.transform.Translate(new Vector3(0,0,-1));
							GameObject.FindGameObjectWithTag(Constants.TAG_BACK).transform.Translate(new Vector3(0,0,-1));
							GameObject.FindGameObjectWithTag(Constants.TAG_GO_NEXT).transform.Translate(new Vector3(0,0,-1));
                        }
                        
                    }
					else {
						Logger.Log ("no main cam! can't change page!");
                    }
                }
            }

			gameController.sendStoryState2ROS ();
			//before changing the page, hide the current page
			gameController.storyImageObjects[(int)this.mainCam.transform.position.z+1].GetComponent<Renderer>().enabled = true;
  		}
  
  
        /// <summary>
        /// Plays the first sound attached to the object, if one exists 
        /// </summary>
        /// <returns><c>true</c>, if sound was played, <c>false</c> otherwise.</returns>
        /// <param name="go">Game object with sound to play.</param>
        private bool PlaySound (GameObject go)
        { 
            // play audio clip if this game object has a clip to play
            AudioSource auds = go.GetComponent<AudioSource>();
            if(auds != null && auds.clip != null) {
                Logger.Log("playing clip for object " + go.name);

                // play the audio clip attached to the game object
                if(!go.GetComponent<AudioSource>().isPlaying)
                    go.GetComponent<AudioSource>().Play();
                
                return true;   
            } else {
                Logger.Log("no sound found for " + go.name + "!");
                return false;
            }
        }
        
        /// <summary>
        /// Plays the first sound attached to an object, if one exists, while
        /// also pulsing the object's size (to draw attention to it)
        /// </summary>
        /// <param name="go">Go.</param>
        private void PlaySoundAndPulse (GameObject go)
        {
            if(go != null) {
                // play a sound, if it exists and is not already playing
                // and also pulse size
                if(PlaySound(go) && (go.GetComponent<AudioSource>() != null) && !go.GetComponent<AudioSource>().isPlaying)
                    go.GetComponent<GrowShrinkBehavior>().ScaleUpOnce();
            }
        }
    
#endregion
    
    }
}
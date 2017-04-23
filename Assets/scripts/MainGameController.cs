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
using System;
using System.Collections.Generic;
using TouchScript.Behaviors;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace opal
{



    /// <summary>
    /// The SAR-opal-base game main controller. Orchestrates everything: 
    /// sets up to receive input via ROS, initializes scenes and creates 
    /// game objecgs based on that input, deals with touch events and
    /// other tablet-specific things.
    /// </summary>
	/// 
	/// 
	/// 
    public class MainGameController : MonoBehaviour
    {

        // --------------- FLAGS ---------------
        // DEMO VERSION
        private bool demo = false;

        // STORYBOOK VERSION
        private bool story = true;
        /// <summary>
        /// The pages in story.
        /// </summary>
        public int pagesInStory = 0;

        // SOCIAL STORIES VERSION
        private bool socialStories = false;
        private List<GameObject> incorrectFeedback;
        private GameObject correctFeedback;


        /// <summary>
        /// The width of the slot.
        /// </summary>
        public float slotWidth = 1;
        /// <summary>
        /// The width of the answer slot.
        /// </summary>
        public float answerSlotWidth = 1;
        /// <summary>
        /// The scenes in order.
        /// </summary>
        public bool scenesInOrder = true;
        // --------------------------------------

        // gesture manager
        private GestureManager gestureManager = null;
        
        // sidekick
        private Sidekick sidekickScript = null;
        
        // rosbridge websocket client
        private RosbridgeWebSocketClient clientSocket = null;
    
        // actions for main thread, because the network messages from the
        // websocket can come in on another thread
        readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();
    
        // for logging stuff
        /// <summary>
        /// Occurs when log event.
        /// </summary>
        public event LogEventHandler logEvent;
        
        // fader for fading out the screen
        //private GameObject fader = null; 

        // This flag indicates whether we should scale graphics based on the
        // screen height (if true) or width (if false).
        private bool scaleToHeight = false;


        // config
        private GameConfig gameConfig;

		// loaded story information
		public StoryInfo storyInfo;
		public Sprite[] storyImageSprites;
		public float targetTime = 1.0f;
		public string storyFolder;






        /// <summary>
        /// Called first, use to initialize stuff
        /// </summary>
        void Awake()
        {
            if (this.demo) Logger.Log("--- RUNNING IN DEMO MODE ---");
            if (this.story) Logger.Log ("--- RUNNING IN STORYBOOK MODE ---");
            if (this.socialStories) Logger.Log("--- RUNNING IN SOCIAL STORIES MODE ---");
        
            string path = "";

			//set the default story name here
			this.storyInfo.StoryName="None";
			this.storyInfo.reload = false;
			this.storyInfo.touch_enabled = true;
			this.storyInfo.buttons_shown = true;


            // Scale all camera views to match the screen size of whatever
            // device we're running on.
            this.scaleToHeight = Utilities.ScaleCameraViewToScreen();

            // find the config file
            #if UNITY_ANDROID
            path = Constants.CONFIG_PATH_ANDROID + Constants.OPAL_CONFIG;
            Logger.Log("trying android path: " + path);
			//string storyPath = "/Users/huilichen/Downloads/graphics_without_text/images/"+storyName;
			storyFolder="/sdcard/edu.mit.media.prg.sar.opal.base/";
            #endif
            
            #if UNITY_EDITOR
            path = Application.dataPath + Constants.CONFIG_PATH_OSX + Constants.OPAL_CONFIG;
            Logger.Log("trying os x path: " + path);
			storyFolder = "/Users/huilichen/Downloads/graphics_without_text/images/";

            #endif

            #if UNITY_STANDALONE_LINUX
            path = Application.dataPath + Constants.CONFIG_PATH_LINUX + Constants.OPAL_CONFIG;
            Logger.Log("trying linux path: " + path);
            #endif
            
            // read config file
            if(!Utilities.ParseConfig(path, out gameConfig)) {
                Logger.LogWarning("Could not read config file! Will try default "
					+ "values of toucan=true, server IP=192.168.1.103, port=9090, "
                    + "opal_action_topic=" + Constants.ACTION_ROSTOPIC
                    + ", opal_audio_topic=" + Constants.AUDIO_ROSTOPIC
                    + ", opal_command_topic=" + Constants.CMD_ROSTOPIC
                    + ", opal_log_topic=" + Constants.LOG_ROSTOPIC
                    + ", opal_scene_topic=" + Constants.SCENE_ROSTOPIC
                    + ".");
            }
            else {
                Logger.Log("Got game config!");
            }

            // find gesture manager
            FindGestureManager(); 
            this.gestureManager.logEvent += new LogEventHandler(HandleLogEvent);
            this.logEvent += new LogEventHandler(HandleLogEvent);
 
            // share flags with everyone else
            this.gestureManager.demo = this.demo;
            this.gestureManager.story = this.story;
            this.gestureManager.socialStories = this.socialStories;
                        
            // find our sidekick
            if (!this.story)
            {
	            GameObject sidekick = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
	            if(sidekick == null) {
	                Logger.LogError("ERROR: Could not find sidekick!");
	            } else {
	                Logger.Log("Got sidekick");
	                if(this.gameConfig.sidekick) {
	                    // add sidekick's gestures
	                    this.gestureManager.AddAndSubscribeToGestures(sidekick, false, false);
	                    
	                    // get sidekick's script
	                    this.sidekickScript = (Sidekick)sidekick.GetComponent<Sidekick>();
	                    if(this.sidekickScript == null) 
                        {
	                        Logger.LogError("ERROR: Could not get sidekick script!");
	                    } else {
	                        Logger.Log("Got sidekick script");
	                        this.sidekickScript.donePlayingEvent += new DonePlayingEventHandler(HandleDonePlayingAudioEvent);
	                    }
	                }
	                else {
	                    // we don't have a sidekick in this game, set as inactive
	                    Logger.Log("Don't need sidekick... disabling");
	                    sidekick.SetActive(false);
	                    
	                    // try to disable the sidekick's highlight as well
	                    GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK_LIGHT).SetActive(false);
	                }
                }
            }
 
            // set up fader
            // NOTE right now we're just using one fader that fades out all but the
            // toucan - but in the unity editor there's an unused 'fader_all' that
            // can fade out everything including the toucan, just switch this tag
            // to "TAG_FADER_ALL" to use that fader instead!


//            	this.fader = GameObject.FindGameObjectWithTag(Constants.TAG_FADER);
//				this.fader.GetComponent<Renderer>().enabled = false;
//
//            if(this.fader != null) {
//                	
//                	Logger.Log("Got fader: " + this.fader.name);
//            } else {
//                	Logger.LogError("ERROR: No fader found");
//            }

            // subscribe to all log events from existing play objects 
            // with collision managers
            this.SubscribeToLogEvents(new string[] { Constants.TAG_PLAY_OBJECT });

            if (this.demo)
            {
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(
                    Constants.TAG_PLAY_OBJECT))
                {
                    this.gestureManager.AddAndSubscribeToGestures(go, true, false);
                }
            }

        }

        /// <summary>
        /// Called on start, use to initialize stuff
        /// </summary>
        void Start()
        {
            // set up rosbridge websocket client
            // note: does not attempt to reconnect if connection fails!
            // demo mode does not use ROS!
            if(this.clientSocket == null && !this.demo)
            {	
                // load file

                if (this.gameConfig.server.Equals("") || this.gameConfig.port.Equals("")) {
                    Logger.LogWarning("Do not have opal configuration... trying "
						+ "hardcoded IP 192.168.1.103 and port 9090");
                    this.clientSocket = new RosbridgeWebSocketClient(
					"192.168.1.103",// server, // can pass hostname or IP address
                    "9090"); //port);   
                } else {
                    this.clientSocket = new RosbridgeWebSocketClient(
                    this.gameConfig.server, // can pass hostname or IP address
                    this.gameConfig.port);  
                }
            
                if (this.clientSocket.SetupSocket())
                {
                    this.clientSocket.receivedMsgEvent += 
                    new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);
                    
					Logger.LogError("!!!!!!!!!!!!!!!!!!! after received meessage event handler!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    
                    // advertise that we will publish opal action messages                
                    if (this.gameConfig.opalActionTopic == "")
                    {
                        Logger.LogWarning("Do not have opal configuration... trying "
                                          + "default topic " + Constants.DEFAULT_ACTION_ROSTOPIC);
                        Constants.ACTION_ROSTOPIC = Constants.DEFAULT_ACTION_ROSTOPIC;
                    }
                    else 
                    {   
                        Constants.ACTION_ROSTOPIC = this.gameConfig.opalActionTopic;
                    }
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                        Constants.ACTION_ROSTOPIC, Constants.ACTION_ROSMSG_TYPE));
                    
                    // advertise that we will publish opal audio messages
                    if (this.gameConfig.opalAudioTopic == "")
                    {
                        Logger.LogWarning("Do not have opal configuration... trying "
                                          + "default topic " + Constants.DEFAULT_AUDIO_ROSTOPIC);
                        Constants.AUDIO_ROSTOPIC = Constants.DEFAULT_AUDIO_ROSTOPIC;
                    }
                    else 
                    {
                        Constants.AUDIO_ROSTOPIC = this.gameConfig.opalAudioTopic;
                    }
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                        Constants.AUDIO_ROSTOPIC, Constants.AUDIO_ROSMSG_TYPE));
                    
                    // advertise that we will subscribe to opal command messages
                    if (this.gameConfig.opalCommandTopic == "")
                    {
                        Logger.LogWarning("Do not have opal configuration... trying "
                                          + "default topic " + Constants.DEFAULT_CMD_ROSTOPIC);
                        
                        Constants.CMD_ROSTOPIC = Constants.DEFAULT_CMD_ROSTOPIC;
                    }
                    else 
                    {
                        Constants.CMD_ROSTOPIC = this.gameConfig.opalCommandTopic;
                    }
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
                        Constants.CMD_ROSTOPIC, Constants.CMD_ROSMSG_TYPE));
                    
                    // advertise that we will publish opal log messages
                    if (this.gameConfig.opalLogTopic == "")
                    {
                        Logger.LogWarning("Do not have opal configuration... trying "
                                          + "default topic " + Constants.DEFAULT_LOG_ROSTOPIC);
                        Constants.LOG_ROSTOPIC = Constants.DEFAULT_LOG_ROSTOPIC;
                    }
                    else 
                    {
                        Constants.LOG_ROSTOPIC = this.gameConfig.opalLogTopic;
                    }
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                        Constants.LOG_ROSTOPIC, Constants.LOG_ROSMSG_TYPE));
                    
                    // advertise that we will publish opal scene messages
                    if (this.gameConfig.opalSceneTopic == "")
                    {
                        Logger.LogWarning("Do not have opal configuration... trying "
                                          + "default topic " + Constants.DEFAULT_SCENE_ROSTOPIC);
                        Constants.SCENE_ROSTOPIC = Constants.DEFAULT_SCENE_ROSTOPIC;
                    }
                    else 
                    {
                        Constants.SCENE_ROSTOPIC = this.gameConfig.opalSceneTopic;
                    }
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
                        Constants.SCENE_ROSTOPIC, Constants.SCENE_ROSMSG_TYPE));
                    
                    // publish log message to opal log topic
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                        Constants.LOG_ROSTOPIC, "Opal game checking in!"));

					Logger.LogError("!!!!!!!!!!!!!!!!!!! setting up all ros topics !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

					// publicsh storybook basic information (e.g., current story page, which state, end page of book, # of pages)
					this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
						Constants.STORYBOOK_ROSTOPIC, Constants.STORYBOOK_ROSMSG_TYPE));

                }
                else {
                    Logger.LogError("Could not set up websocket!");
                }            
            // register log callback for Logger.Log calls
            Application.logMessageReceivedThreaded += HandleApplicationLogMessageReceived;
 			
            }

//			storyPanel = GameObject.FindGameObjectWithTag ("Panel2");
//			GameObject goback = GameObject.FindGameObjectWithTag (Constants.TAG_BACK);
//			GameObject gonext = GameObject.FindGameObjectWithTag (Constants.TAG_GO_NEXT);
//			goback.transform.position = new 

        }

	




        /** On enable, initialize stuff */
        private void OnEnable ()
        {
        	
        }

        /** On disable, disable some stuff */
        private void OnDestroy ()
        {
            // unsubscribe from log events
            this.gestureManager.logEvent -= new LogEventHandler(HandleLogEvent);
            this.logEvent -= new LogEventHandler(HandleLogEvent);
            
            // unsubscribe from sidekick audio events
            if (this.sidekickScript != null)
            {
                this.sidekickScript.donePlayingEvent -= new DonePlayingEventHandler(HandleDonePlayingAudioEvent);
            }

            // unsubscribe from Unity Logger.Log events
            Application.logMessageReceivedThreaded -= HandleApplicationLogMessageReceived;
        
            // close websocket
            if(this.clientSocket != null) {
                this.clientSocket.CloseSocket();
    
                // unsubscribe from received message events
                this.clientSocket.receivedMsgEvent -= HandleClientSocketReceivedMsgEvent;
            }
         
            Logger.Log("destroyed main game controller");
        }
    
        
        /// <summary>
        /// update is called once per frame
        /// </summary>
        void Update ()
        {
			
            // if user presses escape or 'back' button on android, exit program
            if(Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        
            // dispatch stuff on main thread (usually stuff in response to 
            // messages received from the websocket on another thread)
            while(ExecuteOnMainThread.Count > 0) {
                Logger.Log("Invoking actions on main thread....");
                try {
                    ExecuteOnMainThread.Dequeue().Invoke(); 
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error when invoking actions on main thread!\n" + ex);
                }
            }

			//update storybook current state
			this.storyInfo.current_page=gestureManager.currentStoryPage;

			//send storybook current state to ROS
			targetTime -= Time.deltaTime;

			if (targetTime <= 0.0f && storyInfo.StoryName!="None") {
				sendStoryState2ROS ();
				targetTime = 1.0f;
			}
        }

        /// <summary>
        /// Subscribes to log events.
        /// </summary>
        protected void SubscribeToLogEvents(string[] tags)
        {
            // subscribe to log events for all playobjects in scene
            foreach(string tag in tags) {
				try{
                	GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
                if(gos.Length == 0)
                    continue;
                foreach(GameObject go in gos) 
                {
                    // add collision manager so we get trigger enter/exit events
                    CollisionManager cm = go.GetComponent<CollisionManager>();
                    if (cm != null)
                    {
                        // subscribe to log events from the collision manager
                        cm.logEvent += new LogEventHandler(HandleLogEvent);
                    }
                    // if there is no collision manager, then we don't care about
                    // subscribing to events from it - this is really just to make
                    // sure we subscribe to log events from objects created with 
                    // the graphical unity editor that had collision managers
                    // manually added
                }
				}catch(UnityException e){
					Logger.LogError ("!!!!!!!no game objects found.....: " + e.Message);
				}
            }
        }

        #region Instantiate game objects

        /// <summary>
        /// Instantiate a new game object with the specified properties
        /// </summary>
        /// <param name="pops">properties of the play object.</param>
        public void InstantiatePlayObject (PlayObjectProperties pops, Sprite spri)
        {
            GameObject go = new GameObject();

            // set object name
            go.name = (pops.Name() != "") ? Path.GetFileNameWithoutExtension(pops.Name()) 
                : UnityEngine.Random.value.ToString();
            Logger.Log("Creating new play object: " + pops.Name() + ", with properties: "
                + pops.ToString());

            // set layer based on whether the object is draggable or not
            go.layer = (pops.draggable ? Constants.LAYER_MOVEABLES : Constants.LAYER_STATICS);

            // load sprite/image for object
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            // if we were not given a sprite for this object, try loading one
            if (spri == null)
            {
                // don't need file extension to load from resources folder -- strip if it exists
                Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH 
                    // if this is a social stories game, load from the directory of
                    // social stories graphics
                    + (this.socialStories ? Constants.SOCIAL_STORY_FILE_PATH : "")
                    + Path.ChangeExtension(pops.Name(), null));
                if(sprite == null)
                {
                    Logger.LogWarning("Could not load sprite from Resources: " 
                        + Constants.GRAPHICS_FILE_PATH  
                        + (this.socialStories ? Constants.SOCIAL_STORY_FILE_PATH : "")
                        + pops.Name()
                        + "\nGoing to try file path...");
                    
                    // TODO add filepath to pops! don't use Name
                    sprite = Utilities.LoadSpriteFromFile(pops.Name());
                    if(sprite == null)
                    {
                        Logger.LogError("Could not load sprite from file path: " 
                                        + pops.Name());
                        // still don't have image - failed to load!
                        // delete game object and return
                        Logger.LogError("Could not load sprite: " + pops.Name());
                        GameObject.Destroy(go);
                        return;
                    }
                }
                
                // got sprite!
                spriteRenderer.sprite = sprite; 
            }
            // otherwise, we were given a sprite, try using that one
            else
            {
                spriteRenderer.sprite = spri;
            }
            
            // if a slot number was assigned and we're in a social stories game,
            // use that slot to figure out where to put the object
            if (pops.Slot() != -1 && this.socialStories)
            {
                // get either the scene slot or answer slot object
                GameObject slot = GameObject.Find((pops.isAnswerSlot ? Constants.ANSWER_SLOT : 
                    Constants.SCENE_SLOT) + (pops.Slot() - 1)); // slots 1-indexed
                if (slot != null)
                {
                    Logger.Log("Slot found: " + slot.name + " at position " 
                            + slot.transform.position + " -- putting object here.");
                    go.transform.position = new Vector3(slot.transform.position.x,
                                                        slot.transform.position.y,
                                                        Constants.Z_PLAY_OBJECT);
                    // make slot visible again
                    slot.GetComponent<Renderer>().enabled = true;

                    // set scale of sprite
                    // scale slot to one portion of the screen width, using the saved
                    // width of a slot 
                    if (pops.isAnswerSlot)
                    {
                        go.transform.localScale = new Vector3(
                            this.answerSlotWidth / spriteRenderer.sprite.bounds.size.x,
                            this.answerSlotWidth / spriteRenderer.sprite.bounds.size.y,
                            this.answerSlotWidth / spriteRenderer.sprite.bounds.size.z);
                    }
                    // use scene slot width, not answer slot width, to scale
                    else
                    {
                        go.transform.localScale = new Vector3(
                            this.slotWidth / spriteRenderer.sprite.bounds.size.x,
                            this.slotWidth / spriteRenderer.sprite.bounds.size.y,
                            this.slotWidth / spriteRenderer.sprite.bounds.size.z);
                    }
                }
                else
                {
                    Logger.LogError("Tried to get position and scale of scene or answer slot so we"
                        + " could load an object at that position, but slot was null! Defaulting"
                        + " to position (0,0,0) and scale (1,1,1).");
                        go.transform.position = Vector3.zero;
                        go.transform.localScale = new Vector3(1,1,1);
                }
            }
            else
            {
                // move object to specified initial position 
                go.transform.position = pops.InitPosition();

                // set the scale of the sprite to the specified scale
                go.transform.localScale = pops.Scale();
            }
            
            // save the initial position in case we need to reset this object later
            // can save other stuff in these properties too!
            SavedProperties sp = go.AddComponent<SavedProperties>();
            sp.initialPosition = go.transform.position;
            
            if (this.socialStories)
            {
                sp.correctSlot = pops.CorrectSlot();
                sp.isCorrect = pops.isCorrect;
                sp.isIncorrect = pops.isIncorrect;
            }
            
            // set tag
            go.tag = pops.Tag();

            // if tag is FEEDBACK, keep reference and set as invisible
            if (go.tag.Equals(Constants.TAG_CORRECT_FEEDBACK)
                && go.GetComponent<Renderer>() != null)
            {
                this.correctFeedback = go;
                go.GetComponent<Renderer>().enabled = false;
            }
            else if (go.tag.Equals(Constants.TAG_INCORRECT_FEEDBACK)
                && go.GetComponent<Renderer>() != null)
            {
                // this list is cleared when the scene is cleared
                if (this.incorrectFeedback == null) 
                {
                    this.incorrectFeedback = new List<GameObject>();
                }
                this.incorrectFeedback.Add(go);
                go.GetComponent<Renderer>().enabled = false;
            }

            // if this is an answer feedback slot graphic, we set it invisible
            // until the associated answer is loaded
            if (go.tag.Equals(Constants.TAG_ANSWER_SLOT))
                go.GetComponent<Renderer>().enabled = false;

            // load audio - add an audio source component to the object if there
            // is an audio file to load
            if(pops.AudioFile() != null) {
                AudioSource audioSource = go.AddComponent<AudioSource>();
                try {
                    // to load a sound file this way, the sound file needs to be in an existing 
                    // Assets/Resources folder or subfolder 
                    audioSource.clip = Resources.Load(Constants.AUDIO_FILE_PATH + 
                        pops.AudioFile()) as AudioClip;
                } catch(UnityException e) {
                    Logger.LogError("Could not load audio: " + pops.AudioFile() + "\n" + e);
                }
                audioSource.loop = false;
                audioSource.playOnAwake = false;
            }
            
            if (pops.draggable)
            {
                // add rigidbody if this is a draggable object
                Rigidbody2D rb2d = go.AddComponent<Rigidbody2D>();
                // remove object from physics engine's control, because we don't want
                // the object to move with gravity, forces, etc - we do the moving
                rb2d.isKinematic = true; 
                // don't want gravity, otherwise objects will fall
                // though with the isKinematic flag set this may not matter
                rb2d.gravityScale = 0; 
                // set collision detection to 'continuous' instead of discrete
                rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                
                // add collision manager so we get trigger enter/exit events
                CollisionManager cm = go.AddComponent<CollisionManager>();
                // subscribe to log events from the collision manager
                cm.logEvent += new LogEventHandler(HandleLogEvent);
                // pass on info about whether scenes are in order or not
                cm.scenesInOrder = this.scenesInOrder;
                
                // and add transformer so it automatically moves on drag
                // note that the AddAndSubscribeToGestures function also
                // checks to add a transformer if there isn't one if the 
                // object is supposed to be draggable
                Transformer trans = go.GetComponent<Transformer>();
                if (trans == null) 
                {
                    trans = go.AddComponent<Transformer>();
                    trans.enabled = true;
                }
            }
            // if the object is not draggable, then we don't need a rigidbody because
            // it is a static object (won't move even if there are collisions)

            // add circle collider - used in detecting touches and dragging.
            // if the collider on the object is too small, touches won't 
            // collide very often or very well, and movement (e.g. drags)
            // will be choppy and weird. don't set as trigger so that this 
            // collider doesn't trigger enter/exit events (because it is bigger
            // than the object and we'd get too many collisions)
            // !! this is now obselete because we're using the transformer that
            // came with TouchScript which works great even with a small collider
            // - so clearly something in how we were dragging stuff before was just
            // wrong, and we can now not bother with the circle collider
            //CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
            //cc.radius = .7f;
            
            // add polygon collider that matches shape of object and set as a 
            // trigger so enter/exit events fire when this collider is hit
            PolygonCollider2D pc = go.AddComponent<PolygonCollider2D>();
            pc.isTrigger = true;
            
            // add and subscribe to gestures
            if(this.gestureManager == null) {
                Logger.Log("ERROR no gesture manager");
                FindGestureManager();
            }
            
            try {
                // add gestures and register to get event notifications
                this.gestureManager.AddAndSubscribeToGestures(go, pops.draggable, false);
            }
            catch (Exception e)
            {
                Logger.LogError("Tried to subscribe to gestures but failed! " + e);
            }
           
            // add pulsing behavior (draws attention to actionable objects)
            // go.AddComponent<GrowShrinkBehavior>();
            // Removing this because it messes with collision detection when
            // objects are close to each other (continuously colliding/uncolliding)
            // go.GetComponent<GrowShrinkBehavior>().StartPulsing();
        
            // HACK to get drag to work right after object is loaded
            // for some reason if we disable then enable the Transformer2D 
            // component, drag will work. if we don't, then the Transformer2D 
            // component will be enabled but dragging will do nothing. not 
            // sure why...
            // TODO is this still needed? trying without!
            //if (go.GetComponent<Transformer>() != null)
            //{
            //    go.GetComponent<Transformer>().enabled = false;
            //}
            if (go.GetComponent<Transformer>() != null)
            {
                go.GetComponent<Transformer>().enabled = true;
            }
        }
    
        /// <summary>
        /// Instantiates a background image object
        /// </summary>
        /// <param name="bops">properties of the background image object to load</param>
        public void InstantiateBackground (BackgroundObjectProperties bops, Sprite spri)
        {
            // remove previous background if there was one
            this.DestroyObjectsByTag(new string[] {Constants.TAG_BACKGROUND});
    
            // now make a new background
            GameObject go = new GameObject();
        
            // set object name
            go.name = (bops.Name() != "") ? bops.Name() : UnityEngine.Random.value.ToString();
            Logger.Log("Creating new background: " + bops.Name());
        
            // set tag
            go.tag = Constants.TAG_BACKGROUND;

            // set layer
            go.layer = Constants.LAYER_STATICS;
        
            // move object to initial position 
            // if background, set at z=2
            // if foreground (in front of toucan), set at z=-4
            if (bops.Tag().Equals(Constants.TAG_BACKGROUND))
            {
                if(bops.InitPosition().z <= 0)
                    go.transform.position = new Vector3(bops.InitPosition().x, 
                        bops.InitPosition().y, Constants.Z_BACKGROUND);
                else                
                   go.transform.position = bops.InitPosition();
            }
            else if (bops.Tag().Equals(Constants.TAG_FOREGROUND))
            {
                if(bops.InitPosition().z >= -3)
                    go.transform.position = new Vector3(bops.InitPosition().x, 
                        bops.InitPosition().y, Constants.Z_FOREGROUND);
                else                
                    go.transform.position = bops.InitPosition();
            }

            // load sprite/image for object
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            // if we were not given a sprite, try loading one
            if (spri == null)
            {
                Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + bops.Name());
                if(sprite == null)
                    Logger.Log("ERROR could not load sprite: " 
                        + Constants.GRAPHICS_FILE_PATH + bops.Name());
                spriteRenderer.sprite = sprite; 
            }
            // otherwise, use the sprite we were given
            else
            {
                spriteRenderer.sprite = spri;
            }
        
            // set scale
            go.transform.localScale = bops.Scale();
        }
    
    
        /// <summary>
        /// Instantiates a story page
        /// </summary>
        /// <param name="sops">story page object properties</param>
        public void InstantiateStoryPage (StorypageObjectProperties sops, Sprite sprite)
        {
			// now make a new background
			GameObject go = new GameObject();
			
			// set object name
			go.name = (sops.Name() != "") ? sops.Name() : UnityEngine.Random.value.ToString();
			Logger.Log("Creating new story page: " + sops.Name());
			
			// set tag
			go.tag = Constants.TAG_BACKGROUND;
			
			// set layer to statics because these pages won't move
			go.layer = Constants.LAYER_STATICS;
			
			// move object to initial position 
			go.transform.position = sops.InitPosition();



			// load sprite/image for object
			SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
			
			// if no sprite was provided, try to load one with that file name
			if (sprite == null)
			{
				Sprite spt = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + 
					sops.StoryPath() + sops.Name());
				if(spt == null)
					Logger.Log("ERROR could not load sprite: " 
				          + Constants.GRAPHICS_FILE_PATH + sops.Name());
			 }


			
			spriteRenderer.sprite = sprite; 
			
			// set scale (local scale)
			if (sops.Scale() != Vector3.zero)
			{
				Logger.Log ("set scale runs...");
				//go.transform.localScale = sops.Scale();
				float width=Screen.width;
				float height = Screen.height;
				float renderX = go.GetComponent<SpriteRenderer> ().bounds.size.x;
				float renderY = go.GetComponent<SpriteRenderer> ().bounds.size.y;
				//Logger.LogError ("render X is: "+renderX.ToString());
				//Logger.LogError ("render Y is: "+renderY.ToString());
				float factorX = width /renderX;
				float factorY = height / renderY;
				go.transform.localScale = new Vector3 (factorX*0.2f, factorY*0.2f, factorX);
			}
			else
			{
			//float width=Screen.width;
			//float height = Screen.height*0.8f;
				go.transform.localScale = new Vector3(100, 100, 0);
			}

//			if (sops.Scale() != Vector3.zero)
//			{
//				Logger.Log ("set scale runs...");
//				//go.transform.localScale = sops.Scale();
//				RectTransform panel_rt=storyPanel.GetComponent<RectTransform>();
//				float width = panel_rt.rect.width;
//				float height = panel_rt.rect.height;
//				float renderX = go.GetComponent<SpriteRenderer> ().bounds.size.x;
//				float renderY = go.GetComponent<SpriteRenderer> ().bounds.size.y;
//				//Logger.LogError ("render X is: "+renderX.ToString());
//				//Logger.LogError ("render Y is: "+renderY.ToString());
//				float factorX = width /renderX;
//				float factorY = height / renderY;
//				go.transform.localScale = new Vector3 (factorX*0.8f, factorY*0.8f, factorX);
//			}
//			else
//			{
//				//float width=Screen.width;
//				//float height = Screen.height*0.8f;
//				go.transform.localScale = new Vector3(100, 100, 0);
//			}


			
			// add polygon collider and set as a trigger so enter/exit events
			// fire when this collider is hit -- needed to recognize touch events!
			PolygonCollider2D pc = go.AddComponent<PolygonCollider2D>();
			pc.isTrigger = true;
			
			// add and subscribe to gestures
			if(this.gestureManager == null) {
				Logger.Log("ERROR no gesture manager");
				FindGestureManager();
			}
			
			try {
				// add gestures and register to get event notifications
				this.gestureManager.AddAndSubscribeToGestures(go, false, true);
				this.gestureManager.pagesInStory = this.pagesInStory;
			}
			catch (Exception e)
			{
				Logger.LogError("Tried to subscribe to gestures but failed! " + e);
			}
			
			// save the initial position in case we need to reset this object later
			SavedProperties sp = go.AddComponent<SavedProperties>();
			sp.initialPosition = sops.InitPosition(); 
			sp.isStartPage = sops.IsStart();
			sp.isEndPage = sops.IsEnd();
            
        }
        
        #endregion
        
        /** Find the gesture manager */ 
        private void FindGestureManager ()
        {
            // find gesture manager
            this.gestureManager = (GestureManager)GameObject.FindGameObjectWithTag(
                Constants.TAG_GESTURE_MAN).GetComponent<GestureManager>();
			
            if(this.gestureManager == null) 
            {
                Logger.Log("ERROR: Could not find gesture manager!");
            } else {
                Logger.Log("Got gesture manager");
            }
        }
       
        /// <summary>
        /// Received message from remote controller - process and deal with message
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="cmd">Cmd.</param>
        /// <param name="props">Properties.</param>
        void HandleClientSocketReceivedMsgEvent (object sender, int cmd, object props)
        {
			Logger.LogError("!!!!!!!!!!!!!!!1!!!!MSG received from remote: " + cmd);
			Logger.LogError ("props: "+props);
            if (this.clientSocket != null)
            {
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                    Constants.LOG_ROSTOPIC, "got message"));
            }
        
            // process first token to determine which message type this is
            // if there is a second token, this is the message argument
            //
            // NOTE that you shouldn't reorder the stuff in this switch because
            // there appears to be a mono compiler bug that causes strange
            // behavior if, e.g., the "CLEAR" case is last (then nothing clears and
            // we get an argument invalid exception). I hate to be the one who 
            // writes the "sometimes weird things happen :)" comment, but here it 
            // is, weird things happen.
            // *** swapped switch for if-else so this problem should be fixed. Leaving
            // note as info in case someone in the future wants to switch to a switch.
			if (cmd == Constants.REQUEST_KEYFRAME) {
				// fire event indicating we want to log the state of the current scene
				if (this.logEvent != null) {
					// get keyframe and send it
					MainGameController.ExecuteOnMainThread.Enqueue (() => {
						LogEvent.SceneObject[] sos = null;
						this.GetSceneKeyframe (out sos);
						this.logEvent (this, new LogEvent (LogEvent.EventType.Scene, sos));
					});
				} else {
					Logger.LogWarning ("Was told to send keyframe but logger " +
					"doesn't appear to exist.");
				}
			} else if (cmd == Constants.HIGHLIGHT_OBJECT) {
				// Move the highlight behind the specified game object, if we
				// were given an object.
				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
					if (props == null) {
						Logger.Log ("Got no object to highlight. Turning highlight off!");
						this.gestureManager.LightOff ();
					} else {
						// Try to find the specified game object to highlight.
						GameObject go = GameObject.Find ((string)props);
						if (go != null) {
							this.gestureManager.LightOn (go.transform.position);
						} else {
							Logger.LogWarning ("Was told to highlight " + (string)props +
							" but could not find the game object! Maybe we need to" +
							" highlight a scene? Checking...");
							// Try to find a scene to highlight.
							if (((string)props).Contains ("scene")) {
								Logger.Log ("Yes - finding which scene to highlight...");
								// Get scene number. Scenes are 0-indexed.
								int num = -1;
								string result = Regex.Match ((string)props, @"\d+").Value;
								Logger.Log ("result is " + result);
								if (int.TryParse (result, out num)) {
									go = GameObject.Find (Constants.SCENE_SLOT + num);
									if (go != null) {
										Logger.Log ("Highlight will be size " + (this.slotWidth * 1.3f)
										+ "\n that's " + this.slotWidth + " / " + " * 1.3f");
										this.gestureManager.LightOn (
											new Vector3 (
												this.slotWidth * 2f,
												this.slotWidth * 2f,
												this.slotWidth * 2f
											),
											go.transform.position); 
									}
								} else {
									Logger.LogWarning ("Could not find scene number for"
									+ "scene slot to highlight!");
								}
							} else {
								Logger.LogWarning ("Nope - did not specify a scene! Turning"
								+ " highlight off.");
								this.gestureManager.LightOff ();
							}
						}
					}
				});  
			} else if (cmd == Constants.DISABLE_TOUCH) {
				// disable touch events from user
				this.gestureManager.allowTouch = false; 
				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
					this.SetTouch (new string[] { Constants.TAG_BACKGROUND,
						Constants.TAG_PLAY_OBJECT
					}, false);
				});
				this.storyInfo.touch_enabled = false;
			} else if (cmd == Constants.ENABLE_TOUCH) {
				// enable touch events from user
				if (this.gestureManager != null)
					this.gestureManager.allowTouch = true;
				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
					this.SetTouch (new string[] { Constants.TAG_BACKGROUND,
						Constants.TAG_PLAY_OBJECT
					}, true);
				});
				this.storyInfo.touch_enabled = true;
			} else if (cmd == Constants.RESET) {
				// reload the current level
				// e.g., when the robot's turn starts, want all characters back in their
				// starting configuration for use with automatic playbacks
				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
					this.ReloadScene ();
				});
			} else if (cmd == Constants.SIDEKICK_DO) {
				if (props == null) {
					Logger.LogWarning ("Sidekick was told to do something, but got no properties!");
				} else if (this.gameConfig.sidekick && props is String) {
					// trigger animation for sidekick character
					MainGameController.ExecuteOnMainThread.Enqueue (() => { 
						this.sidekickScript.SidekickDo ((string)props);
					}); 
				}
			} else if (cmd == Constants.SIDEKICK_SAY) {
				if (props == null) {
					Logger.LogWarning ("Sidekick was told to say something, but got no properties!");
				} else if (this.gameConfig.sidekick && props is String) {
					// trigger playback of speech for sidekick character
					MainGameController.ExecuteOnMainThread.Enqueue (() => { 
						this.sidekickScript.SidekickSay ((string)props);
					}); 
				}
			} else if (cmd == Constants.CLEAR) {
				try {                   
					if (props == null) {
						// if no properties,  remove all play objects and background
						// objects from scene, hide highlight
						MainGameController.ExecuteOnMainThread.Enqueue (() => { 
							this.ClearScene (); 
						});
					} else {
						MainGameController.ExecuteOnMainThread.Enqueue (() => {
							this.ClearObjects ((string)props);
						});
					}
				} catch (Exception ex) {
					Logger.LogError (ex);
				}
			} else if (cmd == Constants.LOAD_OBJECT) {
				// load the specified game object
				if (props == null) {
					Logger.LogWarning ("Was told to load an object, but got no properties!");
				} else {
					try {
						SceneObjectProperties sops = (SceneObjectProperties)props;
 
						// load new background image with the specified properties
						if (sops.Tag ().Equals (Constants.TAG_BACKGROUND) ||
						                  sops.Tag ().Equals (Constants.TAG_FOREGROUND)) {
							MainGameController.ExecuteOnMainThread.Enqueue (() => {
								this.InstantiateBackground ((BackgroundObjectProperties)sops, null);
							}); 
						}
    	                // or instantiate new playobject with the specified properties
    	                else if (sops.Tag ().Equals (Constants.TAG_PLAY_OBJECT)) {
							//Logger.Log("play object");
							MainGameController.ExecuteOnMainThread.Enqueue (() => { 
								this.InstantiatePlayObject ((PlayObjectProperties)sops, null);
							});
						}
					} catch (Exception e) {
						Logger.LogWarning ("Was told to load an object, but could not convert properties "
						+ "provided to SceneObjectProperties!\n" + e);
					}
				}
			} else if (cmd == Constants.MOVE_OBJECT) {
				if (props == null) {
					Logger.LogWarning ("Was told to move an object but did not " +
					"get name of which one or position to move to.");
					return;
				}
				try {
					MoveObject mo = (MoveObject)props;
					// use LeanTween to move object from curr_posn to new_posn
					MainGameController.ExecuteOnMainThread.Enqueue (() => { 
						GameObject go = GameObject.Find (mo.name);
						if (go != null)
							LeanTween.move (go, mo.destination, 2.0f).setEase (
								LeanTweenType.easeOutSine); 
					});
				} catch (Exception e) {
					Logger.LogWarning ("Was told to move an object, but properties were not for "
					+ "moving an object!\n" + e);
				}
			}
            
//			else if (cmd == Constants.FADE_SCREEN)
//            {
//                // places a white cloud-like object over the scene to give the
//                // appearance that the scene is faded out
//                MainGameController.ExecuteOnMainThread.Enqueue(() =>
//                { 
//                    if (this.fader != null)
//                        this.fader.GetComponent<Renderer>().enabled = true;
//                });
//            }
//                
//			else if (cmd == Constants.UNFADE_SCREEN)
//            {
//                // remove the fader so the scene is clearly visible again
//                MainGameController.ExecuteOnMainThread.Enqueue(() =>
//                { 
//                    if (this.fader != null)
//                        this.fader.GetComponent<Renderer>().enabled = false;
//                });
//            }

			else if (cmd == Constants.NEXT_PAGE) {
				
				// in a story game, goes to the next page in the story
				MainGameController.ExecuteOnMainThread.Enqueue (() => {
					if (this.gestureManager != null)
						this.gestureManager.ChangePage (Constants.NEXT);
				});
			} else if (cmd == Constants.PREV_PAGE) {
				
				// in a story game, goes to the previous page in the story
				MainGameController.ExecuteOnMainThread.Enqueue (() => {
					if (this.gestureManager != null)
						this.gestureManager.ChangePage (Constants.PREVIOUS);
				});
			} else if (cmd == Constants.EXIT) {
				// exit the program
				MainGameController.ExecuteOnMainThread.Enqueue (() => {
					Application.Quit ();
				});
			} else if (cmd == Constants.SET_CORRECT) {
				// given two lists of object names, set as correct or incorrect
				// set object flags for correct or incorrect
				if (props == null) {
					Logger.LogWarning ("Was told to set objects as correct/incorrect, " +
					"but got no properties!");
				} else {
					try {
						SetCorrectObject sco = (SetCorrectObject)props;
						MainGameController.ExecuteOnMainThread.Enqueue (() => {
							this.SetCorrect (sco.correct, sco.incorrect);
						});
					} catch (Exception e) {
						Logger.LogWarning ("Was told to set objects as correct/incorrect, but "
						+ "could not convert properties to SetCorrectobject!\n" + e);
					}
				}
			} else if (cmd == Constants.SHOW_CORRECT) {
				// show all objects for visual feedback tagged 'correct' or 'incorrect'
				MainGameController.ExecuteOnMainThread.Enqueue (() => {
					this.ToggleCorrect (true);
				});
			} else if (cmd == Constants.HIDE_CORRECT) {
				// hide all objects for visual feedback tagged 'correct' or 'incorrect'
				MainGameController.ExecuteOnMainThread.Enqueue (() => {
					this.ToggleCorrect (false);
				});
			} else if (cmd == Constants.SETUP_STORY_SCENE) {
				// setup story scene
				try {
					SetupStorySceneObject ssso = (SetupStorySceneObject)props;
					MainGameController.ExecuteOnMainThread.Enqueue (() => {
						this.SetupSocialStoryScene (ssso.numScenes, ssso.scenesInOrder,
							ssso.numAnswers);
					});
				} catch (Exception e) {
					Logger.LogWarning ("Supposed to set up story scene, but did not get "
					+ "properties for setting up a story scene!\n" + e);
				}
			} else if (cmd == Constants.STORY_SELECTION) {
				// select story to load
				//TODO: story name must be valid 
				string storyName = (string)props;
				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
					this.storyInfo.StoryName = storyName;
					this.storyInfo.reload = true;
					//clear all background images
					this.DestroyObjectsByTag (new string[] {
						Constants.TAG_BACKGROUND
					});
					this.LoadStory ();
					this.gestureManager.GoToFirstPage();
					this.Update ();
				});
			} else if (cmd == Constants.SAME_PAGE) {
				Logger.Log ("...same page...");
			}
			else if (cmd == Constants.STORY_SHOW_BUTTONS)
			{
				// show flip page buttons
				MainGameController.ExecuteOnMainThread.Enqueue(() =>
					{
						this.ShowPageButtons(true);
					});
			}
			else if (cmd == Constants.STORY_HIDE_BUTTONS)
			{
				// hide all objects for visual feedback tagged 'correct' or 'incorrect'
				MainGameController.ExecuteOnMainThread.Enqueue(() =>
					{
						this.ShowPageButtons(false);
					});
			}else if (cmd == Constants.STORY_GO_TO_PAGE) {
				// go to a specific page in the story
				string tmp = (string)props;
				Logger.LogError ("get tmp..");
				int storyPage = Convert.ToInt32 (tmp);

				MainGameController.ExecuteOnMainThread.Enqueue (() => { 
						Logger.LogError("go to XX story page:"+ storyPage.ToString() );
						if (this.gestureManager != null)
						this.gestureManager.GoToPage(storyPage);
					});
			}
            else
	        {
				Logger.LogError("Got a message that doesn't match any we expect!");
	            
        	}

        }

		public void sendStoryState2ROS(){
			//send storybook state message to ROS
			LogEvent.StorybookObject storyObj= new LogEvent.StorybookObject();
			storyObj.book_name = storyInfo.StoryName;
			storyObj.current_page = storyInfo.current_page;
			storyObj.total_pages = storyInfo.total_pages;
			storyObj.buttons_shown = storyInfo.buttons_shown;
			storyObj.touch_enabled = storyInfo.touch_enabled;
			this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStorybookMsg(
				Constants.STORYBOOK_ROSTOPIC, storyObj));
		}
    
    #region utilities
        /// <summary>
        /// Reload the current scene by moving all objects back to
        /// their initial positions and resetting any other relevant
        /// things
        /// </summary>
        void ReloadScene ()
        {
            Logger.Log("Reloading current scene...");
        
            // turn light off if it's not already
            this.gestureManager.LightOff();
            
            // make all feedback invisible if it's not already
            if (this.socialStories)
                this.ToggleCorrect(false);

            // move all play objects back to their initial positions
            ResetAllObjectsWithTag(new string[] {Constants.TAG_PLAY_OBJECT});
        
        }
    
        /// <summary>
        /// Clears the scene, deletes all objects
        /// </summary>
        void ClearScene ()
        {
            Logger.Log("Clearing current scene...");
        
            // turn off the light if it's not already
            this.gestureManager.LightOff();
            
            // remove all objects with specified tags
            this.DestroyObjectsByTag(new string[] {
                Constants.TAG_BACKGROUND,
                Constants.TAG_PLAY_OBJECT,
                Constants.TAG_CORRECT_FEEDBACK,
                Constants.TAG_INCORRECT_FEEDBACK,
                Constants.TAG_ANSWER_SLOT
            });
            // after removing all incorrect feedback objects, reset the list too
            if (this.incorrectFeedback != null)
                this.incorrectFeedback.Clear();
        }

        void ClearObjects(string toclear)
        {
            Logger.Log("Clearing objects: " + toclear);

            // turn off the light if it's not already
            this.gestureManager.LightOff();

            // clear background only
            if (toclear.Contains(Constants.TAG_BACKGROUND))
            {
                this.DestroyObjectsByTag(new string[] {
                    Constants.TAG_BACKGROUND
                });
            }
            // clear play objects only
            else if (toclear.Contains(Constants.TAG_PLAY_OBJECT))
            {
                this.DestroyObjectsByTag(new string[] {
                    Constants.TAG_PLAY_OBJECT
                });
            }
            // clear answer graphics only
            else if (toclear.ToLower().Contains("answer"))
            {
                // find all answer graphics and destroy them
                // these will be play objects set as correct or incorrect
                GameObject[] objs = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
                if(objs.Length == 0)
                    return;
                foreach(GameObject go in objs) 
                {
                    if (go.GetComponent<SavedProperties>() != null
                        && (go.GetComponent<SavedProperties>().isCorrect
                        || go.GetComponent<SavedProperties>().isIncorrect))
                    {
                        Logger.Log("destroying " + go.name);
                        DestroyImmediate(go);
                    }
                }

                // turn answer slots invisible
                GameObject[] slots = GameObject.FindGameObjectsWithTag(Constants.TAG_ANSWER_SLOT);
                if(objs.Length == 0)
                    return;
                foreach(GameObject go in slots)
                {
                    go.GetComponent<Renderer>().enabled = false;
                }
            }

        }
    
        /// <summary>
        /// Resets all objects with the specified tags back to initial positions
        /// </summary>
        /// <param name="tags">tags of object types to reset</param>
        void ResetAllObjectsWithTag (string[] tags)
        {
            // move objects with the specified tags
            foreach(string tag in tags) {
                // find all objects with the specified tag
                GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
                if(objs.Length == 0)
                    continue;
                foreach(GameObject go in objs) {
                    Logger.Log("moving " + go.name);
                    // if the initial position was saved, move to it
                    SavedProperties spop = go.GetComponent<SavedProperties>();
                    if(spop == null) {
                        Logger.LogWarning("Tried to reset " + go.name + " but could not find " +
                            " any saved properties.");
                    } else {
                        go.transform.position = spop.initialPosition;  
                    }
                }
            }
        }
    
        /// <summary>
        /// Destroy objects with the specified tags
        /// </summary>
        /// <param name="tags">tags of objects to destroy</param>
        void DestroyObjectsByTag (string[] tags)
        {
            // destroy objects with the specified tags
            foreach(string tag in tags) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
                if(objs.Length == 0)
                    continue;
                foreach(GameObject go in objs) {
                    Logger.Log("destroying " + go.name);
                    DestroyImmediate(go);
                }
            }
        }
        
        /// <summary>
        /// Change touch options for objects with the specified tags
        /// </summary>
        /// <param name="tags">tags of objects to change</param>
        /// <param name="enabled">enable touch or disable touch</param>
        void SetTouch (string[] tags, bool enabled)
        {
            // change touch for objects with the specified tags
            foreach(string tag in tags) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
                if(objs.Length == 0)
                    continue;
                foreach(GameObject go in objs) {
                    if (go.GetComponent<Transformer>() != null)
                    {
                        Logger.Log("touch " + (enabled ? "enabled" : "disabled") + " for " + go.name);
                        go.GetComponent<Transformer>().enabled = enabled;
                    }
                }
            }
        }
        
        /// <summary>
        /// Sets the correct/incorrect properties for a set of game objectes
        /// </summary>
        /// <param name="correctGameObjects">Correct game objects.</param>
        /// <param name="incorrectGameObjects">Incorrect game objects.</param>
        private void SetCorrect(string[] correctGameObjects, string[] incorrectGameObjects)
        {
            // First, reset all play objects to neither correct nor incorrect.
            GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
            foreach (GameObject go in gos)
            {
                if(go.GetComponent<SavedProperties>() == null) 
                {
                    Logger.LogWarning("Tried to reset correct and incorrect flags for " 
                        + go + " but could not find any saved properties.");
                } else {
                    go.GetComponent<SavedProperties>().isCorrect = false;
                    go.GetComponent<SavedProperties>().isIncorrect = false;
                } 
            }
            
            if (correctGameObjects != null)
            {
                foreach(string cgo in correctGameObjects)
                {
                    // set correct flag
                    GameObject go = GameObject.Find(cgo);
                    if(go == null || go.GetComponent<SavedProperties>() == null) 
                    {
                        Logger.LogWarning("Tried to set \"correct\" flag for " + cgo +
                         " but could not find any saved properties.");
                    } else {
                        go.GetComponent<SavedProperties>().isCorrect = true;
                    } 
                }
            }
            
            if (incorrectGameObjects != null)
            {
                foreach(string igo in incorrectGameObjects)
                {
                    // set incorrect flag
                    GameObject go = GameObject.Find(igo);
                    if(go == null || go.GetComponent<SavedProperties>() == null) 
                    {
                        Logger.LogWarning("Tried to set \"incorrect\" flag for " + igo +
                                         " but could not find any saved properties.");
                    } else {
                        go.GetComponent<SavedProperties>().isIncorrect = true;
                    } 
                }
            }
        }

		/// <summary>
		/// Show or hide flip page buttons
		/// </summary>
		private void ShowPageButtons(bool show){
			
			GameObject go_goNext = GameObject.FindGameObjectWithTag (Constants.TAG_GO_NEXT);
			GameObject go_goBack = GameObject.FindGameObjectWithTag (Constants.TAG_BACK);
			if (show) {
				Logger.LogError (" show pages ...");
				go_goNext.GetComponent<Renderer>().enabled = true;
				go_goBack.GetComponent<Renderer>().enabled = true;
				this.storyInfo.buttons_shown = true;
			} else {
				Logger.LogError (" hide pages...");
				go_goNext.GetComponent<Renderer>().enabled = false;
				go_goBack.GetComponent<Renderer>().enabled = false;
				this.storyInfo.buttons_shown = false;
			}
		}

        
        /// <summary>
        /// Show or hide visual feedback for correct and incorrect responses
        /// </summary>
        /// <param name="show">If set to <c>true</c> show.</param>
        private void ToggleCorrect(bool show)
        {
            if (show)
            {
                //show which answer slot is correct
                // find objects that have property "correct" = true / "incorrect" = true
                // first find all the play objects
                GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
                // then check their properties for flags
                int counter = 0;
                foreach (GameObject go in gos)
                {
                    if(go.GetComponent<SavedProperties>() == null) 
                    {
                        Logger.LogWarning("Tried to check flags for " + go +
                                         " but could not find any saved properties.");
                    } 
                    else if (go.GetComponent<SavedProperties>().isCorrect)
                    {
                        // load correct visual feedback object at that object
                        // make visible
                        if(this.correctFeedback != null && 
                            this.correctFeedback.GetComponent<Renderer>() != null) 
                        {
                            this.correctFeedback.GetComponent<Renderer>().enabled = true;
                            // make sure feedback is shown in the correct position
                            // this is necessary even though the feedback graphics are
                            // loaded on top of the slots, because we do not check when
                            // loading the feedback graphics *which* slots are correct
                            // or incorrect, so we probably have to swap around which 
                            // slots these graphics are shown over
                            this.correctFeedback.transform.position = 
                                new Vector3(go.transform.position.x, go.transform.position.y, 
                                    Constants.Z_FEEDBACK);
                        } 
                        else 
                        {
                            Logger.LogWarning("Tried to make correct feedback visible, but feedback "
                                + "object is null!");
                        }
                          
                    } else if (go.GetComponent<SavedProperties>().isIncorrect)
                    {
                        // load incorrect visual feedback object at that object
                        // and make visible
                        // note that if there are more objects marked correct or incorrect
                        // than there are answer slots, some won't get marked, since we 
                        // assume that there are only as many correct or incorrect options
                        // as there are answer slots
                        if(this.incorrectFeedback != null
                           && this.incorrectFeedback.Count > counter
                           && this.incorrectFeedback[counter].GetComponent<Renderer>() != null) 
                        {
                            this.incorrectFeedback[counter].transform.position = 
                                new Vector3(go.transform.position.x, go.transform.position.y, 
                                            Constants.Z_FEEDBACK);
                            this.incorrectFeedback[counter].GetComponent<Renderer>().enabled = true;
                            counter++;
                        } 
                        else 
                        {
                            Logger.LogWarning("Tried to make incorrect feedback visible, but feedback "
                                      + "object is null!");
                        }
                    }
                    // there may be some game objects that are not correct and not incorrect
                    // so we just skip over them
                    
                }
                
            }
            else
            {
                // TODO now that we're using the renderer instead of setting active,
                // we can use GameObject.Find to find these objects...

                // hide visual feedback by setting inactive
                if (this.correctFeedback != null
                    && this.correctFeedback.GetComponent<Renderer>() != null)
                {
                    this.correctFeedback.GetComponent<Renderer>().enabled = false;
                }
                else 
                {
                    Logger.LogWarning("Tried to make correct feedback invisible, but object"
                        + " was null!");
                }
                if (this.incorrectFeedback != null)
                {
                    foreach (GameObject go in this.incorrectFeedback)
                    {
                        if (go.GetComponent<Renderer>() != null)
                        {
                            go.GetComponent<Renderer>().enabled = false;
                        }
                    }
                }
                else 
                {
                    Logger.LogWarning("Tried to make incorrect feedback invisible, but object"
                                   + " was null!");
                }
            }
        }
        
        /// <summary>
        /// Sets up the social story scene.
        /// </summary>
        /// <param name="num_scenes">Number of scenes in this story</param>
        /// <param name="scenes_in_order">If set to <c>true</c> scenes are in order.</param>
        /// <param name="num_answers">Number of answer options for this story</param>
        public void SetupSocialStoryScene(int numScenes, bool scenesInOrder, int numAnswers)
        {
            // Check that we got valid data first.
            if (numScenes < 1)
            {
                Logger.LogWarning("Setup Social Story Scene: Told to set up fewer " +
                    "than 1 scene. Not setting up.");
                return;
            }

            // Save whether we are showing a social story in order or not in
            // order, since we will need that information later.
            this.scenesInOrder = scenesInOrder;

            // We have already scaled the cameras so the camera viewport is at
            // at 16:9 aspect ratio that fits within the device's screen. So
            // here, we should use the camera viewport size, not the screen
            // size, to determine how to scale the loaded graphics. This is
            // because the device screen may not have a 16:9 aspect ratio, so
            // there may be empty/black bars on either side of the game scene or
            // at the top and bottom, depending on the screen's aspect ratio. We
            // don't want any graphics to be shown outside the camera view.

            // Load background image.
            Logger.Log ("Loading background");
            Sprite bk = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + "SSBackground");
            BackgroundObjectProperties bops = new BackgroundObjectProperties(
                "SSBackground", Constants.TAG_BACKGROUND, 
                // Scale background to size of camera view.
                Camera.main.ScreenToWorldPoint(new Vector3(
                    (float)(this.scaleToHeight ? Camera.main.pixelHeight / bk.bounds.size.y 
                        : Camera.main.pixelWidth / bk.bounds.size.x),
                    (float)(this.scaleToHeight ? Camera.main.pixelHeight / bk.bounds.size.y 
                        : Camera.main.pixelWidth / bk.bounds.size.x), 
                    (float)(this.scaleToHeight ? Camera.main.pixelHeight / bk.bounds.size.y 
                        : Camera.main.pixelWidth / bk.bounds.size.x))));
            this.InstantiateBackground(bops, bk);

            // We need to scale the scene and answer slots to evenly fit in the
            // screen. We'll use the background image in place of the actual
            // screen. The scene slots and answer slots are both square.
            // Slots can be bigger if there are fewer slots, but we never want
            // them to be taller than two-fifths of the screen height.
            float backgroundWidth = GameObject.FindGameObjectWithTag(
                Constants.TAG_BACKGROUND).GetComponent<Renderer>().bounds.size.x;
            float backgroundHeight = GameObject.FindGameObjectWithTag(
                Constants.TAG_BACKGROUND).GetComponent<Renderer>().bounds.size.y;
            float slotwidth = backgroundWidth / numScenes * 0.88f;

            Logger.Log("Background size = (" + backgroundWidth + ", " + backgroundHeight + ")");
            Logger.Log("The " + numScenes + " slot(s) will have width " + slotwidth);

            // If the height of the slots is greater than half the camera view
            // height, shrink the slots so they fit on the top half of the
            // screen.
            if (slotwidth > (backgroundHeight / 2))
            {
                Logger.Log("Slots would be too tall based on the width " + slotwidth
                    + " so we're shrinking them...");
                slotwidth = (backgroundHeight / 2) * 0.95f;
                Logger.Log("New slot width is " + slotwidth);
            }

            // Save slot width so we can load scenes of the right size later.
            this.slotWidth = slotwidth;

            // We get the answer slot width later, since we do not always need
            // to load answer slots.

            // Load the number of slots needed for this story.
            for (int i = 0; i < numScenes; i++)
            {
                Sprite s = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                  + Constants.SOCIAL_STORY_FILE_PATH
                                                  + Constants.SS_SCENESLOT_PATH
                                                  + Constants.SS_SLOT_NAME
                                                  + (scenesInOrder ? "" : (i+1).ToString()));
                if (s == null)
                {
                    Logger.LogError("Could not load scene slot image!" );
                    continue;
                }

                Logger.Log("The x-position of next slot will be: " + 
                    ((-backgroundWidth / 2) + (backgroundWidth / (numScenes * 2))
                    + (i * (backgroundWidth / numScenes))));

                PlayObjectProperties pops = new PlayObjectProperties(
                    // Slot name is just the name plus a counter.
                    Constants.SCENE_SLOT + i,
                    // Tag the slot as a play object so we handle it properly
                    // elsewhere.
                    Constants.TAG_PLAY_OBJECT,
                    // Slots are not draggable.
                    false,
                    // Slots have no audio attached.
                    null,
                    // Position the object:
                    // x = left edge + offset to first item +
                    //     (counter * width of one item without item margins)
                    // y = near top of screen
                    // z = specified in constants 
                    new Vector3 (
                        (-backgroundWidth / 2) 
                            + (backgroundWidth / (numScenes * 2)) 
                            + (i * (backgroundWidth / numScenes)),
                        backgroundHeight * 0.25f,
                        Constants.Z_SLOT),
                    // Scale slot to one portion of the screen width.
                    new Vector3(
                        slotwidth / s.bounds.size.x,
                        slotwidth / s.bounds.size.y,
                        slotwidth / s.bounds.size.z)
                    );
                
                // Instantiate the scene slot with the properties above.
                this.InstantiatePlayObject(pops, s);
                
                // Change name and size to make smaller version that we will use
                // to detect collisions during out-of-order games.
                if (!scenesInOrder)
                {
                    pops.SetName(Constants.SCENE_COLLIDE_SLOT + i);
                    pops.SetInitPosition(new Vector3(pops.InitPosition().x,
                                                     pops.InitPosition().y, 
                                                     Constants.Z_COLLIDE_SLOT));
                    pops.SetScale(new Vector3(pops.Scale().x / 3, 
                                              pops.Scale().y / 3, 
                                              pops.Scale().z) / 3);
                    
                    // Instantiate smaller scene collision object.
                    this.InstantiatePlayObject(pops, s);
                }
            }
            
            // Load answer slots, if we need to.
            if (numAnswers >= 1)
            {
                // Calculate answer slot width so they're evenly spaced.
                float aslotwidth = (float) (backgroundWidth / numAnswers * 0.8);

                // If the height of the slots is greater than two-fifths of the
                // camera view height, shrink the slots so they fit in the
                // bottom half of the screen.
                if (aslotwidth > backgroundHeight * 2/5)
                {
                    aslotwidth = backgroundHeight * 2/5;
                }

                // Save answer slot width so we can load answer graphics of the
                // right size later.
                this.answerSlotWidth = aslotwidth;

                // Find the image files for the answer slots and load the number
                // of answer slots needed for this story. Since all the answer
                // slots look the same, we can load one graphic and reuse it.
                Sprite ans = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                    + Constants.SOCIAL_STORY_FILE_PATH
                                                    + Constants.SS_ANSWER_SLOT_PATH
                                                    + Constants.SS_SLOT_NAME);

                // We also load graphics for correct and incorrect feedback,
                // since these will be shown over the answer slots on request.
                Sprite feedc = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                      + Constants.SOCIAL_STORY_FILE_PATH
                                                      + Constants.SS_FEEDBACK_PATH
                                                      + Constants.SS_CORRECT_FEEDBACK_NAME);  
                
                Sprite feedic = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH
                                                       + Constants.SOCIAL_STORY_FILE_PATH
                                                       + Constants.SS_FEEDBACK_PATH
                                                       + Constants.SS_INCORRECT_FEEDBACK_NAME);                                       
                
                for (int i = 0; i < numAnswers; i++)
                {   
                    // Create answer slot properties.
                    PlayObjectProperties pops = new PlayObjectProperties(
                        // Slot name is just the name plus a counter.
                        Constants.ANSWER_SLOT + i,
                        // Tag the slot as a play object so we handle it properly
                        // elsewhere.
                        Constants.TAG_ANSWER_SLOT,
                        // Slots are not draggable.
                        false,
                        // Slots have no audio attached.
                        null,
                        // Position the object:
                        // x = left edge + offset to first item +
                        //     (counter * width of one item without item margins)
                        // y = near bottom of screen
                        // z = specified in constants 
                        new Vector3 (
                            (-backgroundWidth / 2) 
                                + (backgroundWidth / (numAnswers * 2)) 
                                + (i * backgroundWidth / (numAnswers)),
                            -backgroundHeight * 0.25f,
                            Constants.Z_SLOT),
                        // Scale to one portion of the screen width.
                        new Vector3(
                            aslotwidth / ans.bounds.size.x,
                            aslotwidth / ans.bounds.size.x,
                            aslotwidth / ans.bounds.size.x)
                        );
                    
                    // Instantiate the answer slot.
                    this.InstantiatePlayObject(pops, ans);
                    
                    // Load answer feedback graphics for each answer slots. We
                    // know only one answer will be correct, so load 1 correct
                    // and N incorrect (where N is the number of answer slots
                    // minus one). Like with the highlight object, keep a 
                    // reference to each feedback graphic and set them as not
                    // visible.
                    PlayObjectProperties pobps = new PlayObjectProperties(
                        // Put "feedback" in the names.
                        (i < numAnswers - 1 ? "feedback-incorrect" + i : "feedback-correct"),
                        (i < numAnswers - 1 ? Constants.TAG_INCORRECT_FEEDBACK : 
                        // Tas as feedback.
                         Constants.TAG_CORRECT_FEEDBACK),
                        // Feedback graphics are not draggable.
                        false,
                        // Feedback graphics have no audio attached.
                        null,
                        // Position on top of the corresponding answer slot.
                        pops.InitPosition(),
                        // Scale to one portion of the screen width, slightly
                        // bigger than the answer slot beneath it.
                        new Vector3(
                            aslotwidth /
                                (i < numAnswers - 1 ? feedic : feedc).bounds.size.x * 1.2f,
                            aslotwidth /
                                (i < numAnswers - 1 ? feedic : feedc).bounds.size.x * 1.2f,
                            aslotwidth /
                                (i < numAnswers - 1 ? feedic : feedc).bounds.size.x * 1.2f)
                        );
                    
                    // Instantiate the feedback graphic.
                    this.InstantiatePlayObject(pobps, (i < numAnswers - 1 ? feedic : feedc));
                }  
            }
        }
    
        /// <summary>
        /// Logs the state of the current scene and sends as a ROS message
        /// </summary>
        private void GetSceneKeyframe (out LogEvent.SceneObject[] sceneObjects)
        {
            // find background image name
            GameObject backg = GameObject.FindGameObjectWithTag(Constants.TAG_BACKGROUND);
            
            // find all game objects currently in scene
            GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
            
            // TODO find any other game objects with other tags?
            
            // make array of scene objects plus one for the background
            sceneObjects = new LogEvent.SceneObject[gos.Length + ((backg != null) ? 1 : 0)];
            // add background image if it exists
            if (backg != null)
            {
                LogEvent.SceneObject bo = new LogEvent.SceneObject();
                bo.name = backg.name;
                bo.tag = backg.tag;
                bo.position = new float[] { backg.transform.position.x,
                    backg.transform.position.y, backg.transform.position.z };
                bo.scale = new float[] { backg.transform.localScale.x,
                    backg.transform.localScale.y, backg.transform.localScale.z };
                bo.draggable = false;
                bo.audio = "";
                sceneObjects[sceneObjects.Length-1] = bo;
            }
            
            // for each game object, get the relevant properties for the keyframe
            // i.e., name, tag, and position
            // though strictly speaking tag isn't necessary unless we're building an
            // array of stuff that's not just play objects - which may be the case
            // later! so we're keeping it as a field anyway
            for(int i = 0; i < gos.Length; i++) 
            {
                LogEvent.SceneObject so = new LogEvent.SceneObject();
                so.name = gos[i].name;
                so.position = new float[] { gos[i].transform.position.x,
                    gos[i].transform.position.y, gos[i].transform.position.z };
                so.tag = gos[i].tag;
                so.scale = new float[] { gos[i].transform.localScale.x,
                    gos[i].transform.localScale.y, gos[i].transform.localScale.z };
                // is this object draggable?
                so.draggable = (gos[i].GetComponent<Transformer>() != null);
                // get audio clip name
                AudioSource auds = gos[i].GetComponent<AudioSource>();
                if(auds != null && auds.clip != null) { so.audio = auds.clip.name; }
                // get saved properties
                SavedProperties sp = gos[i].GetComponent<SavedProperties>();
                if (sp != null)
                {
                    so.correctSlot = sp.correctSlot;
                    so.isCorrect = sp.isCorrect;
                    so.isIncorrect = sp.isIncorrect;
                }
                sceneObjects[i] = so;
            }
        }
    #endregion
    
        /// <summary>
        /// Handles log message events
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="logme">event to log</param>
        void HandleLogEvent (object sender, LogEvent logme)
        {
            // don't log stuff for demo games
            if (this.demo) return;

            if (this.clientSocket != null)
            {
                switch(logme.type) 
                {
                case LogEvent.EventType.Action:
                    // note that for some gestures, the 2d Point returned by the gesture
                    // library does not include z position and sets z to 0 by default, so
                    // the z position may not be accurate (but it also doesn't really matter)
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishActionMsg(
                        Constants.ACTION_ROSTOPIC, logme.name, logme.nameTwo, logme.action, 
                        (logme.position.HasValue ? new float[] 
                        {logme.position.Value.x, logme.position.Value.y,
                        logme.position.Value.z} : new float[] {}),
                        (logme.positionTwo.HasValue ? new float[] 
                        {logme.positionTwo.Value.x, logme.position.Value.y,
                        logme.positionTwo.Value.z} : new float[] {}),
                        logme.message));
                    break;
                
                case LogEvent.EventType.Scene:
                    // send keyframe message
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishSceneMsg(
                        Constants.SCENE_ROSTOPIC, logme.sceneObjects));
                    break;
                
                case LogEvent.EventType.Message:
                    // send string message
                    this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                        Constants.LOG_ROSTOPIC, logme.message));
                    break;
                }
            }
        }




        /// <summary>
        /// Handles the application log message received event.
        /// </summary>
        /// <param name="logString">Log string.</param>
        /// <param name="stackTrace">Stack trace.</param>
        /// <param name="type">Type.</param>
        public void HandleApplicationLogMessageReceived(string condition, string stackTrace, 
            LogType type)
        {
            if (this.clientSocket != null && this.gameConfig.logDebugToROS)
            {
                // send log string over ROS
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                    Constants.LOG_ROSTOPIC, 
                    System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff: ") +
                    condition + "\n" + stackTrace));
            }
        }

        /// <summary>
        /// Called when sidekick audio is done playing
        /// </summary>
        void HandleDonePlayingAudioEvent(object sender)
        {
            // send a "done playing audio" message
            this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishAudioMsg(
                Constants.AUDIO_ROSTOPIC, true));
        }



		public void loadingImages(string [] fileEntries){
			
			int counter = 0;
			foreach (string fileName in fileEntries)
			{
				Logger.LogWarning ("fileName is: "+fileName);
				if (fileName == ".DS_STORE") {
					Logger.LogWarning ("file is found...");
					continue;
				}
				//load images
				string imageUrl = "file://"+fileName;


				WWW www = new WWW( imageUrl );

				while (www.isDone != true) {
					if (www.isDone == true)
						break;
				}

				Texture2D left = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);

				Sprite spriteImage = Sprite.Create(left,
					new Rect(0, 0, left.width, left.height),
					new Vector2(0.5f,0.5f),
					40);


				www.LoadImageIntoTexture(spriteImage.texture);

			

				this.storyImageSprites[counter]=spriteImage;
				counter++;
			}

		}

		void LoadImages(string storyPath)
		{
			
			//iterate through the folder to get all file names
			string [] fileEntries = Directory.GetFiles(storyPath);

			this.storyImageSprites = new Sprite[fileEntries.Length];

			storyInfo.total_pages = fileEntries.Length;

			loadingImages(fileEntries);

		}

		/** Load story */
		public void LoadStory()
		{


			string storyName=storyInfo.StoryName;
			string storyPath=storyFolder + storyName;


			Logger.LogError ("story path: "+storyPath);
			LoadImages(storyPath);
			int pageCounter = 0;



			foreach (Sprite s in this.storyImageSprites)
			{
				StorypageObjectProperties sops = new StorypageObjectProperties(
					pageCounter.ToString(),
					Constants.TAG_BACKGROUND,
					pageCounter,
					storyPath+storyName,
					new Vector3(10,10,10),
					(pageCounter == 0 ? true : false),
					(pageCounter == this.storyImageSprites.Length-1 ? true : false),
					new Vector2 (0f, -50f)
				);

				
				// instantiate the page
				if (GameObject.Find (s.name) == null || this.storyInfo.reload==true ) // skip duplicates
				{
					pagesInStory = pageCounter;
					InstantiateStoryPage(sops, s);
				}

				pageCounter++;
			}
	
		}


    }
}

using UnityEngine;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;
using TouchScript.Behaviors;

namespace opal
{
    /// <summary>
    /// The SAR-opal-base game main controller. Orchestrates everything: 
    /// sets up to receive input via ROS, initializes scenes and creates 
    /// game objecgs based on that input, deals with touch events and
    /// other tablet-specific things.
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
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
        public event LogEventHandler logEvent;
        
        // fader for fading out the screen
        private GameObject fader = null; 
    
        // DEMO  VERSION
        private bool demo = true;
    
        /// <summary>
        /// Called first, use to initialize stuff
        /// </summary>
        void Awake()
        {
            // find gesture manager
            FindGestureManager(); 
            this.gestureManager.logEvent += new LogEventHandler(HandleLogEvent);
            this.logEvent += new LogEventHandler(HandleLogEvent);
            
            // if demo, tell everyone else
            this.gestureManager.demo = this.demo;
            
            // find our sidekick
            GameObject sidekick = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
            if(sidekick == null) {
                Debug.LogError("ERROR: Could not find sidekick!");
            } else {
                Debug.Log("Got sidekick");
                this.gestureManager.AddAndSubscribeToGestures(sidekick, false);
            }
            
            this.sidekickScript = (Sidekick)sidekick.GetComponent<Sidekick>();
            if(this.sidekickScript == null) {
                Debug.LogError("ERROR: Could not get sidekick script!");
            } else {
                Debug.Log("Got sidekick script");
                this.sidekickScript.donePlayingEvent += new DonePlayingEventHandler(HandleDonePlayingAudioEvent);
            }
            
            // set up fader
            // NOTE right now we're just using one fader that fades out all but the
            // toucan - but in the unity editor there's an unused 'fader_all' that
            // can fade out everything including the toucan, just switch this tag
            // to "TAG_FADER_ALL" to use that fader instead!
            this.fader = GameObject.FindGameObjectWithTag(Constants.TAG_FADER);
            if(this.fader != null) {
                this.fader.SetActive(false);
                Debug.Log("Got fader: " + this.fader.name);
            } else {
                Debug.LogError("ERROR: No fader found");
            }
            
            // subscribe to all log events from existing play objects 
            // with collision managers
            this.SubscribeToLogEvents(new string[] { Constants.TAG_PLAY_OBJECT });
            
        }
            
        /// <summary>
        /// Called on start, use to initialize stuff
        /// </summary>
        void Start()
        {
            // set up rosbridge websocket client
            // note: does not attempt to reconnect if connection fails
            if(this.clientSocket == null && !this.demo) {
                // load websocket config from file
                string server = "";
                string port = "";
                string path = "";
            
                // find the websocket config file
                #if UNITY_ANDROID
                path = Constants.CONFIG_PATH_ANDROID + Constants.WEBSOCKET_CONFIG;
                Debug.Log("trying android path: " + path);
                #endif
            
                #if UNITY_EDITOR
                path = Application.dataPath + Constants.CONFIG_PATH_OSX + Constants.WEBSOCKET_CONFIG;
                Debug.Log("osx 1 path: " + path);
                #endif
        
                // load file
                if(!RosbridgeUtilities.DecodeWebsocketJSONConfig(path, out server, out port)) {
                    Debug.LogWarning("Could not read websocket config file! Trying "
                        + "hardcoded IP 18.85.39.35 and port 9090");
                    this.clientSocket = new RosbridgeWebSocketClient(
                    "18.85.39.35",// server, // can pass hostname or IP address
                    "9090"); //port);   
                } else {
                    this.clientSocket = new RosbridgeWebSocketClient(
                    server, // can pass hostname or IP address
                    port);  
                }
            
                this.clientSocket.SetupSocket();
                this.clientSocket.receivedMsgEvent += 
                new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);
                
                // advertise that we will publish opal_tablet messages
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                Constants.LOG_ROSTOPIC, Constants.LOG_ROSMSG_TYPE));
            
                // advertise that we will publish opal_tablet_action messages
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                Constants.ACTION_ROSTOPIC, Constants.ACTION_ROSMSG_TYPE));
                
                // advertise that we will publish opal_tablet_scene messages
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                    Constants.SCENE_ROSTOPIC, Constants.SCENE_ROSMSG_TYPE));
                
                // advertise that we will publish opal_tablet_audio messages
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                    Constants.AUDIO_ROSTOPIC, Constants.AUDIO_ROSMSG_TYPE));
                
                // subscribe to opal command messages
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
                Constants.CMD_ROSTOPIC, Constants.CMD_ROSMSG_TYPE));
                
                // public string message to opal_tablet
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                Constants.LOG_ROSTOPIC, "Opal tablet checking in!"));
            }
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
        
            // close websocket
            if(this.clientSocket != null) {
                this.clientSocket.CloseSocket();
    
                // unsubscribe from received message events
                this.clientSocket.receivedMsgEvent -= HandleClientSocketReceivedMsgEvent;
            }
            
            
        
            Debug.Log("destroyed main game controller");
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
                Debug.Log("Invoking actions on main thread....");
                try {
                    ExecuteOnMainThread.Dequeue().Invoke(); 
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error when invoking actions on main thread!" + ex);
                }
            }
        }

        /// <summary>
        /// Subscribes to log events.
        /// </summary>
        protected void SubscribeToLogEvents(string[] tags)
        {
            // subscribe to log events for all playobjects in scene
            foreach(string tag in tags) {
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
            }
        }

        /// <summary>
        /// Instantiate a new game object with the specified properties
        /// </summary>
        /// <param name="pops">properties of the play object.</param>
        void InstantiatePlayObject (PlayObjectProperties pops)
        {
            GameObject go = new GameObject();

            // set object name
            go.name = (pops.Name() != "") ? pops.Name() : UnityEngine.Random.value.ToString();
            Debug.Log("Creating new play object: " + pops.Name());

            // set tag
            go.tag = pops.Tag();
            
            // set layer
            go.layer = (pops.draggable ? Constants.LAYER_MOVEABLES : Constants.LAYER_STATICS);

            // move object to initial position 
            go.transform.position = pops.InitPosition();

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
                    Debug.Log("ERROR could not load audio: " + pops.AudioFile() + "\n" + e);
                }
                audioSource.loop = false;
                audioSource.playOnAwake = false;
            }

            // load sprite/image for object
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + pops.Name());
            if(sprite == null)
                Debug.Log("ERROR could not load sprite: " 
                    + Constants.GRAPHICS_FILE_PATH + pops.Name());
            spriteRenderer.sprite = sprite; 

            // set the scale/size of the sprite/image
            go.transform.localScale = pops.Scale();

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
                
                // and add transformer so it automatically moves on drag
                // note that the AddAndSubscribeToGestures function also
                // checks to add a transformer if there isn't one if the 
                // object is supposed to be draggable
                Transformer2D t2d = go.GetComponent<Transformer2D>();
                if (t2d == null) {
                    t2d = go.AddComponent<Transformer2D>();
                    t2d.Speed = 30;
                    t2d.enabled = true;
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
                Debug.Log("ERROR no gesture manager");
                FindGestureManager();
            }
            
            try {
                // add gestures and register to get event notifications
                this.gestureManager.AddAndSubscribeToGestures(go, pops.draggable);
            }
            catch (Exception e)
            {
                Debug.LogError("Tried to subscribe to gestures but failed! " + e);
            }
           
            // add pulsing behavior (draws attention to actionable objects)
            go.AddComponent<GrowShrinkBehavior>();
            // Removing this because it messes with collision detection when
            // objects are close to each other (continuously colliding/uncolliding)
            // go.GetComponent<GrowShrinkBehavior>().StartPulsing();
        
            // save the initial position in case we need to reset this object later
            SavedProperties sp = go.AddComponent<SavedProperties>();
            sp.initialPosition = pops.InitPosition();   
            
            // HACK to get drag to work right after object is loaded
            // for some reason if we disable then enable the Transformer2D 
            // component, drag will work. if we don't, then the Transformer2D 
            // component will be enabled but dragging will do nothing. not 
            // sure why...
            if (go.GetComponent<Transformer2D>() != null)
            {
                go.GetComponent<Transformer2D>().enabled = false;
            }
            if (go.GetComponent<Transformer2D>() != null)
            {
                go.GetComponent<Transformer2D>().enabled = true;
            }
        }
    
        /// <summary>
        /// Instantiates a background image object
        /// </summary>
        /// <param name="bops">properties of the background image object to load</param>
        private void InstantiateBackground (BackgroundObjectProperties bops)
        {
            // remove previous background if there was one
            this.DestroyObjectsByTag(new string[] {Constants.TAG_BACKGROUND});
    
            // now make a new background
            GameObject go = new GameObject();
        
            // set object name
            go.name = (bops.Name() != "") ? bops.Name() : UnityEngine.Random.value.ToString();
            Debug.Log("Creating new background: " + bops.Name());
        
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
                    go.transform.position = new Vector3(bops.InitPosition().x, bops.InitPosition().y, 2);
                else                
                   go.transform.position = bops.InitPosition();
            }
            else if (bops.Tag().Equals(Constants.TAG_FOREGROUND))
            {
                if(bops.InitPosition().z >= -3)
                    go.transform.position = new Vector3(bops.InitPosition().x, bops.InitPosition().y, -4);
                else                
                    go.transform.position = bops.InitPosition();
            }

            // load sprite/image for object
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + bops.Name());
            if(sprite == null)
                Debug.Log("ERROR could not load sprite: " 
                    + Constants.GRAPHICS_FILE_PATH + bops.Name());
            spriteRenderer.sprite = sprite; 
        
            // TODO should the scale be a parameter too?
            go.transform.localScale = new Vector3(100, 100, 100);
        }
    
        /** Find the gesture manager */ 
        private void FindGestureManager ()
        {
            // find gesture manager
            this.gestureManager = (GestureManager)GameObject.FindGameObjectWithTag(
            Constants.TAG_GESTURE_MAN).GetComponent<GestureManager>();
            if(this.gestureManager == null) {
                Debug.Log("ERROR: Could not find gesture manager!");
            } else {
                Debug.Log("Got gesture manager");
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
            Debug.Log("MSG received from remote: " + cmd);
            this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
                Constants.LOG_ROSTOPIC, "got message"));
        
            // process first token to determine which message type this is
            // if there is a second token, this is the message argument
            //
            // NOTE that you shouldn't reorder the stuff in this switch because
            // there appears to be a mono compiler bug that causes strange
            // behavior if, e.g., the "CLEAR" case is last (then nothing clears and
            // we get an argument invalid exception). I hate to be the one who 
            // writes the "sometimes weird things happen :)" comment, but here it 
            // is, weird things happen. TODO Consider swapping switch for if-elses.
            switch(cmd) { 
                case Constants.REQUEST_KEYFRAME:
                    // fire event indicating we want to log the state of the current scene
                    if(this.logEvent != null) {
                        // get keyframe and send it
                        MainGameController.ExecuteOnMainThread.Enqueue(() => {
                            LogEvent.SceneObject[] sos = null;
                            this.GetSceneKeyframe(out sos);
                            this.logEvent(this, new LogEvent(LogEvent.EventType.Scene, sos));
                        });
                    }  else {
                        Debug.LogWarning("Was told to send keyframe but logger " +
                                         "doesn't appear to exist.");
                    }
                    break;
                
                case Constants.HIGHLIGHT_OBJECT:
                    // move the highlight behind the specified game object
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        GameObject go = GameObject.Find((string)props);
                        if(go != null) {
                            this.gestureManager.LightOn(go.transform.position);
                        } else {
                            Debug.LogWarning("Was told to highlight " + (string)props + 
                                             " but could not find the game object!");
                        }
                    });  
                    break;
                
                case Constants.DISABLE_TOUCH:
                    // disable touch events from user
                    this.gestureManager.allowTouch = false; 
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.SetTouch(new string[] { Constants.TAG_BACKGROUND,
                            Constants.TAG_PLAY_OBJECT }, false);
                        // and fade the screen
                        this.fader.SetActive(true);
                    });
                    break;
                
                case Constants.ENABLE_TOUCH:
                    // enable touch events from user
                    this.gestureManager.allowTouch = true;
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.SetTouch(new string[] { Constants.TAG_BACKGROUND,
                            Constants.TAG_PLAY_OBJECT }, true);
                        // and unfade the screen
                        this.fader.SetActive(false);
                    });
                    break;
                
                case Constants.RESET:
                   // reload the current level
                    // e.g., when the robot's turn starts, want all characters back in their
                    // starting configuration for use with automatic playbacks
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.ReloadScene();
                    });
                    break;
                
                case Constants.SIDEKICK_DO:
                    // trigger animation for sidekick character
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.sidekickScript.SidekickDo((string)props);
                    }); 
                    break;
                
                case Constants.SIDEKICK_SAY:
                    // trigger playback of speech for sidekick character
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                    this.sidekickScript.SidekickSay((string)props);
                    }); 
                    break;
            
                case Constants.CLEAR:
                    Debug.Log("clearing scene");
                    try {                   
                        // remove all play objects and background objects from scene, hide highlight
                        MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                            this.ClearScene(); 
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                break;
                
                case Constants.LOAD_OBJECT:
                    // load the specified game object
                    if(props == null) {
                        Debug.Log("was told to load an object, but got no properties!");
                        break;
                    }
                    SceneObjectProperties sops = (SceneObjectProperties)props;
                    
                    // load new background image with the specified properties
                    if(sops.Tag().Equals(Constants.TAG_BACKGROUND) ||
                        sops.Tag().Equals(Constants.TAG_FOREGROUND)) {
                        Debug.Log("background");
                        MainGameController.ExecuteOnMainThread.Enqueue(() => {
                            this.InstantiateBackground((BackgroundObjectProperties)sops);
                        }); 
                    }
                    // or instantiate new playobject with the specified properties
                    else if(sops.Tag().Equals(Constants.TAG_PLAY_OBJECT)) {
                        Debug.Log("play object");
                        MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                            this.InstantiatePlayObject((PlayObjectProperties)sops);
                        });
                    }
                    break;
                
            
                case Constants.MOVE_OBJECT:
                    if(props == null) {
                        Debug.Log("Was told to move an object but did not " +
                                  "get name of which one or position to move to.");
                        return;
                    }
                    MoveObject mo = (MoveObject)props;
                    // use LeanTween to move object from curr_posn to new_posn
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        GameObject go = GameObject.Find(mo.name);
                        if(go != null)
                            LeanTween.move(go, mo.destination, 2.0f).setEase(
                                LeanTweenType.easeOutSine);    
                    });
                    break;
                
                case Constants.FADE_SCREEN:
                    Debug.LogWarning("Action fade screen not tested yet!");
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.fader.SetActive(true);
                    });
                    break;
                    
                case Constants.UNFADE_SCREEN:
                    Debug.LogWarning("Action unfade screen not tested yet!");
                    MainGameController.ExecuteOnMainThread.Enqueue(() => { 
                        this.fader.SetActive(false);
                    });
                    break;
                
                default:
                    Debug.LogWarning("Got a message that doesn't match any we expect!");
                    break;
            }
        }
    
    #region utilities
        /// <summary>
        /// Reload the current scene by moving all objects back to
        /// their initial positions and resetting any other relevant
        /// things
        /// </summary>
        void ReloadScene ()
        {
            Debug.Log("Reloading current scene...");
        
            // turn light off if it's not already
            this.gestureManager.LightOff();

            // move all play objects back to their initial positions
            ResetAllObjectsWithTag(new string[] {Constants.TAG_PLAY_OBJECT});
        
        }
    
        /// <summary>
        /// Clears the scene, deletes all objects
        /// </summary>
        void ClearScene ()
        {
            Debug.Log("Clearing current scene...");
        
            // turn off the light if it's not already
            this.gestureManager.LightOff();
        
            // remove all objects with specified tags
            this.DestroyObjectsByTag(new string[] {
                Constants.TAG_BACKGROUND,
                Constants.TAG_PLAY_OBJECT
            });
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
                    Debug.Log("moving " + go.name);
                    // if the initial position was saved, move to it
                    SavedProperties spop = go.GetComponent<SavedProperties>();
                    if(ReferenceEquals(spop, null)) {
                        Debug.LogWarning("Tried to reset " + go.name + " but could not find " +
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
                    Debug.Log("destroying " + go.name);
                    Destroy(go);
                }
            }
        }
        
        /// <summary>
        /// Destroy objects with the specified tags
        /// </summary>
        /// <param name="tags">tags of objects to destroy</param>
        /// <param name="enabled">enable touch or disable touch</param>
        void SetTouch (string[] tags, bool enabled)
        {
            // change touch for objects with the specified tags
            foreach(string tag in tags) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
                if(objs.Length == 0)
                    continue;
                foreach(GameObject go in objs) {
                    if (go.GetComponent<Transformer2D>() != null)
                    {
                        Debug.Log("touch " + (enabled ? "enabled" : "disabled") + " for " + go.name);
                        go.GetComponent<Transformer2D>().enabled = enabled;
                    }
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
            for(int i = 0; i < gos.Length; i++) {
                LogEvent.SceneObject so = new LogEvent.SceneObject();
                so.name = gos[i].name;
                so.position = new float[] { gos[i].transform.position.x,
                    gos[i].transform.position.y, gos[i].transform.position.z };
                so.tag = gos[i].tag;
                so.scale = new float[] { gos[i].transform.localScale.x,
                    gos[i].transform.localScale.y, gos[i].transform.localScale.z };
                // is this object draggable?
                so.draggable = (gos[i].GetComponent<PanGesture>() != null);
                // get audio clip name
                AudioSource auds = gos[i].GetComponent<AudioSource>();
                if(auds != null && auds.clip != null) { so.audio = auds.clip.name; }
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
            if (this.demo) return;
        
            switch(logme.type) {
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
                    logme.positionTwo.Value.z} : new float[] {})));
                break;
            
            case LogEvent.EventType.Scene:
                // send keyframe message
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishSceneMsg(
                Constants.SCENE_ROSTOPIC, logme.sceneObjects));
                break;
            
            case LogEvent.EventType.Message:
                // send string message
                this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
            Constants.LOG_ROSTOPIC, logme.state));
                break;
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

    }
}

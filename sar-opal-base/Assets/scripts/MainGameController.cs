using UnityEngine;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;

/**
 * The SAR-opal-base game main controller. Orchestrates everything: 
 * sets up to receive input via ROS, initializes scenes and creates 
 * game objecgs based on that input, deals with touch events and
 * other tablet-specific things.
 */
public class MainGameController : MonoBehaviour
{
    // gesture manager
    private GestureManager gestureManager = null;
    
    // rosbridge websocket client
    private RosbridgeWebSocketClient clientSocket = null;

    /** Called on start, use to initialize stuff  */
    void Start ()
    {
        // find gesture manager
        FindGestureManager(); 
        this.gestureManager.logEvent += new GestureManager.LogEventHandler(HandleLogEvent);
       
        // Create a new game object programmatically as a test
        PlayObjectProperties pops = new PlayObjectProperties();
        pops.setAll("ball2", Constants.TAG_PLAY_OBJECT, false, "chimes", 
                    new Vector3 (-200, 50, -2), null);
        this.InstantiatePlayObject(pops);
        
        // Create a new background programmatically as a test
        BackgroundObjectProperties bops = new BackgroundObjectProperties();
        bops.setAll("playground", Constants.TAG_BACKGROUND, 
                    new Vector3(0,0,2));
        this.InstantiateBackground(bops);
        
		// set up rosbridge websocket client
		// note: does not attempt to reconnect if connection fails
		if (this.clientSocket == null)
		{
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
            if(!RosbridgeUtilities.DecodeWebsocketJSONConfig(path, out server, out port))
            {
                Debug.LogWarning("Could not read websocket config file! Trying "
                                 + "hardcoded IP 18.85.39.32 and port 9090");
                this.clientSocket = new RosbridgeWebSocketClient(
                    "18.85.39.32",// server, // can pass hostname or IP address
                    "9090"); //port);   
            }
            else
            {
                this.clientSocket = new RosbridgeWebSocketClient(
                    server, // can pass hostname or IP address
                    port);  
            }
			
			this.clientSocket.SetupSocket();
			this.clientSocket.receivedMsgEvent += 
				new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);
				
			this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                Constants.LOG_ROSTOPIC, Constants.LOG_ROSMSG_TYPE));
            this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
                Constants.CMD_ROSTOPIC, Constants.CMD_ROSMSG_TYPE));
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
		// close websocket
		if (this.clientSocket != null)
		{
			this.clientSocket.CloseSocket();
    
			// unsubscribe from received message events
			this.clientSocket.receivedMsgEvent -= HandleClientSocketReceivedMsgEvent;
		}
		
		Debug.Log("destroyed main game controller");
    }
    
    /** 
     * Update is called once per frame 
     */
    void Update ()
    {
        // if user presses escape or 'back' button on android, exit program
        if (Input.GetKeyDown (KeyCode.Escape))
            Application.Quit ();
    }

    /// <summary>
    /// Instantiate a new game object with the specified properties
    /// </summary>
    /// <param name="pops">properties of the play object.</param>
    void InstantiatePlayObject (PlayObjectProperties pops)
    {
        GameObject go = new GameObject ();

        // set object name
        go.name = (pops.Name () != "") ? pops.Name () : UnityEngine.Random.value.ToString ();
        Debug.Log ("Creating new play object: " + pops.Name ());

        // set tag
        go.tag = Constants.TAG_PLAY_OBJECT;

        // move object to initial position 
        go.transform.position = pops.InitPosition();//pops.initPosn.x, pops.initPosn.y, pops.initPosn.z);

        // load audio - add an audio source component to the object if there
        // is an audio file to load
        if (pops.AudioFile() != null) {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            try {
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder 
                audioSource.clip = Resources.Load(Constants.AUDIO_FILE_PATH + 
                                                  pops.AudioFile()) as AudioClip;
            } catch (UnityException e) {
                Debug.Log("ERROR could not load audio: " + pops.AudioFile() + "\n" + e);
            }
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }

        // load sprite/image for object
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + pops.Name());
        if (sprite == null)
            Debug.Log ("ERROR could not load sprite: " 
                + Constants.GRAPHICS_FILE_PATH + pops.Name());
        spriteRenderer.sprite = sprite; 

        // TODO should this be a parameter too?
        go.transform.localScale = new Vector3 (100, 100, 100);

        // add rigidbody
        Rigidbody2D rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0; // don't want gravity, otherwise objects will fall

        // add polygon collider
        go.AddComponent<CircleCollider2D>();

        // add and subscribe to gestures
        if (this.gestureManager == null ) {
            Debug.Log ("ERROR no gesture manager");
            FindGestureManager();
        }
        
        // add gestures and register to get event notifications
        this.gestureManager.AddAndSubscribeToGestures(go, pops.draggable);
        
        // add pulsing behavior (draws attention to actionable objects)
        go.AddComponent<GrowShrinkBehavior>();
        
    }
    
    /// <summary>
    /// Instantiates a background image object
    /// </summary>
    /// <param name="bops">properties of the background image object to load</param>
    private void InstantiateBackground(BackgroundObjectProperties bops)
    {
        // remove previous background if there was one
        this.DestroyObjectsByTag(new string[] {Constants.TAG_BACKGROUND});
    
        // now make a new background
        GameObject go = new GameObject();
        
        // set object name
        go.name = (bops.Name() != "") ? bops.Name() : UnityEngine.Random.value.ToString ();
        Debug.Log ("Creating new background: " + bops.Name ());
        
        // set tag
        go.tag = Constants.TAG_BACKGROUND;
        
        // move object to initial position 
        go.transform.position = new Vector3(0,0,0);
        
        // load sprite/image for object
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + bops.Name());
        if (sprite == null)
            Debug.Log ("ERROR could not load sprite: " 
                       + Constants.GRAPHICS_FILE_PATH + bops.Name());
        spriteRenderer.sprite = sprite; 
        
        // TODO should this be a parameter too?
        go.transform.localScale = new Vector3 (100, 100, 100);
        
        
    }
    
    /** Find the gesture manager */ 
    private void FindGestureManager()
    {
        // find gesture manager
        this.gestureManager = (GestureManager) GameObject.FindGameObjectWithTag(
            Constants.TAG_GESTURE_MAN).GetComponent<GestureManager>();
        if (this.gestureManager == null) {
            Debug.Log("ERROR: Could not find gesture manager!");
        }
        else {
            Debug.Log("Got gesture manager");
        }
    }
    
    /**
     * Received message from remote controller - process and deal with message
     * */
    void HandleClientSocketReceivedMsgEvent (object sender, int cmd, object props)
    {
        Debug.Log ("MSG received from remote: " + cmd);
        this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishStringMsg(
            Constants.LOG_ROSTOPIC, "got message"));
        
        // process first token to determine which message type this is
        // if there is a second token, this is the message argument
        switch (cmd)
        {
        case Constants.DISABLE_TOUCH:
            // disable touch events from user
            this.gestureManager.allowTouch = false; 
            break;
            
        case Constants.ENABLE_TOUCH:
            // enable touch events from user
            this.gestureManager.allowTouch = true;
            break;
            
        case Constants.RESET:
            // reload the current level
            // e.g., when the robot's turn starts, want all characters back in their
            // starting configuration for use with automatic playbacks
            this.ReloadScene();
            break;
        case Constants.SIDEKICK_DO:
            // trigger animation for sidekick character
            Sidekick.SidekickDo((string)props);
            break;
            
        case Constants.SIDEKICK_SAY:
            // trigger playback of speech for sidekick character
            Sidekick.SidekickSay((string)props);
            break;
            
        case Constants.LOAD_OBJECT:
            Debug.LogWarning("Action load_object not fully tested yet! Might break.");
            // load new background image with the specified properties
            if (props is BackgroundObjectProperties)
            {
                this.InstantiateBackground((BackgroundObjectProperties) props);
            }
            // or instantiate new playobject with the specified properties
            else if (props is PlayObjectProperties)
            {
                this.InstantiatePlayObject((PlayObjectProperties) props);
            }
            break;
            
        case Constants.CLEAR:
            // remove all play objects and background objects from scene, hide highlight
            DestroyObjectsByTag(new string[] { Constants.TAG_BACKGROUND, 
                Constants.TAG_PLAY_OBJECT });
            gestureManager.LightOff();
            break;
            
        case Constants.MOVE_OBJECT:
            Debug.LogWarning("Action move_object not implemented yet!");
            // TODO use LeanTween to move object from curr_posn to new_posn
            break;
            
        case Constants.HIGHLIGHT_OBJECT:
            Debug.LogWarning("Action highlight_object not implemented yet!");
            // TODO ?? do we need a second highlight object for this? or just move it there?
            break;
            
        case Constants.REQUEST_KEYFRAME:
            Debug.LogWarning("Action request_keyframe not implemented yet!");
            // TODO send back keyframe log message ...
            break;
        }
    }
    
    /// <summary>
    /// Reload the current scene by moving all objects back to
    /// their initial positions and resetting any other relevant
    /// things
    /// </summary>
    void ReloadScene()
    {
        Debug.Log("Reloading current scene...");

        Debug.LogWarning("Reload not implemented yet!");
        // TODO move all play objects back to their initial positions
        // TODO need to save initial positions for objects for reloading
        
    }
    
    /// <summary>
    /// Destroy objects with the specified tags
    /// </summary>
    /// <param name="tags">tags of objects to destory</param>
    void DestroyObjectsByTag(string[] tags)
    {
        // destroy objects with the specified tags
        foreach (string tag in tags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            if (objs.Length == 0) return;
            foreach (GameObject go in objs)
            {
                Debug.Log ("destroying " + go.name);
                Destroy(go);
            }
        }
    }
    
    /// <summary>
    /// Logs the state of the current scene and sends as a ROS message
    /// </summary>
    private void LogCurrentScene()
    {
        // find background image
        
        
        // find all game objects currently in scene
        
        
        // update list of current game objects
        
    }
    
    /// <summary>
    /// Handles log message events
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="logme">event to log</param>
    void HandleLogEvent (object sender, LogEvent logme)
    {
        switch(logme.type)
        {
        case LogEvent.EventType.Action:
            RosbridgeUtilities.GetROSJsonPublishActionMsg(Constants.ACTION_ROSTOPIC,
                logme.name, logme.action, new float[] {logme.position.x, logme.position.y,
                logme.position.z}, System.DateTime.Now);
            break;
            
        case LogEvent.EventType.Scene:
            Debug.LogWarning("Log scene event not implemented yet!"); //TODO send keyframes
            break;
            
        case LogEvent.EventType.Message:
            RosbridgeUtilities.GetROSJsonPublishStringMsg(Constants.LOG_ROSTOPIC,
                                                       logme.state);
            break;
        
        }
        
    }
    

}

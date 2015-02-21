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
       
        // Create a new game object programatically as a test
        PlayObjectProperties pops = new PlayObjectProperties ();
        pops.setAll("ball2", false, "chimes", new Vector3 (-200, 50, 0), null);
        this.InstantiatePlayObject (pops);
        
		// set up rosbridge websocket client
		// note: does not attempt to reconnect if connection fails
		if (this.clientSocket == null)
		{
			this.clientSocket = new RosbridgeWebSocketClient(
				"192.168.1.36", //"18.85.38.90",
                "9090");
			
			this.clientSocket.SetupSocket();
			this.clientSocket.receivedMsgEvent += 
				new ReceivedMessageEventHandler(HandleClientSocketReceivedMsgEvent);
				
			this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonAdvertiseMsg(
                Constants.OUR_ROSTOPIC, Constants.OUR_ROSMSG_TYPE));
            this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonSubscribeMsg(
                Constants.CMD_ROSTOPIC, Constants.CMD_ROSMSG_TYPE));
            this.clientSocket.SendMessage(RosbridgeUtilities.GetROSJsonPublishMsg(
                Constants.OUR_ROSTOPIC, "Opal tablet checking in!"));
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


    /**
     * Instantiate a new game object with the specified properties
     */
    void InstantiatePlayObject (PlayObjectProperties pops)
    {
        GameObject go = new GameObject ();

        // set object name
        go.name = (pops.Name () != "") ? pops.Name () : UnityEngine.Random.value.ToString ();
        Debug.Log ("Creating new play object: " + pops.Name ());

        // set tag
        go.tag = Constants.TAG_PLAY_OBJECT;

        // move object to initial position 
        go.transform.position = pops.initPosn;//pops.initPosn.x, pops.initPosn.y, pops.initPosn.z);

        // load audio - add an audio source component to the object if there
        // is an audio file to load
        if (pops.audioFile != null) {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            try {
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder 
                audioSource.clip = Resources.Load(Constants.AUDIO_FILE_PATH + 
                                                  pops.audioFile) as AudioClip;
            } catch (UnityException e) {
                Debug.Log("ERROR could not load audio: " + pops.audioFile + "\n" + e);
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
    void HandleClientSocketReceivedMsgEvent (object sender, int cmd, string props)
    {
        Debug.Log ("!! MSG received from remote: " + cmd);
        
        // TODO parse message - then pass parsed content to switch
        
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
                // TODO trigger animation for sidekick character  
                break;
                
            case Constants.SIDEKICK_SAY:
                // TODO trigger playback of speech for sidekick character
                break;
                
            case Constants.LOAD_OBJECT:
                // TODO instantiate new playobject with the specified properties
                // TODO load new background image with the specified properties
                break;
            
            // TODO what other messages?
        }
    }
    
    
    /**
     * Reload the current scene by moving all objects back to
     * their initial positions and resetting any other relevant
     * things
     */
    void ReloadScene()
    {
        Debug.Log("Reloading current scene...");
        
        // move all play objects back to their initial positions
        // TODO 
        
    }

}

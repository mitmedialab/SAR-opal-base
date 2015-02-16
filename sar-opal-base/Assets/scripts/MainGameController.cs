using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * The SAR-opal-base game main controller. Orchestrates everything: 
 * sets up to receive input via ROS, initializes scenes and creates 
 * game objecgs based on that input, deals with touch events and
 * other tablet-specific things.
 */
using System;


public class MainGameController : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
        // Create a new game object programatically as a test
        PlayObjectProperties pops = new PlayObjectProperties();
        pops.setAll("ball2", false, null, new Vector3(-200,50,0), null);
        this.InstantiatePlayObject(pops);

	}

    /** On enable, initialize stuff */
    private void OnEnable()
    {
        // subscribe to gesture events
        GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
        foreach (GameObject go in gos)
        {
            TouchScript.Gestures.TapGesture tg = go.GetComponent<TouchScript.Gestures.TapGesture>();
            if (tg != null) {
                tg.Tapped += TapHandler;
                Debug.Log (go.name + " subscribed to gesture events!");
            }
        }
    }

    /** On disable, disable some stuff */
    private void OnDisable()
    {
        // unsubscribe from gesture events
        GameObject[] gos = GameObject.FindGameObjectsWithTag(Constants.TAG_PLAY_OBJECT);
        foreach (GameObject go in gos)
        {
            TouchScript.Gestures.TapGesture tg = go.GetComponent<TouchScript.Gestures.TapGesture>();
            if (tg != null) {
                tg.Tapped -= TapHandler;
                Debug.Log (go.name + " unsubscribed from gesture events");
            }
        }
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
     * Handle all tap events - log them and trigger actions in response
     * TODO need to trigger action on tap, e.g., play sound
     */
    private void TapHandler(object sender, EventArgs e)
    {
        // get the gesture that was sent to us
        // this gesture will tell us what object was touched
        var gesture = sender as TouchScript.Gestures.TapGesture;
        TouchScript.Hit.ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the tap occur?
        if (gesture.GetTargetHitResult(out hit)) 
        {
            // want the info as a 2D point 
            TouchScript.Hit.ITouchHit2D hit2d = (TouchScript.Hit.ITouchHit2D) hit; 
            Debug.Log ("TAP registered on " + gesture.gameObject.name + " at " + hit2d.Point);
        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.Log ("!! could not register where TAP was located!");
        }
    }

	/**
	 * Instantiate a new game object with the specified properties
	 */
	void InstantiatePlayObject (PlayObjectProperties pops)
	{
		GameObject go = new GameObject();

		// set object name
		go.name = (pops.Name() != "") ? pops.Name() : UnityEngine.Random.value.ToString();
        Debug.Log ("Creating new play object: " + pops.Name());

        // set tag
        go.tag = Constants.TAG_PLAY_OBJECT;

		// move object to initial position 
        go.transform.position = pops.initPosn;//pops.initPosn.x, pops.initPosn.y, pops.initPosn.z);

		// load audio - add an audio source component to the object if there
        // is an audio file to load
		if (pops.audioFile != null)
        {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            try	{
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder - TODO may need to change how we load
                audioSource.clip = Resources.Load (pops.audioFile) as AudioClip;
            } catch (UnityException e)
            {
                Debug.Log ("ERROR could not load audio: " + pops.audioFile + "\n" + e);
            }
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }

		// load sprite/image for object
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        Sprite sprite = Resources.Load<Sprite>(Constants.GRAPHICS_FILE_PATH + pops.Name());
        if (sprite == null) Debug.Log ("ERROR could not load sprite: " 
                                       + Constants.GRAPHICS_FILE_PATH + pops.Name());
        spriteRenderer.sprite = sprite; 

        // TODO should this be a parameter too?
        go.transform.localScale = new Vector3(100,100,100);

        // add rigidbody
        Rigidbody2D rb2d = go.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0; // don't want gravity, otherwise objects will fall

        // add polygon collider
        go.AddComponent<PolygonCollider2D>();

        // add tapgesture
        TouchScript.Gestures.TapGesture tg = go.AddComponent<TouchScript.Gestures.TapGesture>();
        tg.CombineTouches = false;
        tg.Tapped += TapHandler;
        Debug.Log (go.name + " subscribed to gesture events!");

        // TODO need to subscribe to tap gestures!
	}



}

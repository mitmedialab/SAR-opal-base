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

    // game objects
    private GameObject highlight = null; // light for highlighting objects

    /** Called on start, use to initialize stuff  */
    void Start ()
    {
        // Create a new game object programatically as a test
        PlayObjectProperties pops = new PlayObjectProperties ();
        pops.setAll ("ball2", false, null, new Vector3 (-200, 50, 0), null);
        this.InstantiatePlayObject (pops);
       
        // set up light
        this.highlight = GameObject.FindGameObjectWithTag (Constants.TAG_LIGHT);
        if (this.highlight != null) {
            this.highlight.SetActive (false);
            Debug.Log ("Got light: " + this.highlight.name);
        } else {
            Debug.Log ("ERROR: No light found");
        }
    }

    /** On enable, initialize stuff */
    private void OnEnable ()
    {
        // subscribe to gesture events
        GameObject[] gos = GameObject.FindGameObjectsWithTag (Constants.TAG_PLAY_OBJECT);
        foreach (GameObject go in gos) {
            TapGesture tg = go.GetComponent<TapGesture> ();
            if (tg != null) {
                tg.Tapped += tappedHandler;
                Debug.Log (go.name + " subscribed to pan events");
            }
            PanGesture pg = go.GetComponent<PanGesture> ();
            if (pg != null) {
                pg.Panned += pannedHandler;
                pg.PanCompleted += panCompleteHandler;
                Debug.Log (go.name + " subscribed to pan events");
            }
            PressGesture prg = go.GetComponent<PressGesture> ();
            if (prg != null) {
                prg.Pressed += pressedHandler;
                Debug.Log (go.name + " subscribed to press events");
            }
            ReleaseGesture rg = go.GetComponent<ReleaseGesture> ();
            if (rg != null) {
                rg.Released += releasedHandler;
                Debug.Log (go.name + " subscribed to release events");
            }
        }
    }

    /** On disable, disable some stuff */
    private void OnDestroy ()
    {
        // unsubscribe from gesture events
        GameObject[] gos = GameObject.FindGameObjectsWithTag (Constants.TAG_PLAY_OBJECT);
        foreach (GameObject go in gos) {
            TapGesture tg = go.GetComponent<TapGesture> ();
            if (tg != null) {
                tg.Tapped -= tappedHandler;
                Debug.Log (go.name + " unsubscribed from tap events");
            }
            PanGesture pg = go.GetComponent<PanGesture> ();
            if (pg != null) {
                pg.Panned -= pannedHandler;
                pg.PanCompleted -= panCompleteHandler;
                Debug.Log (go.name + " unsubscribed to pan events");
            }
            PressGesture prg = go.GetComponent<PressGesture> ();
            if (prg != null) {
                prg.Pressed -= pressedHandler;
                Debug.Log (go.name + " unsubscribed to press events");
            }
            ReleaseGesture rg = go.GetComponent<ReleaseGesture> ();
            if (rg != null) {
                rg.Released -= releasedHandler;
                Debug.Log (go.name + " unsubscribed to release events");
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

    #region gesture handlers
    /** 
     * Handle all tap events - log them and trigger actions in response
     * TODO need to trigger action on tap, e.g., play sound
     */
    private void tappedHandler (object sender, EventArgs e)
    {
        Debug.Log ("TAP");
        // get the gesture that was sent to us
        // this gesture will tell us what object was touched
        TapGesture gesture = sender as TapGesture;
        ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the tap occur?
        if (gesture.GetTargetHitResult (out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log ("TAP registered on " + gesture.gameObject.name + " at " + hit2d.Point);
            // TODO trigger sound on tap
        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.Log ("!! could not register where TAP was located!");
        }
    }

    /** 
     * Handle press events - log and turn on highlight
     */
    private void pressedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PRESS");
        // get the gesture that was sent to us, which will tell us 
        // which object was being dragged
        PressGesture gesture = sender as PressGesture;
        ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the press occur?
        if (gesture.GetTargetHitResult (out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log ("PRESS on " + gesture.gameObject.name + " at " + hit2d.Point);

            // move highlighting light and set active
            LightOn (1, hit2d.Point);

        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.Log ("!! could not register where PRESS was located!");
        }
    }

    /*
     * Handle released events - when object released, stop highlighting object 
     */
    private void releasedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PRESS COMPLETE");
        LightOff ();
    }
     

    /**
     * Handle all pan/drag events - log them, trigger actions in response
     */
    private void pannedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PAN");
        // get the gesture that was sent to us, which will tell us 
        // which object was being dragged
        PanGesture gesture = sender as PanGesture;
        ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the drag occur?
        if (gesture.GetTargetHitResult (out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log ("PAN on " + gesture.gameObject.name + " at " + hit2d.Point);

            // move this game object with the drag
            // TODO check if on screen?
            gesture.gameObject.transform.position = hit2d.Point;
            // move highlighting light and set active
            LightOn (1, hit2d.Point);

        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.Log ("!! could not register where PAN was located!");
        }

    }

    /*
     * Handle pan complete events - when drag is done, stop highlighting object 
     */
    private void panCompleteHandler (object sender, EventArgs e)
    {
        Debug.Log ("PAN COMPLETE");
        LightOff ();
    }
    #endregion

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
            AudioSource audioSource = go.AddComponent<AudioSource> ();
            try {
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder - TODO may need to change how we load
                audioSource.clip = Resources.Load (pops.audioFile) as AudioClip;
            } catch (UnityException e) {
                Debug.Log ("ERROR could not load audio: " + pops.audioFile + "\n" + e);
            }
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }

        // load sprite/image for object
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer> ();
        Sprite sprite = Resources.Load<Sprite> (Constants.GRAPHICS_FILE_PATH + pops.Name ());
        if (sprite == null)
            Debug.Log ("ERROR could not load sprite: " 
                + Constants.GRAPHICS_FILE_PATH + pops.Name ());
        spriteRenderer.sprite = sprite; 

        // TODO should this be a parameter too?
        go.transform.localScale = new Vector3 (100, 100, 100);

        // add rigidbody
        Rigidbody2D rb2d = go.AddComponent<Rigidbody2D> ();
        rb2d.gravityScale = 0; // don't want gravity, otherwise objects will fall

        // add polygon collider
        go.AddComponent<CircleCollider2D> ();

        // add tap gestures
        TapGesture tg = go.AddComponent<TapGesture> ();
        tg.CombineTouches = false;
        tg.Tapped += tappedHandler;
        Debug.Log (go.name + " subscribed to tap events");

        // add drag gestures
        PanGesture pg = go.AddComponent<PanGesture> ();
        pg.Panned += pannedHandler;
        pg.PanCompleted += panCompleteHandler;
        Debug.Log (go.name + " subscribed to pan events");

        // add press gestures
        PressGesture prg = go.AddComponent<PressGesture> ();
        prg.Pressed += pressedHandler;
        Debug.Log (go.name + " subscribed to press events");

        // add release gestures
        ReleaseGesture rg = go.AddComponent<ReleaseGesture> ();
        rg.Released += releasedHandler;
        Debug.Log (go.name + " subscribed to release events");

    }

    /**
     * Sets light object active in the specified position and with the specified scale
     */
    public void LightOn (Vector3 posn)
    {
        LightOn (1, posn);
    }

    public void LightOn (int scaleBy, Vector3 posn)
    {
        if (this.highlight != null) {
            this.highlight.SetActive (true);
            this.highlight.transform.position = new Vector3 (posn.x, posn.y, posn.z + 1);
            Vector3 sc = this.highlight.transform.localScale;
            sc.x *= scaleBy;
            this.highlight.transform.localScale = sc;
        } else {
            Debug.Log ("Tried to turn light on ... but light is null!");
        }
    }

    /**
     * Deactivates light, returns to specified scale
     */    
    public void LightOff ()
    {
        LightOff (1);
    }

    public void LightOff (int scaleBy)
    {
        if (this.highlight != null) {
            Vector3 sc = this.highlight.transform.localScale;
            sc.x /= scaleBy;
            this.highlight.transform.localScale = sc;
    
            this.highlight.SetActive (false); // turn light off
        } else {
            Debug.Log ("Tried to turn light off ... but light is null!");
        }
    }


}

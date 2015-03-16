using UnityEngine;
using System;
using TouchScript.Gestures;
using TouchScript.Hit;

/**
 * Manage gesture events and actions taken as a result of
 * gestures (e.g., play sound, show highlight)
 */
public class GestureManager : MonoBehaviour
{
    // allow touch? if false, doesn't react to touch events
    public bool allowTouch = true;
    
    // light for highlighting objects
    private GameObject highlight = null; 
    
    // for logging stuff
    // log message event -- fire when you want to log something
    // so others who do logging can listen for the messages
    public delegate void LogEventHandler(object sender, LogEvent logme);
    public event LogEventHandler logEvent;
    
    /** Called on start, use to initialize stuff  */
    void Start ()
    {
        // set up light
        this.highlight = GameObject.FindGameObjectWithTag(Constants.TAG_LIGHT);
        if (this.highlight != null) {
            this.highlight.SetActive (false);
            Debug.Log("Got light: " + this.highlight.name);
        } else {
            Debug.Log("ERROR: No light found");
        }
    }

    /** On enable, initialize stuff */
    private void OnEnable ()
	{  
	    // subscribe to gesture events
        GameObject[] gos = GameObject.FindGameObjectsWithTag (Constants.TAG_PLAY_OBJECT);
        foreach (GameObject go in gos) {
            AddAndSubscribeToGestures(go, true);
        }
	} 
    
	/** On destroy, disable some stuff */
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
	/** Subscribes a play object to all relevant gestures - tap, pan,
     * press, release */
    public void AddAndSubscribeToGestures(GameObject go, bool draggable)
    {
        // add a tap gesture component if one doesn't exist
        TapGesture tg = go.GetComponent<TapGesture>();
        if (tg == null) {
            tg = go.AddComponent<TapGesture>();
        }
        // checking for null anyway in case adding the component didn't work
        if (tg != null) {
            tg.Tapped += tappedHandler; // subscribe to tap events
            Debug.Log (go.name + " subscribed to pan events");
        }
        if (draggable)
        {
            // add pan gesture component if one doesn't exist yet
            PanGesture pg = go.GetComponent<PanGesture>();
            if (pg == null) {
                pg = go.AddComponent<PanGesture>();
            }
            if (pg != null) {
                pg.Panned += pannedHandler;
                pg.PanCompleted += panCompleteHandler;
                Debug.Log (go.name + " subscribed to pan events");
            }
        }
        PressGesture prg = go.GetComponent<PressGesture>();
        if (prg == null) {
            prg = go.AddComponent<PressGesture>();
        }
        if (prg != null) {
            prg.Pressed += pressedHandler;
            Debug.Log (go.name + " subscribed to press events");
        }
        ReleaseGesture rg = go.GetComponent<ReleaseGesture>();
        if (rg == null) {
            rg = go.AddComponent<ReleaseGesture>();
        }
        if (rg != null) {
            rg.Released += releasedHandler;
            Debug.Log (go.name + " subscribed to release events");
        }
    }

    #region gesture handlers
    /** 
     * Handle all tap events - log them and trigger actions in response
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
        if (gesture.GetTargetHitResult(out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log("TAP registered on " + gesture.gameObject.name + " at " + hit2d.Point);
            
            // fire event indicating that we received a message
            if (this.logEvent != null)
            {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                 gesture.gameObject.name, "tap", hit2d.Point));
            }
            
            // trigger sound on tap
            Debug.Log("going to play a sound for " + gesture.gameObject.name);
            if (this.allowTouch) PlaySoundAndPulse(gesture.gameObject);
        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.LogWarning("!! could not register where TAP was located!");
        }
    }

    /** 
     * Handle press events - log and turn on highlight
     */
    private void pressedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PRESS");
        // get the gesture that was sent to us, which will tell us 
        // which object was pressed
        PressGesture gesture = sender as PressGesture;
        ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the press occur?
        if (gesture.GetTargetHitResult (out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log ("PRESS on " + gesture.gameObject.name + " at " + hit2d.Point);
            
            // fire event indicating that we received a message
            if (this.logEvent != null)
            {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                            gesture.gameObject.name, "press", hit2d.Point));
            }
            
            // move highlighting light and set active
            if (this.allowTouch) LightOn (1, hit2d.Point);

        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.LogWarning("!! could not register where PRESS was located!");
        }
    }

    /*
     * Handle released events - when object released, stop highlighting object 
     */
    private void releasedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PRESS COMPLETE");
        LightOff();
        
        // get the gesture that was sent to us
        PressGesture gesture = sender as PressGesture;
        ITouchHit hit;
        // get info about where the hit object was located when the gesture was
        // recognized - i.e., where on the object (in screen dimensions) did
        // the press occur?
        if (gesture.GetTargetHitResult (out hit)) {
            // want the info as a 2D point 
            ITouchHit2D hit2d = (ITouchHit2D)hit; 
            Debug.Log ("RELEASE of " + gesture.gameObject.name + " at " + hit2d.Point);
            
            // fire event indicating that we received a message
            if (this.logEvent != null)
            {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                            gesture.gameObject.name, "release", hit2d.Point));
            }
            
        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.LogWarning("!! could not register where RELEASE was located!");
        }
    }
     

    /**
     * Handle all pan/drag events - log them, trigger actions in response
     */
    private void pannedHandler (object sender, EventArgs e)
    {
        Debug.Log ("PAN");
        // TODO consider subscribing to PAN BEGIN and playing object sound then?
        
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
            if (this.allowTouch) gesture.gameObject.transform.position = hit2d.Point;
            // move highlighting light and set active
            if (this.allowTouch) LightOn (1, hit2d.Point);
            // fire event indicating that we received a message
            if (this.logEvent != null)
            {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                        gesture.gameObject.name, "pan", hit2d.Point));
            }

        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.LogWarning("!! could not register where PAN was located!");
        }

    }

    /*
     * Handle pan complete events - when drag is done, stop highlighting object 
     */
    private void panCompleteHandler (object sender, EventArgs e)
    {
        Debug.Log("PAN COMPLETE");
        LightOff();
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
            Debug.Log ("PAN COMPLETE on " + gesture.gameObject.name + " at " + hit2d.Point);
            
            // fire event indicating that we received a message
            if (this.logEvent != null)
            {
                // only send subset of msg that is actual message
                this.logEvent(this, new LogEvent(LogEvent.EventType.Action,
                            gesture.gameObject.name, "pancomplete", hit2d.Point));
            }
            
        } else {
            // this probably won't ever happen, but in case it does, we'll log it
            Debug.LogWarning("!! could not register where PAN COMPLETE was located!");
        }
    }
    #endregion
    
    #region utilities
    /**
     * Sets light object active in the specified position and with the specified scale
     */
    public void LightOn(Vector3 posn)
    {
        LightOn(1, posn);
    }

    public void LightOn(int scaleBy, Vector3 posn)
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
    public void LightOff()
    {
        LightOff(1);
    }

    public void LightOff(int scaleBy)
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

    /** 
     * Plays the first sound attached to the object, if one exists 
     */ 
    private bool PlaySound(GameObject go)
    { 
        // play audio clip if this game object has a clip to play
        AudioSource auds = go.GetComponent<AudioSource>();
        if (auds != null && auds.clip != null)
        {
            Debug.Log ("playing clip for object " + go.name);

            // play the audio clip attached to the game object
            if(!go.audio.isPlaying)
                go.audio.Play();
            return true;   
        } else {
            Debug.Log ("no sound found for " + go.name + "!");
            return false;
        }
    }
    /**
     * Plays the first sound attached to an object, if one exists, while
     * also pulsing the object's size (to draw attention to it)
     */
    private void PlaySoundAndPulse(GameObject go)
    {
        if (go != null)
        {
           // play a sound, if it exists, also pulse
           if (PlaySound(go))
                go.GetComponent<GrowShrinkBehavior>().ScaleUpOnce();
        }
    }
    
#endregion
    
}
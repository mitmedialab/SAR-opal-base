using UnityEngine;
using System.Collections;

/**
 * The SAR-opal-base game main controller. Orchestrates everything: 
 * sets up to receive input via ROS, initializes scenes and creates 
 * game objecgs based on that input, deals with touch events and
 * other tablet-specific things.
 */
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
	
	// Update is called once per frame
	void Update () 
	{
	
	}


	/**
	 * Instantiate a new game object with the specified properties
	 */
	void InstantiatePlayObject (PlayObjectProperties pops)
	{
		GameObject go = new GameObject();

		// set object name
		go.name = (pops.Name() != "") ? pops.Name() : Random.value.ToString();

        // set tag
        go.tag = "PlayObject";

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

		// TODO add other necessary components/scripts, like colliders or touch stuff


	}

}

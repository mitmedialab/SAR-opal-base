using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * properties of the play object, such as initial position and
 * whether the object is draggable
 */
public class PlayObjectProperties 
{
    /** set all properties - name of object, whether it is a draggable
     object, audio file to attach or null if none, initial position,
     end positions or null if none (stationary object or doesn't matter) */
    public void setAll(string name, bool draggable, string audioFile,
                       Vector3 initPosn, List<Vector3> endPosns)
    {
        this.SetName(name);
        this.draggable = draggable;
        this.audioFile = audioFile;
        this.initPosn = initPosn;
        this.endPosns = endPosns;
    }

	/** name of the object */ //TODO is this the name of the image/texture?
	private string objName = "";

	/** the object is draggable (true) or stationary (false) */
	public bool draggable = true;

	/** sound file to play on tap */
	public string audioFile = null;

	/** initial position in the world */
	public Vector3 initPosn = new Vector3(0, 0, 0);

	/** list of desired end positions */
	public List<Vector3> endPosns = null;

	//TODO consider get&set methods, eg make sure posns are on screen?

    /** set object name */
    public void SetName (string name)
    {
        if (name != "") this.objName = name;
    }

    /** get object name */
    public string Name ()
    {
        return this.objName;
    }

   

}
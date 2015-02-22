using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * properties of the play object, such as initial position and
 * whether the object is draggable
 */
public class PlayObjectProperties : LoadObjectProperties
{
	/** the object is draggable (true) or stationary (false) */
	public bool draggable = true;

	/** sound file to play on tap */
	private string audioFile = null;

	/** list of desired end positions */
	private List<Vector3> endPosns = null;

    /** constructor */
    public PlayObjectProperties()
    {
    }
    
    /** constructor */
    public PlayObjectProperties(string name, string tag, bool draggable, 
                                string audioFile, Vector3 initPosn, 
                                List<Vector3> endPosns)
    {
        this.SetName(name);
        this.SetInitPosition(initPosn);
        this.draggable = draggable;
        this.audioFile = audioFile;
        this.endPosns = endPosns;
    }

    /** set all properties - name of object, whether it is a draggable
     object, audio file to attach or null if none, initial position,
     end positions or null if none (stationary object or doesn't matter) */
    public void setAll(string name, string tag, bool draggable, 
                       string audioFile, Vector3 initPosn, List<Vector3> endPosns)
    {
        this.SetName(name);       
        this.SetInitPosition(initPosn);
        this.draggable = draggable;
        this.audioFile = audioFile;
        this.endPosns = endPosns;
    }
    
    /** set object end position - checks that the desired
     * position is within the screen */
    public void AddEndPosition(Vector3 posn)
    {
        // check if position is on the screen
        if (endPosns == null) 
            endPosns = new List<Vector3>();
        this.endPosns.Add(base.CheckOnScreen(posn));
    }
   
    /** set object end positions - checks that the desired
     * positions are within the screen */
    public void SetEndPositions(List<Vector3> posns)
    {
        foreach (Vector3 posn in posns)
        {
            this.AddEndPosition(posn);
        }
    }
    
    /** get end positions */
    public List<Vector3> EndPositions()
    {
        return this.endPosns;
    }

    /** set audio file */
    public void SetAudioFile(string audioFile)
    {
        // TODO do some kind of error check on this?
        this.audioFile = audioFile;
    }
    
    /** get audio file */
    public string AudioFile()
    {
        return this.audioFile;
    }

}
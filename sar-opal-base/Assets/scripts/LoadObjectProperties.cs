using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * properties of an object to load, such as initial position
 */
public class LoadObjectProperties 
{
    /** name of the image/texture to load */
    protected string objName = "";
    
    /** tag of object */
    protected string tag = "";
    
    /** initial position in the world */
    protected Vector3 initPosn = new Vector3(0, 0, 0);
    
    /** constructor */
    public LoadObjectProperties()
    {
    }
    
    /* set all properties */
    public void setAll(string name, string tag, Vector3 initPosn)
    {
        this.SetName(name);
        this.SetTag(tag);
        this.initPosn = initPosn;
    }
    
    /** set object name */
    public void SetName(string name)
    {
        if (name != "") this.objName = name;
    }
    
    /** get object name */
    public string Name()
    {
        return this.objName;
    }
    
    /** set object tag */
    public void SetTag(string tag)
    {
        if (tag != null) this.tag = tag;
    }
    
    /** get object tag */
    public string Tag()
    {
        return this.tag;
    }
    
    /** set object initial position - checks that the desired initial
     * position is within the screen */
    public void SetInitPosition(Vector3 posn)
    {
        
        this.initPosn = this.CheckOnScreen(posn);
    }
    
    /** get object initial position */
    public Vector3 InitPosition()
    {
        return this.initPosn;
    }
    
    /** checks that the position is within the screen */
    protected Vector3 CheckOnScreen(Vector3 posn)
    {
        // check if position is on the screen
        if (posn.x > Constants.RIGHT_SIDE)
            posn.x = Constants.RIGHT_SIDE;
        else if (posn.x < Constants.LEFT_SIDE)
            posn.x = Constants.LEFT_SIDE;
        if (posn.y > Constants.TOP_SIDE)
            posn.y = Constants.TOP_SIDE;
        else if (posn.y < Constants.BOTTOM_SIDE)
            posn.y = Constants.BOTTOM_SIDE;
        return posn;
    }
    
}
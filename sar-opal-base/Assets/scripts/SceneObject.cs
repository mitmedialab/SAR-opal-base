using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An object present in the current scene
/// </summary>
public class SceneObject
{
    /** name of the image/texture to load */
    protected string objName = "";
    
    /** tag of object */
    protected string tag = "";
    
    /** current position in the world */
    protected Vector3 position = new Vector3(0, 0, 0);
        
    /// <summary>
    /// Distance to this object's goal position, if there is one
    /// (-1 if no goal)
    /// </summary>
    protected float distanceToGoal = -1;

	/// <summary>
	/// Initializes a new instance of the <see cref="SceneObject"/> class.
	/// </summary>
    public SceneObject ()
    {
    }
    
    /* set all properties */
    public void setAll(string name, string tag, Vector3 initPosn)
    {
        this.SetName(name);
        this.SetTag(tag);
        this.position = initPosn;
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
    
    
}



using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace opal
{
/**
 * properties of a story page object, such as its position
 */
    public class StorypageObjectProperties : SceneObjectProperties
    {
        /** page number in story */
        protected int pageNum = -1;
        
        /** story file path */
        protected string storypath = "";
        
        /** scale */
        protected Vector3 scale = new Vector3(0,0,0);
        
        /** constructor */
        public StorypageObjectProperties()
        {
        }
        
        /** constructor */
        public StorypageObjectProperties(string name, string tag, int pageNum, string storypath, 
        	Vector3 scale)
        {
        	this.pageNum = pageNum;
            this.SetName(name);
            this.SetTag(tag);
			this.SetInitPosition(new Vector3(initPosn.x, initPosn.y, this.pageNum));
			this.SetStoryPath(storypath);
			this.SetScale(scale);
        }
        
        /** Set storypath */
        protected void SetStoryPath(string storypath)
        {
        	this.storypath = storypath;
        }
        
        /** Get storypath */
        public string StoryPath()
        {
        	return this.storypath;
        } 
        
        /** set scale */
        protected void SetScale(Vector3 scale)
        {
        	this.scale = scale;
        }
        
        /** get scale */
        public Vector3 Scale()
        {
        	return this.scale;
        }
        
    }
}

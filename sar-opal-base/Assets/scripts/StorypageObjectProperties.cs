// Jacqueline Kory Westlund
// June 2016
//
// The MIT License (MIT)
// Copyright (c) 2016 Personal Robots Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
        
        /** is this the first page? */
        protected bool start = false;
        
        /** is this the last page? */
        protected bool end = false;
        
        /** constructor */
        public StorypageObjectProperties()
        {
        }
        
        /** constructor */
        public StorypageObjectProperties(string name, string tag, int pageNum, string storypath, 
        	Vector3 scale, bool start, bool end, Vector2 initPosn)
        {
        	this.pageNum = pageNum;
            this.SetName(name);
            this.SetTag(tag);
			this.SetInitPosition(new Vector3(initPosn.x, initPosn.y, this.pageNum));
			this.SetStoryPath(storypath);
			this.SetScale(scale);
			this.start = start;
			this.end = end;
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
        
        /** is this the start page? */
        public bool IsStart()
        {
        	return this.start;
        }
        
		/** is this the end page? */
        public bool IsEnd()
        {
        	return this.end;
        }
        
    }
}

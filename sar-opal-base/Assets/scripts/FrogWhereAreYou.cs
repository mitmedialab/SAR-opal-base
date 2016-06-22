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
using System.IO;

namespace opal
{
    /**
    * Frog, Where Are You?
    * A wordless picture book. Loads images of story pages. Swipe to go to the next page.
    *
    */
    public class FrogWhereAreYou : MonoBehaviour
    {
    	MainGameController mgc;
    
        /** Start */
        void Start ()
        {
			// find maingamecontroller
			this.mgc = (MainGameController)GameObject.FindGameObjectWithTag(
				Constants.TAG_DIRECTOR).GetComponent<MainGameController>();
			if(this.mgc == null) {
				Logger.Log("ERROR: Could not find main game controller!");
			} else {
				Logger.Log("Got main game controller");
			}
        
            // load "Frog, where are you?" story
            LoadStory();
        }
        
        /** On enable, make pages swipeable */
        void OnEnable ()
        {
            
        }
        
        /** On disable,  */
        void OnDisable ()
        {   
            
        }
        
        /** Update is called once per frame */
        void Update ()
        {
        }
        
        
        /** Load story */
        void LoadStory()
        {
        	Logger.Log ("Loading \"Frog, where are you?\" story");
            // load "Frog, where are you?" story
            // find the image files
            Sprite[] sprites = Resources.LoadAll<Sprite>(Constants.GRAPHICS_FILE_PATH 
            	+ Constants.FROG_FILE_PATH);
            	
			int pageCounter = 0;
            foreach (Sprite s in sprites)
			{
				StorypageObjectProperties sops = new StorypageObjectProperties(
					s.name,
					Constants.TAG_BACKGROUND,
					pageCounter,
					Constants.FROG_FILE_PATH,
					new Vector3(16,16,16),
					(pageCounter == 0 ? true : false),
					(pageCounter == sprites.Length-1 ? true : false),
					new Vector2 (0f, -50f)
				);
				
				// instantiate the page
				if (GameObject.Find (s.name) == null) // skip duplicates
				{
					this.mgc.pagesInStory = pageCounter;
					this.mgc.InstantiateStoryPage(sops, s);
				}
				
				pageCounter++;
			}
			
			
        }
    }
}

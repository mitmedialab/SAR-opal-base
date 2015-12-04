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
				Debug.Log("ERROR: Could not find main game controller!");
			} else {
				Debug.Log("Got main game controller");
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
        	Debug.Log ("Loading \"Frog, where are you?\" story");
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
					new Vector3(190,190,190),
					(pageCounter == 0 ? true : false),
					(pageCounter == sprites.Length-1 ? true : false)
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

using System;
using UnityEngine;

public static class Sidekick
{
    /** 
     * Plays the first sound attached to the object, if one exists 
     */ 
    public static bool SidekickSay(object props)
    { 
        // TODO figure out which audio clip to play
        Debug.LogWarning("Action sidekick_say not implemented yet!");
        
        // and play it
        
        // while playing the open-close beak animation
    
        // play audio clip if this game object has a clip to play
        /*AudioSource auds = go.GetComponent<AudioSource>();
        if (auds != null && auds.clip != null)
        {
            Debug.Log ("playing clip for object " + go.name);
            
            // play the audio clip attached to the game object
            if(!go.audio.isPlaying)
                go.audio.Play();
            
            // to do something after audio stops - 
            // auds.clip.length and then invoke(length) to do something in that time
            // or "timePlaying >= length" (make a float timeplaying to track)
            
            return true;   
        } else {
            Debug.Log ("no sound found for " + go.name + "!");
            return false;
        }*/
        return false;
    }
    
    /// <summary>
    /// Sidekick play an animation
    /// </summary>
    /// <returns><c>true</c>, if successful <c>false</c> otherwise.</returns>
    /// <param name="props">thing to do</param>
    public static bool SidekickDo(object props)
    {
        // TODO play designated animation clip for sidekick
        Debug.LogWarning("Action sidekick_do not implemented yet!");
        
        return false;
    }
}


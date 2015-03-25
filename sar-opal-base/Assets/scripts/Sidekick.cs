using System;
using UnityEngine;

namespace opal
{
    public static class Sidekick
    {
        /// <summary>
        /// Loads and  the  sound attached to the object, if one exists
        /// </summary>
        /// <returns><c>true</c>, if audio is played <c>false</c> otherwise.</returns>
        /// <param name="utterance">Utterance to say.</param>
        public static bool SidekickSay (string utterance)
        { 
            if (utterance.Equals(""))
            {
                Debug.LogWarning("Sidekick was told to say an empty string!");
                return false;
            }
                
             // TODO figure out which audio clip to play
            Debug.LogWarning("Action sidekick_say not fully implemented or tested yet!");
            
            // find our sidekick
            GameObject sidekick = GameObject.FindGameObjectWithTag(Constants.TAG_SIDEKICK);
            
            // if we didn't find the sidekick, we can't play audio!
            if (sidekick == null)
            {
                Debug.LogWarning("Was going to play sidekick sound but could not find sidekick!");
                return false;
            }
            
            // get the sidekick's audio source
            AudioSource audioSource = sidekick.GetComponent<AudioSource>();
            
            // if we didn't find a source, create once
            if (audioSource == null)
            {
                audioSource = sidekick.AddComponent<AudioSource>();
            }
            
            // then try loading a sound file to play
            try {
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder 
                audioSource.clip = Resources.Load(Constants.AUDIO_FILE_PATH + 
                                                  utterance) as AudioClip;
            } catch(UnityException e) {
                Debug.Log("ERROR could not load audio: " + utterance + "\n" + e);
                return false;
            }
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            
            // then play sound if it's not playing
            if (!sidekick.audio.isPlaying)
                sidekick.audio.Play();
            
            // TODO the above works! need to send mutliple sounds in a row and see if that
            // works too - sending chimes several times played each time :D
            
            // and then 
            // to do something after audio stops - 
            // auds.clip.length and then invoke(length) to do something in that time
            // or "timePlaying >= length" (make a float timeplaying to track)
            
            // and also while playing the open-close beak animation
            
           return true;
        }
    
        /// <summary>
        /// Sidekick play an animation
        /// </summary>
        /// <returns><c>true</c>, if successful <c>false</c> otherwise.</returns>
        /// <param name="props">thing to do</param>
        public static bool SidekickDo (string action)
        {
            // TODO play designated animation clip for sidekick
            Debug.LogWarning("Action sidekick_do not implemented yet!");
            
            if (action.Equals(""))
            {
                Debug.LogWarning("Sidekick was told to do an empty string!");
                return false;
            }
        
            return false;
        }
    }
}


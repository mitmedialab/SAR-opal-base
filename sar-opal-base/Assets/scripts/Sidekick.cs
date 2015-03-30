using System;
using UnityEngine;

namespace opal
{
    public class Sidekick : MonoBehaviour
    {
        AudioSource audioSource = null;
        Animator animator = null;
        bool check = false;
        
        /// <summary>
        /// On starting, do some setup
        /// </summary>
        void Awake()
        {
            // get the sidekick's audio source once
            this.audioSource = this.gameObject.GetComponent<AudioSource>();
            
            // if we didn't find a source, create once
            if (this.audioSource == null)
            {
                this.audioSource = this.gameObject.AddComponent<AudioSource>();
            }
            
            // TODO load all audio in Resources/Sidekick folder ahead of time?
            
            // get the sidekick's animator source once
            this.animator = this.gameObject.GetComponent<Animator>();
            
            // if we didn't find a source, create once
            if (this.animator == null)
            {
                this.animator = this.gameObject.AddComponent<Animator>();
            }
            
            // always start in an idle, no animations state
            this.animator.SetBool("Speaking", false);
            this.animator.SetBool("SpeakFly", false);
            this.animator.SetBool("Flying", false);
            
        }
        
        // Start
        void Start ()
        {
        }
        
        void OnEnable ()
        {
        }
        
        void OnDisable ()
        {   
        }
        
        // Update is called once per frame
        void Update ()
        {
            // we started playing audio and we're waiting for it to finish
            if (this.check && !this.audioSource.isPlaying)
            {
                // we're done playing audio, tell sidekick to stop playing
                // the speaking animation
                Debug.Log("done speaking");
                this.check = false;
                this.animator.SetBool("Speaking",false);
            }
        }
        
        /// <summary>
        /// Loads and  the  sound attached to the object, if one exists
        /// </summary>
        /// <returns><c>true</c>, if audio is played <c>false</c> otherwise.</returns>
        /// <param name="utterance">Utterance to say.</param>
        public bool SidekickSay (string utterance)
        { 
            if (utterance.Equals(""))
            {
                Debug.LogWarning("Sidekick was told to say an empty string!");
                return false;
            }
                
            // try loading a sound file to play
            try {
                // to load a sound file this way, the sound file needs to be in an existing 
                // Assets/Resources folder or subfolder 
                this.audioSource.clip = Resources.Load(Constants.AUDIO_FILE_PATH + 
                                                  utterance) as AudioClip;
            } catch(UnityException e) {
                Debug.LogError("ERROR could not load audio: " + utterance + "\n" + e);
                return false;
            }
            this.audioSource.loop = false;
            this.audioSource.playOnAwake = false;
            
            // then play sound if it's not playing
            if (!this.gameObject.audio.isPlaying)
            {
                // start the speaking animation
                this.animator.SetBool("Speaking",true);
                this.gameObject.audio.Play();
                this.check = true;
            }
           
           return true;
        }
    
        /// <summary>
        /// Sidekick play an animation
        /// </summary>
        /// <returns><c>true</c>, if successful <c>false</c> otherwise.</returns>
        /// <param name="props">thing to do</param>
        public bool SidekickDo (string action)
        {
            // TODO play designated animation clip for sidekick
            Debug.LogWarning("Action sidekick_do not fully tested yet!");
            
            if (action.Equals(""))
            {
                Debug.LogWarning("Sidekick was told to do an empty string!");
                return false;
            }
        
            // now try playing animation
            try {
                // start the animation
                this.animator.SetBool(action,true);
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not play animation " + action + ": " + ex);
                return false;
            }
            return true;
        }
    }
}


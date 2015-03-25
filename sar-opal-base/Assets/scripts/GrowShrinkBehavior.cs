using UnityEngine;
using System.Collections;

namespace opal
{
/**
 * GrowShrinkBehavior
 * 
 * The object this behavior is attached to will "pulse" its size, enlarging
 * a little and shrinking back to the original size
 * 
 **/
    public class GrowShrinkBehavior : MonoBehaviour
    {
        public float scaleUpBy = 1.1f; // scale object up by this much
        public float scaleUpOnce = 1.3f; // scale object one by this much
        public float scaleTime = 0.8f; // time to complete single scaling animation
    
        // Start
        void Start ()
        {
        }
    
        /** On enable, start pulsing */
        void OnEnable ()
        {
            LeanTween.scale(gameObject, new Vector3(gameObject.transform.localScale.x * 
                this.scaleUpBy, gameObject.transform.localScale.y * this.scaleUpBy, 
            gameObject.transform.localScale.z * this.scaleUpBy), Random.Range(.6f, 1.5f))
            .setEase(LeanTweenType.easeOutSine).setLoopPingPong();
        }
    
        /** On disable, stop pulsing, cancel all tweening */
        void OnDisable ()
        {   
            LeanTween.cancel(gameObject); 
        }
    
        // Update is called once per frame
        void Update ()
        {
        }
    
        /**
     * scale up once
     **/
        public void ScaleUpOnce ()
        {
            LeanTween.scale(gameObject, new Vector3(gameObject.transform.localScale.x * 
                this.scaleUpOnce, gameObject.transform.localScale.y * this.scaleUpOnce, 
            gameObject.transform.localScale.z * this.scaleUpOnce), this.scaleTime)
            .setEase(LeanTweenType.easeOutSine).setRepeat(2).setLoopPingPong();
        }
    

    }
}


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
        
        /// <summary>
        /// Initiates the grow-shrink pulsing that can be used to indicate
        /// that an object can be interacted with
        /// </summary>
        public void StartPulsing()
        {
            LeanTween.scale(gameObject, new Vector3(gameObject.transform.localScale.x * 
                                                    this.scaleUpBy, gameObject.transform.localScale.y * this.scaleUpBy, 
                                                    gameObject.transform.localScale.z * this.scaleUpBy), Random.Range(.6f, 1.5f))
                .setEase(LeanTweenType.easeOutSine).setLoopPingPong();
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


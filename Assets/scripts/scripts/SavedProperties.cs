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

using System;
using UnityEngine;

namespace opal
{
/// <summary>
/// Saved properties of this game object - namely initial position
/// so we can reset later if necessary
/// </summary>
    public class SavedProperties : MonoBehaviour
    {
        /// <summary>
        /// The initial position of the game object
        /// </summary>
        public Vector3 initialPosition = Vector3.zero;
        
		/// <summary>
		/// Is this the first page of the story? (storybook)
		/// </summary>
		public bool isStartPage = false;
		
		/// <summary>
		/// Is this the first page of the story? (storybook)
		/// </summary>
		public bool isEndPage = false;
        
        /// <summary>
        /// Is this a correct response? (social stories)
        /// Note that there are separate flags for correct and incorrect because
        /// it is possible for an object to be neither.
        /// </summary>
        public bool isCorrect = false;
        
        /// <summary>
        /// Is this an incorrect response? (social stories)
        /// Note that there are separate flags for correct and incorrect because
        /// it is possible for an object to be neither.
        /// </summary>
        public bool isIncorrect = false;
        
        /// <summary>
        /// What is the correct position of this scene in the story?
        /// Each social story has some number of scenes (e.g., 4). The scenes may
        /// be shown out of order and have to be dragged back into order, to the 
        /// correct slots.
        /// </summary>
        public int correctSlot = -1;
        
    }
}

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

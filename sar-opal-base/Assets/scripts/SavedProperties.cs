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
		/// Is this the first page of the story?
		/// </summary>
		public bool isStartPage = false;
		
		/// <summary>
		/// Is this the first page of the story?
		/// </summary>
		public bool isEndPage = false;
        
        /// <summary>
        /// Is this a correct response?
        /// Note that there are separate flags for correct and incorrect because
        /// it is possible for an object to be neither.
        /// </summary>
        public bool isCorrect = false;
        
        /// <summary>
        /// Is this an incorrect response?
        /// </summary>
        public bool isIncorrect = false;
        
    }
}

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
		/// is this the first page of the story?
		/// </summary>
		public bool isStartPage = false;
		
		/// <summary>
		/// is this the first page of the story?
		/// </summary>
		public bool isEndPage = false;
    }
}

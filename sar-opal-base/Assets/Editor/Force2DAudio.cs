using UnityEngine;
using UnityEditor;

/** 
 * Custom asset postprocessor that sets the properties of an imported
 * audio clip. Automatically called when an asset is imported.
 * source: 
 * http://answers.unity3d.com/questions/133392/change-properties-on-multiple-audio-files.html
 */
public class Force2DAudio : AssetPostprocessor 
{
	/** Called when an imported audio clip is processed */
	void OnPreprocessAudio () 
	{
		AudioImporter importer = (AudioImporter) assetImporter;
		// set the clip as a 2D sound, not a 3D sound
		// we do this so that the clip plays without the default volume 
		// rolloff that 3D sounds have (and thus, is audible when played) 
		importer.threeD = false;
	}
}

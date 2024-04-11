using UnityEditor;

namespace SOM{
	/// <summary>
	/// The Scenes module adds the path of every scene in the Build Settings as a constant. 
	/// Useful to be used in cojunction with Application.LoadLevel
	/// </summary>
	public class SOMScenesModule:SOMModule{
		//==============================
		//Properties
		//==============================
		public override string moduleName{
			get{return "Scenes";}
		}

		//==============================
		//Refresh
		//==============================
		public override void Refresh(){
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			string[] sceneNames = new string[scenes.Length];
			string[] sceneValues = new string[scenes.Length];

			for (int i = 0; i < sceneNames.Length; i++){
				sceneValues[i] = scenes[i].path;
				//Cut ".unity" off of the scene path
				sceneValues[i] = sceneValues[i].Substring(7, sceneValues[i].Length-13);
				//As a constant name, take the scene path without slashes
				sceneNames[i] = sceneValues[i].Replace('/',' ');
				sceneNames[i] = SOMUtils.NicifyConstantName(sceneNames[i]);
			}

			SOMXmlHandler.AddConstants(moduleName, sceneNames, sceneValues);
		}
	}
}
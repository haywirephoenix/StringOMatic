using UnityEditor;
using System.Collections.Generic;

namespace SOM{
	/// <summary>
	/// The Input Module adds a constant for every axes name in the ProjectSettings/InputManager.asset file
	/// </summary>
	public class SOMInputModule:SOMModule{
		//===================================
		//Properties
		//===================================
		public override string moduleName{
			get{return "Input";}
		}

		//===================================
		//Refresh
		//===================================
		public override void Refresh(){
			string[] values = GetInputAxis().ToArray();
			string[] inputs = new string[values.Length];

			for (int i = 0; i < inputs.Length; i++)
				inputs[i] = SOMUtils.NicifyConstantName(values[i]);
			SOMXmlHandler.AddConstants(moduleName, inputs, values);
		}

		//This returns a list with all axes names
		//Borrowed from http://answers.unity3d.com/questions/21083/how-do-you-get-the-button-names-assigned-to-an-axi.html
		public static List<string> GetInputAxis(){
			List<string> allAxis = new List<string>();
			SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
			SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");
			axesProperty.Next(true);
			axesProperty.Next(true);
			while (axesProperty.Next(false)){
				SerializedProperty axis = axesProperty.Copy();
				axis.Next(true);
				if (!allAxis.Contains(axis.stringValue))
					allAxis.Add(axis.stringValue);
			}
			return allAxis;
		}
	}
}
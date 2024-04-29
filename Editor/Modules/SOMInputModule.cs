using UnityEditor;
using System.Collections.Generic;

namespace SOM
{
	/// <summary>
	/// The Input Module adds a constant for every axes name in the ProjectSettings/InputManager.asset file
	/// </summary>
	public class SOMInputModule : SOMModule
	{
		//===================================
		//Properties
		//===================================
		public override string moduleName
		{
			get { return "Input"; }
		}

		//===================================
		//Refresh
		//===================================
		public override void Refresh()
		{
			string[] values = GetInputAxis().ToArray();
			string[] inputs = new string[values.Length];

			for (int i = 0; i < inputs.Length; i++)
				inputs[i] = SOMUtils.NicifyConstantName(values[i]);

			SOMDataHandler.AddConstants(moduleName, inputs, values);
		}


		public static List<string> GetInputAxis()
		{
			List<string> allAxis = new List<string>();
			SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
			SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

			// Ensure the axes property is valid
			if (axesProperty != null && axesProperty.isArray)
			{
				// Iterate over each element in the array
				for (int i = 0; i < axesProperty.arraySize; i++)
				{
					SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);

					// Get the "name" property of the axis
					SerializedProperty nameProperty = axis.FindPropertyRelative("m_Name");

					// Ensure the "name" property is valid and add it to the list
					if (nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue))
					{
						allAxis.Add(nameProperty.stringValue);
					}
				}
			}

			return allAxis;
		}
	}
}
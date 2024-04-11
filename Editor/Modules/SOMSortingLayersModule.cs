using System;
using UnityEditorInternal;
using System.Reflection;

namespace SOM{
	/// <summary>
	/// The Sorting Layers Module adds the name of every Sorting Layer name.
	/// </summary>
	public class SOMSortingLayersModule:SOMModule{
		//==============================
		//Properties
		//==============================
		public override string moduleName{
			get{return "Sorting Layers";}
		}

		//==============================
		//Refresh
		//==============================
		public override void Refresh(){
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			string[] values = (string[])sortingLayersProperty.GetValue(null, new object[0]);

			string[] sortingLayers = new string[values.Length];

			for (int i = 0; i < sortingLayers.Length; i++)
				sortingLayers[i] = SOMUtils.NicifyConstantName(values[i]);
			SOMXmlHandler.AddConstants(moduleName, sortingLayers, values);
		}
	}
}
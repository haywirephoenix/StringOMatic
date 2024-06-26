﻿using UnityEditorInternal;

namespace SOM
{
	/// <summary>
	/// The Layers Module adds a constant for every layer name in the project
	/// </summary>
	public class SOMLayersModule : SOMModule
	{
		//===================================
		//Properties
		//===================================
		public override string moduleName
		{
			get { return "Layers"; }
		}

		//===================================
		//Refresh
		//===================================
		public override void Refresh()
		{
			string[] values = InternalEditorUtility.layers;
			string[] layers = new string[values.Length];

			string[] niceValues = new string[values.Length];

			for (int i = 0; i < values.Length; i++)
				niceValues[i] = SOMUtils.NicifyModuleName(values[i]);

			for (int i = 0; i < layers.Length; i++)
				layers[i] = SOMUtils.NicifyModuleName(values[i]);

			SOMDataHandler.AddConstants(moduleName, layers, niceValues);
		}
	}
}
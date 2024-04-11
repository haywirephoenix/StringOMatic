using UnityEditorInternal;

namespace SOM{
	/// <summary>
	/// The Tags Module adds a constant for every axes name in the ProjectSettings/InputManager.asset file
	/// </summary>
	public class SOMTagsModule:SOMModule{
		//==============================
		//Properties
		//==============================
		public override string moduleName{
			get{return "Tags";}
		}

		//==============================
		//Refresh
		//==============================
		public override void Refresh(){
			string[] values = InternalEditorUtility.tags;
			string[] tags = new string[values.Length];

			for (int i = 0; i < tags.Length; i++)
				tags[i] = SOMUtils.NicifyConstantName(values[i]);
			SOMXmlHandler.AddConstants(moduleName, tags, values);
		}
	}
}
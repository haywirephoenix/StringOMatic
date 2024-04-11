using UnityEditor;

namespace SOM{
	/// <summary>
	/// The Navigation Module adds the name of about every navigation area
	/// </summary>
	public class SOMNavigationModule : SOMModule {
		//=====================================
		//Properties
		//=====================================
		public override string moduleName {
			get {
				return "Navigation";
			}
		}

		//=====================================
		//Refresh
		//=====================================
		public override void Refresh(){
			string areasModule = moduleName+".areas";
			SOMXmlHandler.AddModule(areasModule);

			string[] values = GameObjectUtility.GetNavMeshAreaNames();
			string[] areaNames = new string[values.Length];

			for (int i = 0; i < areaNames.Length; i++)
				areaNames[i] = SOMUtils.NicifyConstantName(values[i]);
			SOMXmlHandler.AddConstants(areasModule, areaNames, values);
		}
	}
}
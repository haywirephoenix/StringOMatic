
using UnityEditor;


namespace SOM
{

	public class SOMAddressableCheck
	{

		public const string assembly = "Unity.Addressables";
		public const string type = "UnityEngine.AddressableAssets.Addressables";
		public const string defineSymbol = "SOM_ADDRESSABLES";


		//==================================
		//Initialization
		//==================================
		[InitializeOnLoadMethod]
		public static void CheckForAddressables()
		{

			SOMUtils.CheckForDefineSymbol(defineSymbol, assembly, type);

		}


	}

#if SOM_ADDRESSABLES
	/// <summary>
	/// The Resources Module adds info about every file in the Resources folders of the project.
	/// It adds every folder as a Module, and every file in that folder as a constant. 
	/// The constant contains the path to the Resource to be used with the Resources class.
	/// Also, you can specify black or white filtering.
	/// </summary>
	public class SOMAddressablesModule : SOMModule
	{

		#region module consts
		//=====================================
		//Consts
		//=====================================


		public const string ClassName = "Addressables";

		#endregion

		#region module properies

		public override bool needsRefreshing
		{
			get
			{
				if (!SOMAddressablesHandler.addressablesReady)
				{
					SOMAddressablesHandler.InitalizeAddressables();
				}
				SOMScriptableSingleton<SOMPreferences> singleton = SOMPreferences.Singleton;

				return SOMPreferences.bools[needsRefreshingKey];
			}
			set
			{
				if (!SOMAddressablesHandler.addressablesReady)
				{
					SOMAddressablesHandler.InitalizeAddressables();
				}
				SOMScriptableSingleton<SOMPreferences> singleton = SOMPreferences.Singleton;

				SOMPreferences.bools[needsRefreshingKey] = value;
			}
		}

		#endregion

		#region module vars



		public override string moduleName
		{
			get { return ClassName; }
		}

		#endregion

		#region module methods


		//=====================================
		//Refresh
		//=====================================
		public override void Refresh()
		{

			SOMAddressablesHandler.GenerateAddressableConstants();
		}

		#endregion


		#region editor

		//=====================================
		//Preferences
		//=====================================
		public override void DrawPreferences()
		{

			// DrawAddressableFilter();//todo: redo filter

			base.DrawPreferences();
		}


		#endregion


	}

#endif
}


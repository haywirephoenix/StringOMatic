﻿

using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;


#if SOM_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets.ResourceLocators;

using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace SOM
{

	public class SOMAddressableCheck
	{

		public const string addressablesTypeString = "UnityEditor.AddressableAssets.Settings.AddressableAssetGroup, Unity.Addressables.Editor";
		public const string addressablesDefineSymbol = "SOM_ADDRESSABLES";

		//==================================
		//Initialization
		//==================================
		[InitializeOnLoadMethod]
		static void CheckForAddressables()
		{
			SOMUtils.CheckForDefineSymbol(addressablesDefineSymbol, addressablesTypeString);

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

		const string fileExtensionSeparator = "_";

		public const string ClassName = "Addressables";
		const string resourcesWord = "Resources";
		const string dirWord = "Dir";
		const string assetWord = "Asset";
		const string metaWord = "meta";

		const string GENERATE_ADDRESSABLES = "Generate Addressables";
		const string GENERATE_ADDRESSABLES_TOOLTIP = "Automatically add resources to addressable groups by filter";



		private const Char atChar = '@';
		private const Char dotChar = '.';
		private const Char _underscore = '_';

		#endregion

		#region module properies

		public override bool needsRefreshing
		{
			get
			{
				if (!addressablesReady)
				{
					InitalizeAddressables();
				}
				return SOMPreferences.bools[needsRefreshingKey];
			}
			set
			{
				if (!addressablesReady)
				{
					InitalizeAddressables();
				}
				SOMPreferences.bools[needsRefreshingKey] = value;
			}
		}

		SOMFilters.FilterList _list; //todo: redo filter

		SOMFilters.FilterList list
		{
			get
			{
				if (_list == null)
					_list = new SOMFilters.FilterList(moduleName, moduleName, SOMFilters.FilterList.FilterType.Black);
				return _list;
			}
		}

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

			// InitalizeAddressables();
			GenerateAddressableConstants(locator);
		}

		#endregion

		#region addressable properties

		private static IResourceLocator locator;

		private static bool addressablesReady = false;

		#endregion


		#region addressables methods

		public static void InitalizeAddressables()
		{
			addressablesReady = false;

			Addressables.InitializeAsync().Completed += OnInitializeComplete;
		}

		static void OnInitializeComplete(AsyncOperationHandle<IResourceLocator> handle)
		{
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				// Debug.Log("Addressables initialization succeeded");
				locator = handle.Result;

				addressablesReady = true;
			}
		}

		public static void GenerateAddressableConstants(IResourceLocator locator)
		{
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings == null)
			{
				return;
			}

			List<AddressableAssetGroup> groups = settings.groups;
			Dictionary<System.Guid, IList<IResourceLocation>> locationsDict = SOMAddressablesUtility.MakeLocationsListDict(locator);

			foreach (var group in groups)
			{
				string groupName = group.Name;

				ProcessAssetGroup(group, groupName, locationsDict);
			}
		}

		private static void ProcessAssetGroup(AddressableAssetGroup group, string groupName, Dictionary<System.Guid, IList<IResourceLocation>> locationsDict)
		{
			List<AddressableAssetEntry> results = new List<AddressableAssetEntry>();
			group.GatherAllAssets(results, true, true, true);

			foreach (var asset in results)
			{
				if (!asset.IsSubAsset)
				{
					ProcessMainAsset(asset, groupName, locationsDict);
				}
				else
				{
					ProcessSubAsset(asset, groupName);
				}
			}
		}


		private static void ProcessMainAsset(AddressableAssetEntry asset, string groupName, Dictionary<System.Guid, IList<IResourceLocation>> locationsDict)
		{
			string assetPath = asset.AssetPath;
			string guidString = asset.parentGroup.Guid;

			if (!Guid.TryParse(guidString, out System.Guid guid))
			{
				Debug.Log($"Invalid GUID format for asset: {asset.AssetPath}");
				return;
			}

			string constPath = $"{ClassName}.{groupName}";

			SOMDataHandler.AddConstant(constPath, "GUID", guidString);
			SOMDataHandler.AddConstant(constPath, "MainAssetPath", assetPath);


			var assetGUID = Guid.Parse(asset.guid);
			var foundResourceLoc = SOMAddressablesUtility.FindResourceDict(assetGUID, locationsDict, asset.address);

			if (foundResourceLoc != null)
			{
				// Debug.Log("added " + foundResourceLoc);
				SOMDataHandler.AddConstant(constPath, "iResourceLocation", foundResourceLoc);
			}
		}

		private static void ProcessSubAsset(AddressableAssetEntry asset, string groupName)
		{
			string parentAssetPath = asset.AssetPath;
			UnityEngine.Object subAsset = asset.TargetAsset;
			string subAssetName = subAsset.name;
			string subAssetType = asset.TargetAsset.GetType().Name;

			string constPath = $"{ClassName}.{groupName}.SubAssets";
			string constName = $"{subAssetName}{subAssetType}";
			string constValue = asset.address;

			SOMDataHandler.AddConstant(constPath, constName, constValue);
		}


		#endregion


		#region addressables editor properties

		// static bool generateAddressables
		// {
		// 	get
		// 	{
		// 		if (!SOMPreferences.bools.Contains(GENERATE_ADDRESSABLES))
		// 			generateAddressables = false;
		// 		return SOMPreferences.bools[GENERATE_ADDRESSABLES];
		// 	}
		// 	set
		// 	{
		// 		SOMPreferences.bools[GENERATE_ADDRESSABLES] = value;
		// 	}
		// }


		#endregion


		#region editor vars

		static List<SOMAddressablesUtility.AddressableListItem> addressableListItems;

		private Vector2 scrollPos;
		private int selected = -1;
		private Vector2 scrollPosition;
		private bool unfoldAssets = true;
		private bool init;

		#endregion

		#region editor consts

		const string FILES = "Files";
		const string ASSETS = "Assets";
		const string ADD_ASSET = "Add Asset";
		const string GROUP_NAME = "Group Name: ";
		const string BUILT_IN_DATA = "Built In Data";
		const string SAVE = "Save Group";
		const string DELETE = "Delete";
		const string X = "X";
		const string ADD_NEW_GROUP = "Add New Group";
		const string NEW_GROUP_UNSAVED = "New group (unsaved)";

		#endregion

		#region editor methods

		//=====================================
		//Preferences
		//=====================================
		public override void DrawPreferences()
		{

			// DrawAddressableFilter();//todo: redo filter

			base.DrawPreferences();
		}


		private void Init()
		{
			init = true;

			addressableListItems = new();
			scrollPos = new();

			var groups = SOMAddressablesUtility.GetAddressableGroups();

			foreach (var group in groups)
			{

				SOMAddressablesUtility.AddressableListItem addressableListItem = new(group);

				addressableListItems.Add(addressableListItem);

			}
		}


		private void DrawAddressableFilter()
		{
			if (!init)
			{
				Init();
			}

			EditorGUILayout.Space(5);

			GUILayout.Label("Addressable Filters", EditorStyles.boldLabel);

			EditorGUILayout.Space(10);

			// Check if the loop should be updated
			// if (shouldUpdateLoop)
			// {
			UpdateGuiElements();
			// shouldUpdateLoop = false; // Reset the flag
			// }

			// Render GUI elements

		}



		private bool DrawDeleteButton(Action callback = null)
		{
			bool buttonPressed = GUILayout.Button(X, GUILayout.Width(25));
			if (buttonPressed)
			{
				callback?.Invoke();
			}

			return buttonPressed;
		}


		private void UpdateGuiElements()
		{
			GUIStyle style = new((GUIStyle)"OL Title");

			// GUILayout.BeginHorizontal();
			// GUILayout.FlexibleSpace();
			// scrollPos = GUILayout.BeginScrollView(scrollPos, (GUIStyle)"OL Box");
			// GUILayout.BeginVertical();
			// int controlId = GUIUtility.GetControlID(SOMManager.CONTROL_ID.GetHashCode(), FocusType.Keyboard);
			// guiElements.Clear();
			// Display each item in the addressableListItems list
			for (int i = 0; i < addressableListItems.Count; i++)
			// foreach (SOMAddressablesUtility.AddressableListItem listItem in addressableListItems)
			{
				SOMAddressablesUtility.AddressableListItem listItem = addressableListItems[i];



				EditorGUI.BeginChangeCheck();

				bool thisIsSelected = i == selected;
				bool isBuiltInData = listItem.GroupName == BUILT_IN_DATA;

				EditorGUILayout.BeginHorizontal();
				GUILayout.Toggle(thisIsSelected, listItem.GroupName, style);

				GUI.enabled = !isBuiltInData;
				if (DrawDeleteButton())
				{

				}
				GUI.enabled = true;

				EditorGUILayout.EndHorizontal();

				// GUILayout.Space(8);

				if (EditorGUI.EndChangeCheck())
				{
					selected = thisIsSelected ? -1 : i;
					// GUIUtility.keyboardControl = controlId;
				}

				if (thisIsSelected)
				{
					GUILayout.BeginVertical(EditorStyles.helpBox);
					DrawAddressableListItem(listItem);
					EditorGUILayout.EndVertical();

				}


				continue;


			}

			// GUILayout.EndVertical();

			// GUILayout.EndScrollView();

			// GUILayout.EndHorizontal();



			// GUILayout.Space(20);

			// Add a button to add a new item

			if (GUILayout.Button(ADD_NEW_GROUP))
			{
				AddressableAssetGroup tempGroup = new();
				tempGroup.Name = NEW_GROUP_UNSAVED;
				addressableListItems.Add(new SOMAddressablesUtility.AddressableListItem(tempGroup));
			}

		}


		private void DrawAddressableListItem(SOMAddressablesUtility.AddressableListItem listItem)
		{

			// GUILayout.BeginHorizontal();


			// GUILayout.Space(-10);

			bool isBuiltInData = listItem.GroupName == BUILT_IN_DATA;

			GUI.enabled = !isBuiltInData;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(GROUP_NAME, GUILayout.Width(80));
			listItem.GroupName = EditorGUILayout.TextField(listItem.GroupName);
			GUILayout.EndHorizontal();

			GUILayout.Space(20);

			GUI.enabled = true;



			unfoldAssets = EditorGUILayout.Foldout(unfoldAssets, ASSETS);

			// If foldout is open, display the list
			if (unfoldAssets)
			{
				foreach (var asset in listItem.Assets)
				{
					GUILayout.BeginHorizontal();

					GUI.enabled = false;

					EditorGUILayout.ObjectField(asset.MainAsset, typeof(UnityEngine.Object), true);
					EditorGUILayout.TextField(asset.AssetPath);

					// if (DrawDeleteButton())
					// {

					// }

					GUILayout.EndHorizontal();

				}

				// if (GUILayout.Button(ADD_ASSET))
				// {

				// }

			}

			GUILayout.Space(20);

			if (!isBuiltInData)
			{

				list.DrawLayout();

				GUILayout.Space(20);

				if (GUILayout.Button(SAVE))
				{

				}

			}




			GUI.enabled = true;


			GUILayout.Space(10);








			return;

			EditorGUILayout.LabelField("Group Name:", GUILayout.Width(80));
			// listItem.GroupName = EditorGUILayout.TextField(listItem.GroupName);


			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Directories:");
			foreach (var directory in listItem.Directories)
			{
				EditorGUILayout.LabelField(directory);
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Files:");
			foreach (var file in listItem.Files)
			{
				EditorGUILayout.LabelField(file);
			}

			EditorGUILayout.Space();








		}

		#endregion


	}

#endif
}


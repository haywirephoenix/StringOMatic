#if SOM_ADDRESSABLES

using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets.ResourceLocators;

using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;


namespace SOM
{

	public static class SOMAddressablesUtility
	{

		public static void SetAddressableGroup(this UnityEngine.Object obj, string groupName)
		{
			var group = GetOrCreateGroup(groupName);
			if (group != null)
				AddAssetToGroup(obj, group);
		}

		public static void AddAssetToGroup(this UnityEngine.Object obj, AddressableAssetGroup group)
		{
			var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
			AddAssetToGroup(guid, group);
		}

		public static void AddAssetToGroup(string guid, AddressableAssetGroup group)
		{
			if (group == null)
				return;

			var settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
				return;

			var entry = settings.CreateOrMoveEntry(guid, group, false, false);
			group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, new List<AddressableAssetEntry> { entry }, false, true);
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, new List<AddressableAssetEntry> { entry }, true, false);
		}

		private static AddressableAssetGroup GetOrCreateGroup(string groupName)
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
				return null;

			var group = settings.FindGroup(groupName);
			if (group == null)
				group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

			return group;
		}

		public static void RemoveAddressableGroup(this UnityEngine.Object obj, string groupName)
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings)
			{
				var group = settings.FindGroup(groupName);
				if (!group) return;

				var assetpath = AssetDatabase.GetAssetPath(obj);
				var guid = AssetDatabase.AssetPathToGUID(assetpath);

				var entry = settings.FindAssetEntry(guid);

				if (entry == null) return;

				group.RemoveAssetEntry(entry, false);

				group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entry, false, true);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entry, true, false);
			}
		}

		public static AddressableAssetGroup GetGroupByName(string groupName)
		{
			// Get the Addressable settings
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings != null)
			{
				// Find the group by name
				AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.Name == groupName);
				return group;
			}
			else
			{
				return null;
			}
		}



		public static Dictionary<System.Guid, IList<IResourceLocation>> MakeLocationsListDict(IResourceLocator locator)
		{


			Dictionary<System.Guid, IList<IResourceLocation>> locationsDict = new();

			if (locator == null) return locationsDict;

			foreach (object locatorKey in locator.Keys)
			{

				if (!Guid.TryParse(locatorKey.ToString(), out System.Guid guid))
				{

					continue;
				}

				if (locator.Locate(locatorKey, typeof(UnityEngine.Object), out IList<IResourceLocation> locations))
				{

					locationsDict[guid] = locations;
				}

			}

			return locationsDict;

		}

		public static IList<IResourceLocation> FindResourceListInDict(Guid parentGuid, Dictionary<Guid, IList<IResourceLocation>> locationsDict)
		{
			IList<IResourceLocation> foundList = null;

			if (locationsDict == null)
			{
				// Debug.LogWarning($"locationsDict is null");
				return null;
			}

			if (locationsDict.TryGetValue(parentGuid, out IList<IResourceLocation> iResourcesList))
			{

				return iResourcesList;
			}

			return foundList;
		}

		public static IResourceLocation FindResourceInList(string assetAddress, IList<IResourceLocation> foundList)
		{
			IResourceLocation foundResourceLoc = null;

			if (foundList == null)
			{
				// Debug.LogWarning($"foundList is null");
				return null;
			}

			foreach (var resourceLocation in foundList)
			{
				// Debug.Log("PrimaryKey: " + resourceLocation.PrimaryKey);

				if (resourceLocation.PrimaryKey == assetAddress)
				{
					// Debug.Log("match: " + resourceLocation.PrimaryKey + " : " + assetAddress);
					foundResourceLoc = resourceLocation;
					break;
				}

			}

			return foundResourceLoc;
		}
		public static IResourceLocation FindResourceDict(Guid parentGuid, Dictionary<Guid, IList<IResourceLocation>> locationsDict, string assetAddress)
		{
			var foundList = FindResourceListInDict(parentGuid, locationsDict);
			return FindResourceInList(assetAddress, foundList);
		}


		public static List<AddressableAssetGroup> GetAddressableGroups()
		{
			List<AddressableAssetGroup> groups = new List<AddressableAssetGroup>();

			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings != null)
			{
				// Retrieve all assets from all groups
				List<AddressableAssetEntry> allAssets = new List<AddressableAssetEntry>();
				settings.GetAllAssets(allAssets, false);

				// Extract unique groups from the retrieved assets
				foreach (var asset in allAssets)
				{

					if (!groups.Contains(asset.parentGroup))
					{
						groups.Add(asset.parentGroup);
					}
				}
			}

			return groups;
		}

		public static void AddAddressableToGroup(UnityEngine.Object obj, string groupName)
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;

			if (settings)
			{
				var group = settings.FindGroup(groupName);
				if (!group)
				{
					Debug.LogError($"Addressable group '{groupName}' not found.");
					return;
				}

				var assetPath = AssetDatabase.GetAssetPath(obj);
				var guid = AssetDatabase.AssetPathToGUID(assetPath);

				// Check if the asset is already in the group
				if (group.entries.Any(entry => entry.guid == guid))
				{
					// Debug.LogWarning($"Asset '{obj.name}' is already in group '{groupName}'.");
					return;
				}

				var entry = settings.CreateOrMoveEntry(guid, group);
				var entriesAdded = new List<AddressableAssetEntry> { entry };

				group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesAdded, false, true);
				settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesAdded, true, false);
			}
		}

		public static void UpdateGroup(AddressableAssetGroup group, List<string> directories, List<string> files)
		{
			// Clear the existing entries in the group
			group.entries.Clear();

			// Add directories to the group
			foreach (string directory in directories)
			{
				AddFolderContentsToGroup(group, directory);
			}

			// Add files to the group
			foreach (string file in files)
			{
				AddAssetToGroup(file, group);
			}
		}

		private static void AddFolderContentsToGroup(AddressableAssetGroup group, string folderPath)
		{
			string[] assets = AssetDatabase.FindAssets("", new[] { folderPath });
			foreach (string assetGUID in assets)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
				AddAssetToGroup(assetPath, group);
			}
		}

		public class AddressableListItem
		{
			public string GroupName { get; set; }
			public List<string> Directories { get; set; }
			public List<string> Files { get; set; }
			public List<AddressableAssetEntry> Assets { get; set; }

			public AddressableListItem(AddressableAssetGroup group)
			{
				GroupName = group.Name;

				Directories = new();
				Files = new();
				Assets = new();

				List<AddressableAssetEntry> results = new();
				group.GatherAllAssets(results, true, true, true);

				foreach (var entry in results)
				{

					Assets.Add(entry);
				}
			}

			// Method to add a directory to the list
			public void AddDirectory(string directoryPath)
			{
				if (!Directories.Contains(directoryPath))
				{
					Directories.Add(directoryPath);
				}
			}

			// Method to remove a directory from the list
			public void RemoveDirectory(string directoryPath)
			{
				if (Directories.Contains(directoryPath))
				{
					Directories.Remove(directoryPath);
				}
			}

			// Method to add a file to the list
			public void AddFile(string filePath)
			{
				if (!Files.Contains(filePath))
				{
					Files.Add(filePath);
				}
			}

			// Method to remove a file from the list
			public void RemoveFile(string filePath)
			{
				if (Files.Contains(filePath))
				{
					Files.Remove(filePath);
				}
			}

			// Method to save changes to the Addressable group
			public void Save()
			{
				// Get the Addressable group by name
				AddressableAssetGroup group = SOMAddressablesUtility.GetGroupByName(GroupName);
				if (group != null)
				{
					// Update the group with the new directories and files
					SOMAddressablesUtility.UpdateGroup(group, Directories, Files);
				}
				else
				{
					// Debug.LogWarning($"Addressable group '{GroupName}' not found.");
				}
			}
		}





	}



}

#endif




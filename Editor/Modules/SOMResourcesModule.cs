using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Linq;

namespace SOM
{
	/// <summary>
	/// The Resources Module adds info about every file in the Resources folders of the project.
	/// It adds every folder as a Module, and every file in that folder as a constant. 
	/// The constant contains the path to the Resource to be used with the Resources class.
	/// Also, you can specify black or white filtering.
	/// </summary>
	public class SOMResourcesModule : SOMModule
	{
		//=====================================
		//Consts
		//=====================================

		const string fileExtensionSeparator = "_";

		public const string ClassName = "ResourcePaths";
		const string resourcesWord = "Resources";
		const string dirWord = "Dir";
		const string assetWord = "Asset";
		const string metaWord = "meta";



		private const Char atChar = '@';
		private const Char dotChar = '.';
		private const Char _underscore = '_';
		//=====================================
		//Vars
		//=====================================
		// SOMUtils.FilterList _list; //todo: redo filter

		//=====================================
		//Properties
		//=====================================
		// SOMUtils.FilterList list
		// {
		// 	get
		// 	{
		// 		if (_list == null)
		// 			_list = new SOMUtils.FilterList(moduleName, moduleName, SOMUtils.FilterList.FilterType.Black);
		// 		return _list;
		// 	}
		// }

		public override string moduleName
		{
			get { return ClassName; }
		}

		private static string AppendSuffixIfMatch(string original, string comparison, string suffix)
		{

			string newstring = original;

			if (original.Equals(comparison, StringComparison.OrdinalIgnoreCase))
			{
				newstring = original + suffix;
			}

			return newstring;
		}

		//=====================================
		//Refresh
		//=====================================
		public override void Refresh()
		{
			string rootClassName = string.Concat(ClassName, dotChar);

			// return;
			//Get all resources. The Key contains the folder name/path, and the Value a list with all of the objects in it
			Dictionary<string, List<string>> resources = GetResources();


			//For each resource
			foreach (KeyValuePair<string, List<string>> resource in resources)
			{


				//Check if this folder is in the filter //todo: redo filter with system slashes and resource.value 
				// if (list.hasFilter)
				// {
				// 	bool isFiltered = false;
				// 	string comparer = (resource.Key + "/").Substring(resourcesWord.Length + 1);
				// 	for (int i = 0; i < list.values.Length; i++)
				// 	{
				// 		if (list.values[i].EndsWith("/"))
				// 		{
				// 			if (comparer.Trim().StartsWith(list.values[i].Trim(), StringComparison.InvariantCultureIgnoreCase))
				// 			{
				// 				isFiltered = true;
				// 				break;
				// 			}
				// 		}
				// 	}
				// 	//And act accordingly...
				// 	if (!(isFiltered ^ list.isBlack))
				// 		continue;
				// }


				//deal with empty name
				if (resource.Key.Length == 0)
					continue;


				string resDir = resource.Key;

				string dirPathDotted = resDir.Replace(Path.DirectorySeparatorChar, dotChar);

				string[] dirStrSplit = dirPathDotted.Split(dotChar);

				string niceDirName = ClassName + dotChar + string.Join(dotChar, dirStrSplit.Select(SOMUtils.NicifyConstantName));

				string moduleRootKey = ClassName;
				string submodulePath = niceDirName;


				foreach (string file in resource.Value)
				{

					string niceFileName = SOMUtils.NicifyConstantName(file);

					SOMDataHandler.AddConstant(niceDirName, niceFileName, file);

				}

				continue;

				//Foreach object in the folder
				for (int i = 0; i < resource.Value.Count; i++)
				{

					//Check if this object's path is in the filter //todo: redo filter
					// if (list.hasFilter)
					// {
					// 	bool isFiltered = false;
					// 	for (int j = 0; j < list.values.Length; j++)
					// 	{
					// 		if (resource.Value[i].Trim().Equals(list.values[j].Trim(), StringComparison.InvariantCultureIgnoreCase))
					// 		{
					// 			isFiltered = true;
					// 			break;
					// 		}
					// 	}
					// 	//And act accordingly...
					// 	if (!(isFiltered ^ list.isBlack))
					// 		continue;
					// }


				}

			}

		}

		public static Dictionary<string, List<string>> GetResources()
		{
			string[] rootResources = Directory.GetDirectories(Application.dataPath, "Resources", SearchOption.AllDirectories);
			Dictionary<string, List<string>> resourceDict = new();


			foreach (string rootResource in rootResources)
			{

				List<string> assets = new();

				// string path = rootResource.Replace(Application.dataPath, "Assets");
				// string path = rootResource.Replace(Application.dataPath, "Assets");
				string path = rootResource.Substring(Application.dataPath.Length + 1);
				string fullpath = rootResource;

				string[] files = Directory.GetFiles(rootResource, "*", SearchOption.AllDirectories);
				foreach (string file in files)
				{
					string relativePath = GetRelativePath(file);
					string filename = Path.GetFileName(file);

					if (filename.EndsWith(metaWord) || filename.StartsWith(dotChar))
						continue;

					// string assetName = Path.GetFileNameWithoutExtension(relativePath);
					// string extension = Path.GetExtension(relativePath);
					assets.Add(filename);
				}

				resourceDict.Add(path, assets);
			}

			return resourceDict;
		}

		private static string GetRelativePath(string fullPath)
		{
			return fullPath.Replace(Application.dataPath, "Assets");
		}

		void AddConstants(Dictionary<string, List<string>> dictionary, string folder)
		{
			//Module name is taken from the last "resource" occurrence onwards
			string moduleName = folder.Substring(folder.LastIndexOf(resourcesWord, StringComparison.InvariantCultureIgnoreCase)).ToLower();

			//If the dictionary does not already contain this module, add it
			if (!dictionary.ContainsKey(moduleName))
				dictionary.Add(moduleName, new List<string>());

			//Get all files paths in this folder
			string[] files = Directory.GetFiles(folder);
			for (int i = 0; i < files.Length; i++)
			{
				//Skip .meta files
				if (files[i].EndsWith(metaWord))
					continue;
				FileInfo file = new FileInfo(files[i]);
				//Compose the value of the constant for the given file path, minus the extension
				string value = (moduleName + "/" + file.Name.Substring(0, file.Name.Length - file.Extension.Length));
				//Cut "resources" off of value
				value = value.Substring(value.IndexOf('/') + 1);

				//If there is more than one file with the same value, only add 1
				if (!dictionary[moduleName].Contains(value))
					dictionary[moduleName].Add(value);
			}
		}

		//=====================================
		//Preferences
		//=====================================
		public override void DrawPreferences()
		{
			// list.DrawLayout(); //todo: redo filter
			base.DrawPreferences();
		}
	}
}
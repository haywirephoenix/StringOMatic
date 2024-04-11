using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

namespace SOM{
	/// <summary>
	/// The Resources Module adds info about every file in the Resources folders of the project.
	/// It adds every folder as a Module, and every file in that folder as a constant. 
	/// The constant contains the path to the Resource to be used with the Resources class.
	/// Also, you can specify black or white filtering.
	/// </summary>
	public class SOMResourcesModule:SOMModule{
		//=====================================
		//Consts
		//=====================================
		const string resourcesWord = "resources";
		const string metaWord = "meta";

		//=====================================
		//Vars
		//=====================================
		SOMUtils.FilterList _list;

		//=====================================
		//Properties
		//=====================================
		SOMUtils.FilterList list{
			get{
				if (_list == null)
					_list = new SOMUtils.FilterList(moduleName, moduleName, SOMUtils.FilterList.FilterType.Black);
				return _list;
			}
		}

		public override string moduleName{
			get{return "Resources";}
		}

		//=====================================
		//Refresh
		//=====================================
		public override void Refresh(){
			//Get all resources. The Key contains the folder name/path, and the Value a list with all of the objects in it
			Dictionary<string,List<string>> resources = GetResources();

			//For each resource
			foreach (KeyValuePair<string, List<string>> resource in resources){

				//Check if this folder is in the filter
				if (list.hasFilter){
					bool isFiltered = false;
					string comparer = (resource.Key+"/").Substring(resourcesWord.Length+1);
					for (int i = 0; i < list.values.Length; i++){
						if (list.values[i].EndsWith("/")){
							if (comparer.Trim().StartsWith(list.values[i].Trim(),StringComparison.InvariantCultureIgnoreCase)){
								isFiltered = true;
								break;
							}
						}
					}
					//And act accordingly...
					if (!(isFiltered^list.isBlack))
						continue;
				}

				string module = resource.Key.Replace('/','.');
				//If this folder doesn't exist as a module, add one
				if (!SOMXmlHandler.ModuleExists(module))
					SOMXmlHandler.AddModule(module);
				
				//Add the path to the folder
				if (module!=resourcesWord)
					SOMXmlHandler.AddConstant(module, "path", resource.Key.Substring(resourcesWord.Length+1)+"/"); 

				//Foreach object in the folder
				for (int i = 0; i < resource.Value.Count; i++){

					//Check if this object's path is in the filter
					if (list.hasFilter){
						bool isFiltered = false;
						for (int j = 0; j < list.values.Length; j++){
							if (resource.Value[i].Trim().Equals(list.values[j].Trim(),StringComparison.InvariantCultureIgnoreCase)){
								isFiltered = true;
								break;
							}
						}
						//And act accordingly...
						if (!(isFiltered^list.isBlack))
							continue;
					}

					//Finally, add the resource path as a constant
					string constant = SOMUtils.NicifyConstantName(resource.Value[i].Substring(resource.Value[i].LastIndexOf('/')+1));
					SOMXmlHandler.AddConstant(module, constant, resource.Value[i]); 
				}
			}
		}

		Dictionary<string,List<string>> GetResources(){
			//Get every "Resources" folder in the Assets folder 
			string[] rootResources = Directory.GetDirectories(Application.dataPath,resourcesWord,SearchOption.AllDirectories);

			//KEY--> Paths to every Resources folder and sub-folders
			//VALUE--> A list with all objects' paths inside that folder
			Dictionary<string,List<string>> dictionary = new Dictionary<string, List<string>>();

			for (int i = 0; i < rootResources.Length; i++){
				//Get every subfolder in this rootFolder folder
				List<string> subDirectories = new List<string>(Directory.GetDirectories(rootResources[i], "*",SearchOption.AllDirectories));
				//And add the rootFolder itself
				subDirectories.Insert(0,rootResources[i]);

				//Reorder array in order to mimic a depth-first search
				for (int j = 1; j < subDirectories.Count; j++){
					for (int k = 0; k < j; k++){
						if (subDirectories[j].StartsWith(subDirectories[k])){
							subDirectories.Insert(k+1,subDirectories[j]);
							subDirectories.RemoveAt(j+1);
						}
					}
				}
				//For each sub-folder...
				for (int j = 0; j < subDirectories.Count; j++){
					//Check if one of the sub directories is also a rootResources folder. If it is, skip it to avoid duplicates and stuff
					bool isRoot = false;
					for (int k = i+1; k < rootResources.Length; k++){
						if (subDirectories[j].StartsWith(rootResources[k])){
							isRoot = true;
							break;
						}
					}
					if (isRoot)
						continue;

					//Check if editor folder. If true, skip it
					subDirectories[j] = subDirectories[j].Replace('\\', '/');
					if (subDirectories[j].IndexOf("/editor/",StringComparison.InvariantCultureIgnoreCase)!=-1 || 
						subDirectories[j].EndsWith("/editor",StringComparison.InvariantCultureIgnoreCase))
						continue;

					//Time to find every resource in this sub-folder
					AddConstants(dictionary, subDirectories[j]);
				}
			}
			return dictionary;
		}
		void AddConstants(Dictionary<string, List<string>> dictionary, string folder){
			//Module name is taken from the last "resource" occurrence onwards
			string moduleName = folder.Substring(folder.LastIndexOf(resourcesWord,StringComparison.InvariantCultureIgnoreCase)).ToLower();

			//If the dictionary does not already contain this module, add it
			if (!dictionary.ContainsKey(moduleName))
				dictionary.Add(moduleName, new List<string>());

			//Get all files paths in this folder
			string[] files = Directory.GetFiles(folder);
			for (int i = 0; i < files.Length; i++){
				//Skip .meta files
				if (files[i].EndsWith(metaWord))
					continue;
				FileInfo file = new FileInfo(files[i]);
				//Compose the value of the constant for the given file path, minus the extension
				string value = (moduleName+"/"+file.Name.Substring(0,file.Name.Length-file.Extension.Length));
				//Cut "resources" off of value
				value = value.Substring(value.IndexOf('/')+1);

				//If there is more than one file with the same value, only add 1
				if (!dictionary[moduleName].Contains(value))
					dictionary[moduleName].Add(value);
			}
		}

		//=====================================
		//Preferences
		//=====================================
		public override void DrawPreferences(){
			list.DrawLayout();
			base.DrawPreferences();
		}
	}
}
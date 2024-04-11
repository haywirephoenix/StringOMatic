using UnityEditor;
using UnityEditor.Audio;
using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace SOM{
	/// <summary>
	/// The Audio Module looks for every Audio Mixer in the project. It then saves their names, their snapshots and their properties
	/// </summary>
	public class SOMAudioModule : SOMModule {

		//==============================
		//Consts
		//==============================
		const string ADD_NAME_KEY = "Audio Module Add Name";
		const string ADD_PARAMETERS_KEY = "Audio Module Add Parameters";
		const string ADD_SNAPSHOTS_KEY = "Audio Module Add Snapshots";

		const string ADD_NAME_LABEL = "Add Name";
		const string ADD_PARAMETERS_LABEL = "Add Parameters";
		const string ADD_SNAPSHOTS_LABEL = "Add Snapshots";

		const string ADD_NAME_TOOLTIP = "Whether or not to add a constant containing the name of the mixer for each mixer";
		const string ADD_PARAMETERS_TOOLTIP = "Whether or not to add a module containing a list of exposed constant names for each mixer";
		const string ADD_SNAPSHOTS_TOOLTIP = "Whether or not to add a module containing a list of snapshots for each mixer";

		//==============================
		//Properties
		//==============================
		public override string moduleName {
			get {return "Audio";}
		}
		static bool addName{
			get{
				if (!SOMPreferences.bools.Contains(ADD_NAME_KEY))
					addName = true;
				return SOMPreferences.bools[ADD_NAME_KEY];
			}
			set{
				SOMPreferences.bools[ADD_NAME_KEY] = value;
			}
		}
		static bool addParameters{
			get{
				if (!SOMPreferences.bools.Contains(ADD_PARAMETERS_KEY))
					addParameters = true;
				return SOMPreferences.bools[ADD_PARAMETERS_KEY];
			}
			set{
				SOMPreferences.bools[ADD_PARAMETERS_KEY] = value;
			}
		}
		static bool addSnapshots{
			get{
				if (!SOMPreferences.bools.Contains(ADD_SNAPSHOTS_KEY))
					addSnapshots = true;
				return SOMPreferences.bools[ADD_SNAPSHOTS_KEY];
			}
			set{
				SOMPreferences.bools[ADD_SNAPSHOTS_KEY] = value;
			}
		}

		//==============================
		//Refresh
		//==============================
		public override void Refresh(){
			string rootMixersModule = moduleName+".mixers";
			SOMXmlHandler.AddModule(rootMixersModule);

			//AudioMixerWindow has a static method to return all mixer controllers in the project
			Type audioMixerWindow = Type.GetType("UnityEditor.AudioMixerWindow, UnityEditor");
			MethodInfo method = audioMixerWindow.GetMethod("FindAllAudioMixerControllers",BindingFlags.Static | BindingFlags.NonPublic);
			IList mixers = method.Invoke(null, null) as IList;

			for (int i = 0; i < mixers.Count; i++){
				//For each mixer, add a module with its name and a constant containing its name...
				string mixerName = mixers[i].GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public).GetValue(mixers[i], null) as string;
				string mixerModule = rootMixersModule+"."+mixerName;
				SOMXmlHandler.AddModule(mixerModule);
				if (addName)
					SOMXmlHandler.AddConstant(mixerModule, "name", mixerName);

				//And its exposed parameters names
				if (addParameters){
					string parametersModule = mixerModule+".parameters";
					SOMXmlHandler.AddModule(parametersModule);
					AddParameters(mixers[i], parametersModule);
				}
					
				//And snapshots names
				if (addSnapshots){
					string snapshotsModule = mixerModule+".snapshots";
					SOMXmlHandler.AddModule(snapshotsModule);
					AddSnapshots(mixers[i], snapshotsModule);
				}
			}
		}

		//Given an AudioMixerController, add a constant for each of its exposed parameters
		void AddParameters(object audioMixer, string parentModule){
			Array properties = audioMixer.GetType().GetProperty("exposedParameters", BindingFlags.Public | BindingFlags.Instance).GetValue(audioMixer, null) as Array;
			for (int j = 0; j < properties.Length; j++){
				string parameter = properties.GetValue(j).GetType().GetField("name", BindingFlags.Instance | BindingFlags.Public).GetValue(properties.GetValue(j)) as string;
				SOMXmlHandler.AddConstant(parentModule,SOMUtils.NicifyConstantName(parameter), parameter);
			}
		}
		//Given an AudioMixerController, add a constant for each of its snapshot names
		void AddSnapshots(object audioMixer, string parentModule){
			Array snapshots = audioMixer.GetType().GetProperty("snapshots", BindingFlags.Public | BindingFlags.Instance).GetValue(audioMixer, null) as Array;
			for (int j = 0; j < snapshots.Length; j++){
				string snapshot = snapshots.GetValue(j).GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public).GetValue(snapshots.GetValue(j), null) as string;
				SOMXmlHandler.AddConstant(parentModule,SOMUtils.NicifyConstantName(snapshot), snapshot);
			}
		}

		//==============================
		//Preferences
		//==============================
		public override void DrawPreferences(){
			addName = EditorGUILayout.ToggleLeft(new GUIContent(ADD_NAME_LABEL, ADD_NAME_TOOLTIP), addName);
			addParameters = EditorGUILayout.ToggleLeft(new GUIContent(ADD_PARAMETERS_LABEL, ADD_PARAMETERS_TOOLTIP), addParameters);
			addSnapshots = EditorGUILayout.ToggleLeft(new GUIContent(ADD_SNAPSHOTS_LABEL, ADD_SNAPSHOTS_TOOLTIP), addSnapshots);
			base.DrawPreferences();
		}
	}
}
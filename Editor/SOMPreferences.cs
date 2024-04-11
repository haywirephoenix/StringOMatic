using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SOM{
	/// <summary>
	/// This class mimics the behaviour of classes like PlayerPrefs or EditorPrefs, excepts that this is project-dependant.
	/// Values are added or retrieved through the indexer property.
	/// </summary>
	public class SOMPreferences : ScriptableObject {

		//==================================
		//Consts
		//==================================
		const string NAME = "SOMPreferences";

		//==================================
		//Singleton
		//==================================
		static SOMPreferences singleton;
		static SOMPreferences Singleton{
			get{
				//Get an existing file or create a new one
				if (singleton == null){ 
					string[] guids = AssetDatabase.FindAssets(NAME+ " t:"+typeof(SOMPreferences).Name);
					if (guids.Length == 0){
						singleton = ScriptableObject.CreateInstance<SOMPreferences>();
						guids = AssetDatabase.FindAssets(typeof(SOMPreferences).Name);
						if (guids.Length == 0)
							throw new FileNotFoundException("File could not be found");
						string targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
						targetPath = targetPath.Substring(0, targetPath.LastIndexOf("/")); 
						AssetDatabase.CreateAsset(singleton, targetPath+"/"+NAME+".asset");
						AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(singleton));
					}
					else
						singleton = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(SOMPreferences)) as SOMPreferences;
				}
				return singleton;
			}
		}

		//==================================
		//Constants
		//==================================
		[SerializeField]
		BoolKeyCollection _bools;
		[SerializeField]
		StringKeyCollection _strings;
		[SerializeField]
		IntKeyCollection _ints;
		[SerializeField]
		FloatKeyCollection _floats;

		//==================================
		//Properties
		//==================================
		/// <summary>
		/// Gets or sets a bool value
		/// </summary>
		public static KeyCollection<bool> bools{ 
			get{ 
				if (Singleton._bools == null)
					Singleton._bools = new BoolKeyCollection(); 
				return Singleton._bools;
			}
		}
		/// <summary>
		/// Gets or sets a string value
		/// </summary>
		public static KeyCollection<string> strings{ 
			get{ 
				if (Singleton._strings == null)
					Singleton._strings = new StringKeyCollection(); 
				return Singleton._strings;
			}
		}
		/// <summary>
		/// Gets or sets an int value
		/// </summary>
		public static KeyCollection<int> ints{
			get{
				if (Singleton._ints == null)
					Singleton._ints = new IntKeyCollection();
				return Singleton._ints;
			}	
		}
		/// <summary>
		/// Gets or sets a float value
		/// </summary>
		public static KeyCollection<float> floats{
			get{
				if (Singleton._floats == null)
					Singleton._floats = new FloatKeyCollection();
				return Singleton._floats;
			}	
		}

		//==================================
		//Methods
		//==================================
		/// <summary>
		/// Tells Unity to serialize and save this object
		/// </summary>
		public static void Save(){
			EditorUtility.SetDirty(Singleton);
		}

		//==================================
		//Classes
		//==================================
		[System.Serializable]
		/// <summary>
		/// Mimics the behaviour of a simplistic Dictionary
		/// </summary>
		public abstract class KeyCollection<T>{
			[SerializeField]
			List<string> _names;
			[SerializeField]
			List<T> _values;

			protected abstract T defaultValue{get;}
			
			List<string> names{
				get{
					if (_names == null)
						_names = new List<string>();
					return _names;
				}
			}
			List<T> values{
				get{
					if (_values == null)
						_values = new List<T>();
					return _values;
				}
			}

			/// <summary>
			/// Whether this collection contains the specified key
			/// </summary>
			/// <param name="name">Name.</param>
			public bool Contains(string name){
				return names.Contains(name);
			}

			/// <summary>
			/// Gets or sets the key with the specified name. If it does not exist, it is created.
			/// </summary>
			/// <param name="name">Name.</param>
			public T this[string name]{
				get{
					if (!Contains(name)){
						names.Add(name);
						values.Add(defaultValue);
					}
					return values[names.IndexOf(name)];
				}
				set{
					if (!Contains(name)){
						names.Add(name);
						values.Add(defaultValue);
					}
					values[names.IndexOf(name)] = value;
				}
			}
			/// <summary>
			/// Delete the specified key
			/// </summary>
			public void Delete(string name){
				if (Contains(name)){
					int index = names.IndexOf(name);
					names.RemoveAt(index);
					values.RemoveAt(index);
				}
			}
		}
		[System.Serializable]
		class BoolKeyCollection:KeyCollection<bool>{
			protected override bool defaultValue {get {return true;}}
		}
		[System.Serializable]
		class IntKeyCollection:KeyCollection<int>{
			protected override int defaultValue {get {return 0;}}
		}
		[System.Serializable]
		class StringKeyCollection:KeyCollection<string>{
			protected override string defaultValue {get {return string.Empty;}}
		}
		[System.Serializable]
		class FloatKeyCollection:KeyCollection<float>{
			protected override float defaultValue {get {return 0;}}
		}
	}

	//==================================
	//Editor
	//==================================
	[CustomEditor(typeof(SOMPreferences))]
	public class SOMPReferencesEditor:Editor{
		public override void OnInspectorGUI(){
			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("Open Preferences"))
				SOMManager.OpenPreferences();
			GUI.backgroundColor = oldColor;
			base.OnInspectorGUI();
		}
	}
}

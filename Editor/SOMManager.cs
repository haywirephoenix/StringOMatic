using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace SOM{
	/// <summary>
	/// Provides an entry point for the rest of the code.
	/// </summary>
	public static class SOMManager{

		//=========================================
		//Consts
		//=========================================
		const string VERSION = "1.1.0";
		const string PREFERENCES_TAB = "String-O-Matic";
		const string MENU_TAB = "Tools/SOM";

		//=========================================
		//Properties
		//=========================================
		//Get a list of all classes that inherit from SOMModule
		static List<SOMModule> _modules;
		static List<SOMModule> modules{
			get{
				if (_modules == null){
					_modules = new List<SOMModule>();
					Type[] types = typeof(SOMManager).Assembly.GetTypes();
					for (int i = 0; i < types.Length; i++)
						if (types[i].IsSubclassOf(typeof(SOMModule)))
							modules.Add((SOMModule)Activator.CreateInstance(types[i]));
				}
				return _modules;
			}
		}
		//Gets the header image
		static Texture _header;
		static Texture header{
			get{
				if (_header == null)
					_header = Resources.Load<Texture>("SOMTitle");
				return _header;
			}
		}

		//=========================================
		//Refresh
		//=========================================
		/// <summary>
		/// Refreshes every module that needs to be updated or forces them to do so.
		/// </summary>
		/// <param name="forceUpdate">If set to <c>true</c> force update.</param>
		public static void RefreshAll(bool forceUpdate = false){
			for (int i = 0; i < modules.Count; i++)
				if (modules[i].needsRefreshing || forceUpdate)
					Refresh(i, false);
		}
		/// <summary>
		/// Refresh the specified module and save.
		/// </summary>
		/// <param name="moduleName">Module name.</param>
		/// <param name="save">If set to <c>true</c>, save.</param>
		public static void Refresh(string moduleName, bool save = true){
			for (int i = 0; i < modules.Count; i++){
				if (modules[i].moduleName.Equals(moduleName)){
					Refresh(i, save);
					break;
				}
			}
		}
		static void Refresh(int index, bool save = true){
			if (!SOMXmlHandler.DocumentExists())
				SOMXmlHandler.CreateDocument();

			SOMModule module = modules[index];
			if (SOMXmlHandler.ModuleExists(module.moduleName))
				SOMXmlHandler.RemoveModule(module.moduleName);
			
			SOMXmlHandler.AddModule(module.moduleName);
			module.Refresh();

			if (save)
				SOMXmlHandler.Save();
		}

		//=========================================
		//Menu
		//=========================================
		static Vector2 scrollPos;
		static int selected = -1;

		[PreferenceItem(PREFERENCES_TAB)]
		static void OnPreferences(){
			//Draw Header
			EditorGUI.DrawPreviewTexture(new Rect(130, 5, 360, 45), header);

			int controlId = GUIUtility.GetControlID("SOMPreferences".GetHashCode(), FocusType.Keyboard);
			GUILayout.BeginVertical();
			GUIStyle style = new GUIStyle((GUIStyle) "OL Title");
			GUILayout.Space(20);

			scrollPos = GUILayout.BeginScrollView(scrollPos,(GUIStyle) "OL Box");

			for (int i = 0; i < modules.Count; i++){
				EditorGUILayout.BeginHorizontal();
				//Check boxes to update modules
				modules[i].needsRefreshing = GUILayout.Toggle(modules[i].needsRefreshing,string.Empty,GUILayout.Width(13));
				EditorGUILayout.BeginVertical();
				EditorGUI.BeginChangeCheck();

				//Select the module we click over. Or deselect it if it's already selected
				bool thisIsSelected = i== selected;
				GUILayout.Toggle(thisIsSelected, modules[i].moduleName, style);
				if (EditorGUI.EndChangeCheck()){
					selected = thisIsSelected?-1:i;
					GUIUtility.keyboardControl = controlId;
				}
				//Draw preferences for the selected module
				if (thisIsSelected){
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					modules[i].DrawPreferences();
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			//Move up & Down
			if (GUIUtility.keyboardControl == controlId && Event.current.type == EventType.KeyDown){
				switch (Event.current.keyCode){
				case KeyCode.UpArrow:
					selected = selected <= 0? 0: selected-1;
					Event.current.Use();
					break;
				case KeyCode.DownArrow:
					selected = selected == modules.Count-1? selected:selected+1;
					Event.current.Use();
					break;
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(5);
			if (GUILayout.Button("Refresh"))
				RefreshMenu();
			if (GUI.changed)
				SOMPreferences.Save(); 

			//Draw Footer
			GUILayout.Space(5);
			//Draw line
			Rect rect = EditorGUILayout.GetControlRect();
			rect.height = 2;
			EditorGUI.DrawRect(rect, Color.black);
			//Draw Version
			GUILayout.Space(-15);
			TextAnchor oldAllignment = EditorStyles.miniLabel.alignment;
			EditorStyles.miniLabel.alignment = TextAnchor.MiddleRight;
			EditorStyles.miniLabel.richText = true;
			EditorGUILayout.LabelField("<b>String-O-Matic</b> version " + VERSION, EditorStyles.miniLabel);
			EditorStyles.miniLabel.richText = false;
			EditorStyles.miniLabel.alignment = oldAllignment;
			//Draw Buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Forum Thread", EditorStyles.miniButton))
				Application.OpenURL("http://forum.unity3d.com/threads/string-o-matic-say-goodbye-to-magic-strings.377123/");
			if (GUILayout.Button("Asset Store page", EditorStyles.miniButton))
				Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/53019");
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		[MenuItem(MENU_TAB+"/Refresh %#r")]
		public static void RefreshMenu(){
			RefreshAll();
			SOMXmlHandler.Save();
			SOMCSHarpHandler.Compile();
		}

		[MenuItem(MENU_TAB+"/Preferences %#c")]
		public static void OpenPreferences(){
			SOMUtils.OpenPreferences(PREFERENCES_TAB);
		}
	}
}
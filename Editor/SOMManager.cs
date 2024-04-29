using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;

namespace SOM
{
	/// <summary>
	/// Provides an entry point for the rest of the code.
	/// </summary>
	public static class SOMManager
	{

		//=========================================
		//Consts
		//=========================================
		const string VERSION = "2.0.4";
		const string PREFERENCES_TAB = "String-O-Matic";
		const string MENU_TAB = "Tools/StringOMatic";

		public const string CONTROL_ID = "SOMPreferences";
		public const string CLASSNAME_KEY = "File Name";
		public const string NAMESPACE_KEY = "Namespace";
		public const string TARGET_DIRECTORY_KEY = "Target Directory";
		public const string GENERATE_CS_KEY = "GenerateCS";
		public const string WRITE_COMMENT_KEY = "WriteComment";
		public const string WRAP_NAMESPACES_KEY = "WrapNamespaces";
		public const string VERSION_LABEL = "version ";

		public const string DEFAULT_CLASS = "StringOMatic";
		public const string DEFAULT_NAMESPACE = "StringOMatic";
		public const string DEFAULT_TARGETDIR = "Assets/StringOMatic/";

		const string SOMTitle = "SOMTitle";
		const string GENERATE_CS_FILE = "Generate CS file";
		const string WRITE_COMMENT = "Write comment at top";
		const string WRAP_NAMESPACES = "Wrap modules in namespaces";

		const string REFRESH_ALL = "Refresh All";
		const string FORUM_THREAD = "Forum Thread";
		const string FORUM_URL = "http://forum.unity3d.com/threads/string-o-matic-say-goodbye-to-magic-strings.377123/";
		const string ASSET_STORE_PAGE = "Asset Store page";
		const string ASSET_STORE_URL = "https://www.assetstore.unity3d.com/#!/content/53019";
		const string GITHUB_REPO = "Github Repo";
		const string GITHUB_REPO_URL = "https://github.com/haywirephoenix/StringOMatic/";


		//=========================================
		//Properties
		//=========================================
		//Get a list of all classes that inherit from SOMModule
		static bool generateCS;
		// static bool generateXML;
		static bool allEnabled;
		static bool classEnabled;
		static bool namespaceEnabled;
		static bool targetDirEnabled;
		static bool wrapModuleNamepsacesEnabled;
		static bool writeCommentEnabled;
		static string classNameText;
		static string namespaceText;
		static string targetDirectory;
		static List<SOMModule> _modules;
		static List<SOMModule> modules
		{
			get
			{
				if (_modules == null)
				{
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
		static Texture header
		{
			get
			{
				if (_header == null)
					_header = Resources.Load<Texture>(SOMTitle);
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
		public static void RefreshAll(bool forceUpdate = false)
		{
			for (int i = 0; i < modules.Count; i++)
				if (modules[i].needsRefreshing || forceUpdate)
					Refresh(i, false);
		}
		/// <summary>
		/// Refresh the specified module and save.
		/// </summary>
		/// <param name="moduleName">Module name.</param>
		/// <param name="save">If set to <c>true</c>, save.</param>
		public static void Refresh(string moduleName, bool save = true)
		{
			for (int i = 0; i < modules.Count; i++)
			{
				if (modules[i].moduleName.Equals(moduleName))
				{
					Refresh(i, save);
					break;
				}
			}
		}
		static void Refresh(int index, bool save = true)
		{

			if (!SOMDataHandler.DatabaseExists())
				SOMDataHandler.CreateDatabase();

			SOMModule module = modules[index];
			// if (SOMDataHandler.ModuleExists(module.moduleName))
			// 	SOMDataHandler.RemoveModule(module.moduleName);

			// //SOMDataHandler.AddModule(module.moduleName);
			module.Refresh();

			if (save)
				SOMDataHandler.Save();


		}

		//=========================================
		//Menu
		//=========================================
		static Vector2 scrollPos;
		static int selected = -1;
		private static Rect _rect;
		private static Rect GetLastRect()
		{
			GUILayout.Label(" ", GUILayout.MaxHeight(0));
			if (Event.current.type == EventType.Repaint)
			{
				// hack to get real view width
				_rect = GUILayoutUtility.GetLastRect();
			}

			return _rect;
		}

		static bool DrawToggleTextField(ref string textref, string key, string defaultText, ref bool fieldEnabled, bool defaultEnabled = false)
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();

			bool isEnabled = GUILayout.Toggle(SOMPreferences.GetBoolFromPrefs(key, defaultEnabled), string.Empty, GUILayout.Width(13));

			using (new EditorGUI.DisabledScope(!isEnabled))
			{
				textref = EditorGUILayout.TextField(key, SOMPreferences.GetStringFromPrefs(key, defaultText));
			}

			if (EditorGUI.EndChangeCheck())
			{
				fieldEnabled = isEnabled;

				SOMPreferences.SetBoolInPrefs(key, isEnabled);

				if (!fieldEnabled)
				{
					ResetToggleTextField(ref textref, key, defaultText);
				}
				else
				{
					SaveToggleTextField(key, textref, defaultText);
				}

				EditorWindow.GetWindow<UnityEditor.EditorWindow>().Repaint();
			}

			EditorGUILayout.EndHorizontal();

			return fieldEnabled;
		}



		static void SaveToggleTextField(string key, string newValue, string defaultText)
		{
			string safeValue = SOMUtils.GetDefaultStringIfEmpty(newValue, defaultText);
			if (defaultText == DEFAULT_TARGETDIR)
				safeValue = SOMUtils.CleanPath(safeValue);
			SOMPreferences.SetStringInPrefs(key, safeValue);
		}

		static void ResetToggleTextField(ref string textref, string key, string defaultText)
		{
			textref = defaultText;
			SOMPreferences.SetStringInPrefs(key, defaultText);
		}

		static bool DrawToggleField(ref bool boolref, string label, string key, bool defaultValue)
		{
			EditorGUI.BeginChangeCheck();
			boolref = GUILayout.Toggle(SOMPreferences.GetBoolFromPrefs(key, defaultValue), label);
			if (EditorGUI.EndChangeCheck())
			{
				SaveToggleField(boolref, key, boolref);
			}
			return boolref;
		}
		static void SaveToggleField(this bool boolref, string key, bool newValue)
		{
			boolref = newValue;
			SOMPreferences.SetBoolInPrefs(key, newValue);
		}

		private static bool Init = false;

		static void OnInit()
		{
			Init = true;
		}



#if UNITY_2018_3_OR_NEWER
		private class SOMSettingsProvider : SettingsProvider
		{
			public SOMSettingsProvider(string path, SettingsScope scopes = SettingsScope.User)
			: base(path, scopes)
			{ }

			public override void OnGUI(string searchContext)
			{
				OnPreferences();
			}
		}

		[SettingsProvider]
		static SettingsProvider OnPreferencesNew()
		{
			return new SOMSettingsProvider("Preferences/" + PREFERENCES_TAB);
		}
#else
		[PreferenceItem(PREFERENCES_TAB)]
#endif


		static void OnPreferences()
		{

			if (!Init)
			{

				OnInit();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			Rect scale = GUILayoutUtility.GetLastRect();

			int controlId = GUIUtility.GetControlID(CONTROL_ID.GetHashCode(), FocusType.Keyboard);
			GUILayout.BeginVertical();
			GUIStyle style = new GUIStyle((GUIStyle)"OL Title");

			//add the text fields
			GUILayout.Space(20);
			// generateCS.DrawToggleField(GENERATE_CS_FILE, GENERATE_CS_KEY, true);
			DrawToggleField(ref writeCommentEnabled, WRITE_COMMENT, WRITE_COMMENT_KEY, true);
			DrawToggleField(ref wrapModuleNamepsacesEnabled, WRAP_NAMESPACES, WRAP_NAMESPACES_KEY, false);
			// generateXML.DrawToggleField("Generate XML file", "GenerateXML", false);
			GUILayout.Space(20);


			DrawToggleTextField(ref classNameText, CLASSNAME_KEY, DEFAULT_CLASS, ref classEnabled);

			string projectNamespace = EditorSettings.projectGenerationRootNamespace;
			string defaultNamespace = string.IsNullOrEmpty(projectNamespace) ? DEFAULT_NAMESPACE : projectNamespace;

			DrawToggleTextField(ref namespaceText, NAMESPACE_KEY, defaultNamespace, ref namespaceEnabled);

			DrawToggleTextField(ref targetDirectory, TARGET_DIRECTORY_KEY, DEFAULT_TARGETDIR, ref targetDirEnabled);

			GUILayout.Space(20);



			//begin the modules scrollview
			scrollPos = GUILayout.BeginScrollView(scrollPos, (GUIStyle)"OL Box");


			for (int i = 0; i < modules.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				//Check boxes to update modules
				modules[i].needsRefreshing = GUILayout.Toggle(modules[i].needsRefreshing, string.Empty, GUILayout.Width(13));
				EditorGUILayout.BeginVertical();
				EditorGUI.BeginChangeCheck();

				//Select the module we click over. Or deselect it if it's already selected
				bool thisIsSelected = i == selected;
				GUILayout.Toggle(thisIsSelected, modules[i].moduleName, style);
				if (EditorGUI.EndChangeCheck())
				{
					selected = thisIsSelected ? -1 : i;
					GUIUtility.keyboardControl = controlId;
				}
				//Draw preferences for the selected module
				if (thisIsSelected)
				{
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					modules[i].DrawPreferences();
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}

			//Move up & Down
			if (GUIUtility.keyboardControl == controlId && Event.current.type == EventType.KeyDown)
			{
				switch (Event.current.keyCode)
				{
					case KeyCode.UpArrow:
						selected = selected <= 0 ? 0 : selected - 1;
						Event.current.Use();
						break;
					case KeyCode.DownArrow:
						selected = selected == modules.Count - 1 ? selected : selected + 1;
						Event.current.Use();
						break;
				}
			}
			GUILayout.EndScrollView();

			if (GUILayout.Button(REFRESH_ALL))
				RefreshMenu();
			if (GUI.changed)
				SOMPreferences.Save();

			GUILayout.Space(25);
			//Draw Footer
			EditorGUI.DrawPreviewTexture(new Rect(scale.width / 2 - 180, GetLastRect().y - 20, 360, 45), header);
			GUILayout.Space(10);
			//Draw Version
			// GUILayout.Space(-10);
			TextAnchor oldAllignment = EditorStyles.miniLabel.alignment;
			EditorStyles.miniLabel.alignment = TextAnchor.MiddleCenter;
			EditorStyles.miniLabel.richText = true;
			EditorStyles.miniLabel.contentOffset = new Vector2(50, 0);
			EditorGUILayout.LabelField(VERSION_LABEL + VERSION, EditorStyles.miniLabel);
			EditorStyles.miniLabel.richText = false;
			EditorStyles.miniLabel.alignment = oldAllignment;
			//Draw Buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(FORUM_THREAD, EditorStyles.miniButton))
				Application.OpenURL(FORUM_URL);
			if (GUILayout.Button(GITHUB_REPO, EditorStyles.miniButton))
				Application.OpenURL(GITHUB_REPO_URL);
			if (GUILayout.Button(ASSET_STORE_PAGE, EditorStyles.miniButton))
				Application.OpenURL(ASSET_STORE_URL);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		[MenuItem(MENU_TAB + "/Refresh %#r")]
		public static void RefreshMenu()
		{
			SOMDataHandler.CreateDatabase();
			RefreshAll();
			SOMDataHandler.Save();

			// if (SOMPreferences.GetBoolFromPrefs(GENERATE_CS_KEY, generateCS))
			SOMCSHarpHandler.Compile();
		}

		[MenuItem(MENU_TAB + "/Preferences %#c")]
		public static void OpenPreferences()
		{
			SOMUtils.OpenPreferences(PREFERENCES_TAB);
		}
	}
}
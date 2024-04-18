using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;

namespace SOM
{
	/// <summary>
	/// Provides some extra functionality
	/// </summary>
	public static class SOMUtils
	{

		public const Char dotChar = '.';
		public const Char colonChar = ':';
		public const Char underscoreChar = '.';
		public const String dotStr = ".";

		//=====================================
		//Nicify Methods
		//=====================================
		static string[] keyWords = new string[]{
			"abstract","as","base","bool",
			"break", "byte", "case", "catch",
			"char", "checked", "class", "const",
			"continue", "decimal", "default", "delegate",
			"do", "double", "else", "eneum",
			"event", "explicit", "extern", "false",
			"finally", "fixed", "float", "for",
			"foreach", "goto", "if", "impicit",
			"in", "int", "interface", "internal",
			"is", "lock", "long", "namespace",
			"new", "null", "object", "operator",
			"out", "override", "params", "private",
			"protected", "public", "readonly", "ref",
			"return", "sbyte", "sealde", "short",
			"sizeof", "stackalloc", "static", "string",
			"struct", "switch", "this", "throw",
			"true", "try", "typeof", "uint",
			"ulong", "unchecked", "unsafe", "ushort",
			"using", "virtual", "void", "volatile", "while"
		};

		private const string MODULE_SUFFIX = "Module";

		public static string RemoveWhitespace(this string str)
		{
			return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
		}

		/// <summary>
		/// Nicifies the name of the module to make it PascalCase. 
		/// Also, any numbers at the start of the name are moved back to the end.
		/// </summary>
		public static string NicifyModuleName(string name)
		{
			string newName = name.Trim();
			newName = RemoveWhitespace(newName);
			if (string.IsNullOrEmpty(newName))
				return newName;
			//Capitalize words and remove spaces
			int index = 0;
			do
			{
				newName = newName.Insert(index, Char.ToUpper(newName[index]).ToString()).Remove(index + 1, 1);
				index = newName.IndexOf(' ', index) + 1;
			} while (index != 0);
			newName = newName.Replace(" ", string.Empty);

			//Remove non alpha-numeric characters
			for (int i = newName.Length - 1; i >= 0; i--)
				if (!char.IsLetterOrDigit(newName, i))
					newName = newName.Remove(i, 1);
			//Move numbers to the end
			index = 0;
			bool isNumber = false;
			do
			{
				bool nextCharIsDigit = char.IsDigit(newName[index]);
				if (!nextCharIsDigit)
					break;
				index++;
				//This means the string is a whole number without alphabetic characters
				if (index >= newName.Length)
				{
					isNumber = true;
					newName = "n" + newName;
					break;
				}
			} while (true);
			newName += newName.Substring(isNumber ? 1 : 0, index);
			newName = newName.Remove(isNumber ? 1 : 0, index);

			return newName;
		}
		/// <summary>
		/// Nicifies the name of the constant to make it camelCase.
		/// If the name coincides with a keyword, it is converted make into PascalCase.
		/// </summary>
		public static string NicifyConstantName(string name)
		{
			name = NicifyModuleName(name);
			if (string.IsNullOrEmpty(name))
				return name;

			//Turn the first letter into lowercase
			string finalName = name.Insert(0, Char.ToLower(name[0]).ToString()).Remove(1, 1);

			//If isKeyword, revert back to uppercase
			if (IsKeyword(finalName))
				finalName += "_";
			return finalName;
		}
		/// <summary>
		/// Checks whether or not the specified string represents a C# keyword name.
		/// </summary>
		public static bool IsKeyword(string name)
		{
			bool isKeyword = false;
			for (int i = 0; i < keyWords.Length; i++)
			{
				if (string.Equals(name, keyWords[i]))
				{
					isKeyword = true;
					break;
				}
			}
			return isKeyword;
		}

		//=====================================
		//Logs
		//=====================================
		public static void Log(string text)
		{
			Log(LogType.Log, text);
		}
		public static void Log(string format, params object[] args)
		{
			Log(string.Format(format, args));
		}
		public static void LogWarning(string text)
		{
			Log(LogType.Warning, text);
		}
		public static void LogWarning(string format, params object[] args)
		{
			LogWarning(string.Format(format, args));
		}
		public static void LogError(string text)
		{
			Log(LogType.Error, text);
		}
		public static void LogError(string format, params object[] args)
		{
			LogError(string.Format(format, args));
		}
		public static void Log(LogType logType, string format, params object[] args)
		{
			Log(logType, string.Format(format, args));
		}
		/// <summary>
		/// Log the specified logType and message.
		/// </summary>
		public static void Log(LogType logType, string message)
		{
			Action<string, object[]> logFormat;
			switch (logType)
			{
				case LogType.Log:
					logFormat = Debug.LogFormat;
					break;
				case LogType.Warning:
					logFormat = Debug.LogWarningFormat;
					break;
				default:
					logFormat = Debug.LogErrorFormat;
					break;
			}
			logFormat("String-O-Matic {0}: {1}", new string[] { logType.ToString(), message });
		}

		//=====================================
		//Other
		//=====================================
		/// <summary>
		/// Opens the Unity Preferences window at the specified section name.
		/// </summary>
		public static void OpenPreferences(string section = "General")
		{

#if UNITY_2018_3_OR_NEWER
			SettingsService.OpenUserPreferences("Preferences/" + section);
#else

			Assembly asm = Assembly.GetAssembly(typeof(EditorWindow));
			Type type=asm.GetType("UnityEditor.PreferencesWindow");
			EditorWindow prefs = EditorWindow.GetWindowWithRect(type,new Rect(100f, 100f, 500f, 400f), true, "Unity Preferences");
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			EditorApplication.CallbackFunction c;
			c= delegate{
				MethodInfo method = type.GetProperty("selectedSectionIndex",flags).GetSetMethod(true);
				object list = type.GetField("m_Sections",flags).GetValue(prefs);
				Array array = (Array) list.GetType().GetMethod("ToArray").Invoke(list, null);
				int iSection;
				bool isNumber = int.TryParse(section, out iSection);
				for (int i = 0; i < array.Length; i++){
					GUIContent content = (GUIContent)array.GetValue(i).GetType().GetField("content").GetValue(array.GetValue(i));
					if (isNumber?iSection == i:content.text.Equals(section, StringComparison.InvariantCultureIgnoreCase)){
						method.Invoke(prefs,new object[]{i});
						break;
					}
				}
				prefs.Repaint();
			};
			EditorApplication.delayCall+=c;
#endif
		}

		public static string GetValidTargetDir(string targetdir)
		{
			// string targetdir = SOMPreferences.GetTargetDir();

			if (!String.IsNullOrEmpty(targetdir))
			{
				if (Directory.Exists(targetdir))
				{
					return CleanPath(targetdir);
				}
				else
				{
					try
					{
						Directory.CreateDirectory(targetdir);
						return CleanPath(targetdir);
					}
					catch (Exception ex)
					{
						throw new FileNotFoundException($"Failed to create target directory {targetdir}: {ex.Message}");
					}
				}
			}
			else
			{
				if (Directory.Exists(SOMManager.DEFAULT_TARGETDIR))
				{
					return SOMManager.DEFAULT_TARGETDIR;
				}
				else
				{
					throw new FileNotFoundException($"Target directory is not specified and default target directory {SOMManager.DEFAULT_TARGETDIR} is missing");
				}
			}
		}


		/// <summary>
		/// Returns the path with a forwardslash
		/// </summary>
		public static string CleanPath(string path)
		{
			// Replace all forward slashes with backslashes
			path = path.Replace('/', '\\');

			if (!path.StartsWith("Assets\\"))
			{
				// If it doesn't, add "Assets\" to the beginning of the path
				path = "Assets\\" + path;
			}
			// Ensure the path ends with a backslash
			if (!path.EndsWith("\\"))
			{
				return path + "\\";
			}


			return path;
		}

		/// <summary>
		/// Returns a default string if the str is empty
		/// </summary>
		public static string GetDefaultStringIfEmpty(string _str, string defaultstr = "")
		{
			if (string.IsNullOrEmpty(_str)) return defaultstr;
			else return _str;
		}

		/// <summary>
		/// Retrieve the root namespace from editor preferences
		/// </summary>
		public static string GetRootNamespace()
		{

			string rootNamespace = EditorSettings.projectGenerationRootNamespace;

			return rootNamespace;
		}
		/// <summary>
		/// Layouted Key Event version of the equivalent InfField, FLoatField, TextField Methods etc.
		/// </summary>
		public static Event KeyEventField(Event e, params GUILayoutOption[] options)
		{
			MethodInfo method = typeof(EditorGUILayout).GetMethod("KeyEventField", BindingFlags.Static | BindingFlags.NonPublic);
			return (Event)method.Invoke(null, new object[] { e, options });
		}

		//=====================================
		//Filter list
		//=====================================
		public class FilterList
		{
			const float minPopupWidth = 70;

			public enum FilterType { None, Black, White };
			public FilterType type;
			List<string> list;
			public ReorderableList reorderableList { get; private set; }
			readonly string title;
			readonly string prefsKey;
			readonly bool fixedFilter;

			string prefsFilterType { get { return prefsKey + "FilterType"; } }
			string prefsItem { get { return prefsKey + "Item"; } }
			public string[] values { get { return list.ToArray(); } }
			public bool hasFilter { get { return type != FilterType.None; } }
			public bool isWhite { get { return type == FilterType.White; } }
			public bool isBlack { get { return type == FilterType.Black; } }

			public FilterList(string prefsKey, string title = "", FilterType type = FilterType.None, string[] list = null, bool fixedFilter = false)
			{
				this.prefsKey = prefsKey + "FilterList";
				this.title = title;
				this.fixedFilter = fixedFilter;

				//Try and load data from SOMPreferences
				if (!SOMPreferences.ints.Contains(prefsFilterType))
				{
					SOMPreferences.ints[prefsFilterType] = (int)type;
					this.type = type;
					if (list == null)
						list = new string[0];
					this.list = new List<string>(list);
				}
				//If no such data, create new one
				else
				{
					this.type = (FilterType)SOMPreferences.ints[prefsFilterType];
					this.list = new List<string>();
					int counter = 0;
					while (SOMPreferences.strings.Contains(prefsItem + counter))
						this.list.Add(SOMPreferences.strings[prefsItem + counter++]);
				}
				CreateReorderableList();
			}

			void CreateReorderableList()
			{
				reorderableList = new ReorderableList(list, typeof(string), false, true, true, true);

				//The header displays a label with the Title and a auto-size-adjustable FilterType popup menu
				reorderableList.drawHeaderCallback = (Rect rect) =>
				{
					EditorGUI.LabelField(rect, title);
					float shift = EditorStyles.label.CalcSize(new GUIContent(title)).x;
					rect.x += EditorStyles.label.CalcSize(new GUIContent(title)).x;
					rect.width = Mathf.Max(rect.width - shift, minPopupWidth);
					GUI.enabled = !fixedFilter;
					EditorGUI.BeginChangeCheck();
					type = (FilterType)EditorGUI.Popup(rect, (int)type, new GUIContent[]{
						new GUIContent("No filter", "No filter whatsoever (useful for debugging)"),
						new GUIContent("Black filter", "Values on this filter will NOT be added"),
						new GUIContent("White filter", "ONLY values on this filter will be added")});
					if (EditorGUI.EndChangeCheck())
						SOMPreferences.ints[prefsFilterType] = (int)type;
					GUI.enabled = true;
				};

				//Each element draws those nice horizontal lines at the start (to make it easy-selectable) and a text field
				reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
				{
					rect.x -= 5;
					ReorderableList.defaultBehaviours.DrawElementDraggingHandle(rect, index, isActive, isFocused, true);
					EditorGUI.BeginChangeCheck();
					list[index] = EditorGUI.TextField(new Rect(rect.x + 20, rect.y + 2, rect.width - 15, EditorGUIUtility.singleLineHeight), list[index]);
					if (EditorGUI.EndChangeCheck())
						SOMPreferences.strings[prefsItem + index] = list[index];
				};
				reorderableList.onAddCallback = (ReorderableList listInternal) =>
				{
					list.Add(string.Empty);
					listInternal.list = list;
					SOMPreferences.strings[prefsItem + (list.Count - 1)] = string.Empty;
				};
				//When deleting an element, we shift down every other value and delete the last one
				reorderableList.onRemoveCallback = (ReorderableList listInternal) =>
				{
					list.RemoveAt(listInternal.index);
					for (int i = 0; i < list.Count; i++)
						SOMPreferences.strings[prefsItem + i] = list[i];
					SOMPreferences.strings.Delete(prefsItem + list.Count);
					listInternal.list = list;
				};
			}

			public void DrawLayout()
			{
				reorderableList.DoLayoutList();
			}
			public void Draw(Rect rect)
			{
				reorderableList.DoList(rect);
			}
		}
	}

}
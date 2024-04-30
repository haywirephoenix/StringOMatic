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
			logFormat("[String-O-Matic] {1}", new string[] { logType.ToString(), message });
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
		public static string GetDefaultStringIfEmpty(string _str, string defaultstr)
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

		public static bool CheckForDefineSymbol(string defineSymbol, string assemblyString, string typeString)
		{
			bool exists = false;
			string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

			try
			{
				Assembly assembly = Assembly.Load(assemblyString);

				if (assembly != null)
				{
					Type type = assembly.GetType(typeString);

					if (type != null)
					{
						exists = true;

						if (!defines.Contains(defineSymbol))
							defines += ";" + defineSymbol;
					}
				}
			}
			catch (System.Exception ex)
			{

				// Debug.LogError("Exception while checking for define symbol: " + defineSymbol + "\n" + ex.Message);
			}

			if (!exists)
			{
				if (defines.Contains(defineSymbol))
					defines = defines.Replace(defineSymbol + ";", "");
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);

			return exists;
		}

	}

}
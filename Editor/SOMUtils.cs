using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace SOM{
	/// <summary>
	/// Provides some extra functionality
	/// </summary>
	public static class SOMUtils{

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

		/// <summary>
		/// Nicifies the name of the module to make it PascalCase. 
		/// Also, any numbers at the start of the name are moved back to the end.
		/// </summary>
		public static string NicifyModuleName(string name){
			string newName = name.Trim();
			if (string.IsNullOrEmpty(newName))
				return newName;
			//Capitalize words and remove spaces
			int index = 0;
			do{
				newName = newName.Insert(index, Char.ToUpper(newName[index]).ToString()).Remove(index+1,1);
				index = newName.IndexOf(' ',index)+1;
			}while (index != 0);
			newName = newName.Replace(" ", string.Empty);

			//Remove non alpha-numeric characters
			for (int i = newName.Length-1; i>=0; i--)
				if (!char.IsLetterOrDigit(newName, i))
					newName = newName.Remove(i, 1);
			//Move numbers to the end
			index = 0;
			bool isNumber = false;
			do{
				bool nextCharIsDigit = char.IsDigit(newName[index]);
				if (!nextCharIsDigit)
					break;
				index++;
				//This means the string is a whole number without alphabetic characters
				if (index>=newName.Length){
					isNumber = true;
					newName = "n"+newName;
					break;
				}
			}while (true);
			newName+=newName.Substring(isNumber?1:0, index);
			newName = newName.Remove(isNumber?1:0,index);
				
			return newName;
		}
		/// <summary>
		/// Nicifies the name of the constant to make it camelCase.
		/// If the name coincides with a keyword, it is converted make into PascalCase.
		/// </summary>
		public static string NicifyConstantName(string name){
			name = NicifyModuleName(name);
			if (string.IsNullOrEmpty(name))
				return name;
			
			//Turn the first letter into lowercase
			string finalName = name.Insert(0, Char.ToLower(name[0]).ToString()).Remove(1, 1);

			//If isKeyword, revert back to uppercase
			if (IsKeyword(finalName))
				finalName = name;
			return finalName;
		}
		/// <summary>
		/// Checks whether or not the specified string represents a C# keyword name.
		/// </summary>
		public static bool IsKeyword(string name){
			bool isKeyword = false;
			for (int i = 0; i < keyWords.Length; i++){
				if (string.Equals(name, keyWords[i])){
					isKeyword = true;
					break;
				}
			}
			return isKeyword;
		}

		//=====================================
		//Logs
		//=====================================
		public static void Log(string text){
			Log(LogType.Log, text);
		}
		public static void Log(string format, params object[] args){
			Log(string.Format(format, args));
		}
		public static void LogWarning(string text){
			Log(LogType.Warning, text);
		}
		public static void LogWarning(string format, params object[] args){
			LogWarning(string.Format(format, args));
		}
		public static void LogError(string text){
			Log(LogType.Error, text);
		}
		public static void LogError(string format, params object[] args){
			LogError(string.Format(format, args));
		}
		public static void Log(LogType logType, string format, params object[] args){
			Log(logType,string.Format(format, args));
		}
		/// <summary>
		/// Log the specified logType and message.
		/// </summary>
		public static void Log(LogType logType, string message){
			Action<string, object[]> logFormat;
			switch(logType){
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
			logFormat("String-O-Matic {0}: {1}", new string[]{logType.ToString(), message});
		}

		//=====================================
		//Other
		//=====================================
		/// <summary>
		/// Opens the Unity Preferences window at the specified section name.
		/// </summary>
		public static void OpenPreferences(string section = "General"){
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
		}
		/// <summary>
		/// Layouted Key Event version of the equivalent InfField, FLoatField, TextField Methods etc.
		/// </summary>
		public static Event KeyEventField(Event e, params GUILayoutOption[] options){
			MethodInfo method = typeof(EditorGUILayout).GetMethod("KeyEventField", BindingFlags.Static | BindingFlags.NonPublic);
			return (Event)method.Invoke(null, new object[]{e, options});
		}

		//=====================================
		//Filter list
		//=====================================
		public class FilterList{
			const float minPopupWidth = 70;

			public enum FilterType{None, Black, White};
			public FilterType type;
			List<string> list;
			public ReorderableList reorderableList{get; private set;}
			readonly string title;
			readonly string prefsKey;
			readonly bool fixedFilter;

			string prefsFilterType{get{return prefsKey+"FilterType";}}
			string prefsItem{get{return prefsKey+"Item";}}
			public string[] values{get{return list.ToArray();}}
			public bool hasFilter{get{return type != FilterType.None;}}
			public bool isWhite{get{return type == FilterType.White;}}
			public bool isBlack{get{return type == FilterType.Black;}}

			public FilterList(string prefsKey, string title = "", FilterType type = FilterType.None, string[] list = null, bool fixedFilter = false){
				this.prefsKey = prefsKey+"FilterList";
				this.title = title;
				this.fixedFilter = fixedFilter;

				//Try and load data from SOMPreferences
				if (!SOMPreferences.ints.Contains(prefsFilterType)){
					SOMPreferences.ints[prefsFilterType] = (int)type;
					this.type = type;
					if (list == null)
						list = new string[0];
					this.list = new List<string>(list);
				}
				//If no such data, create new one
				else{
					this.type = (FilterType)SOMPreferences.ints[prefsFilterType];
					this.list = new List<string>();
					int counter = 0;
					while (SOMPreferences.strings.Contains(prefsItem+counter))
						this.list.Add(SOMPreferences.strings[prefsItem+counter++]);
				}
				CreateReorderableList();
			}

			void CreateReorderableList(){
				reorderableList = new ReorderableList(list,typeof(string),false,true,true,true);

				//The header displays a label with the Title and a auto-size-adjustable FilterType popup menu
				reorderableList.drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField(rect,title);
					float shift = EditorStyles.label.CalcSize(new GUIContent(title)).x;
					rect.x+=EditorStyles.label.CalcSize(new GUIContent(title)).x;
					rect.width= Mathf.Max(rect.width-shift, minPopupWidth);
					GUI.enabled = !fixedFilter;
					EditorGUI.BeginChangeCheck();
					type = (FilterType) EditorGUI.Popup(rect, (int)type, new GUIContent[]{
						new GUIContent("No filter", "No filter whatsoever (useful for debugging)"),
						new GUIContent("Black filter", "Values on this filter will NOT be added"),
						new GUIContent("White filter", "ONLY values on this filter will be added")});
					if (EditorGUI.EndChangeCheck())
						SOMPreferences.ints[prefsFilterType] = (int)type;
					GUI.enabled = true;
				};

				//Each element draws those nice horizontal lines at the start (to make it easy-selectable) and a text field
				reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
					rect.x-=5;
					ReorderableList.defaultBehaviours.DrawElementDraggingHandle(rect, index,isActive, isFocused,true);
					EditorGUI.BeginChangeCheck();
					list[index] = EditorGUI.TextField(new Rect(rect.x+20, rect.y+2, rect.width-15, EditorGUIUtility.singleLineHeight),list[index]);
					if (EditorGUI.EndChangeCheck())
						SOMPreferences.strings[prefsItem+index] = list[index];
				};
				reorderableList.onAddCallback = (ReorderableList listInternal) => {
					list.Add(string.Empty);
					listInternal.list = list;
					SOMPreferences.strings[prefsItem+(list.Count-1)] = string.Empty;
				}; 
				//When deleting an element, we shift down every other value and delete the last one
				reorderableList.onRemoveCallback = (ReorderableList listInternal) => {
					list.RemoveAt(listInternal.index);
					for (int i = 0; i < list.Count; i++)
						SOMPreferences.strings[prefsItem+i] = list[i];
					SOMPreferences.strings.Delete(prefsItem+list.Count);
					listInternal.list = list;
				}; 
			}
			 
			public void DrawLayout(){
				reorderableList.DoLayoutList();
			}
			public void Draw(Rect rect){
				reorderableList.DoList(rect);
			}
		}
	}

	//=====================================
	//Exceptions
	//=====================================
	public class SOMException:Exception{
		public SOMException ():base(){}
		public SOMException (string message):base(message){}
		public SOMException (string message, Exception innerException):base(message, innerException){}
	}
	public class XmlDocumentDoesNotExistException:SOMException{
		public XmlDocumentDoesNotExistException():base("The document does not exist"){}
	}
	public class ModuleDoesNotExistException:SOMException{
		public ModuleDoesNotExistException(string path):base(string.Format("The module at path \'{0}\' does not exist", path)){}
	}
	public class ModuleAlreadyExistsException:SOMException{
		public ModuleAlreadyExistsException(string path):base(string.Format("The module at path \'{0}\' already exists", path)){}
	}
	public class ConstantDoesNotExistException:SOMException{
		public ConstantDoesNotExistException(string path, string constant):base(string.Format("The module at path \'{0}\' does not have the constant \'{1}\'", path, constant)){}
	}
	public class ConstantAlreadyExistsException:SOMException{
		public ConstantAlreadyExistsException(string path, string constant):base(string.Format("The module at path \'{0}\' already has the constant \'{1}\'", path, constant)){}
	}
	public class InvalidModuleNameException:SOMException{
		public InvalidModuleNameException(string name):base(string.Format("\'{0}\' is not a valid module name", name)){}
	}
	public class InvalidConstantNameException:SOMException{
		public InvalidConstantNameException(string constant):base(string.Format("\'{0}\' is not a valid constant name", constant)){}
	}
}
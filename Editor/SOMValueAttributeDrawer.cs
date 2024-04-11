using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomPropertyDrawer(typeof(SOMValueAttribute))]
public class SOMValueAttributeDrawer : PropertyDrawer {

	//===========================
	//Vars
	//===========================
	List<string> values;
	bool validType;
	bool missing;

	//===========================
	//Methods
	//===========================
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
		return base.GetPropertyHeight(property, label)+(validType?EditorGUIUtility.singleLineHeight:0)+(missing?EditorGUIUtility.singleLineHeight*2:0);
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
		//Check if the attribute is properly applied
		position.height = EditorGUIUtility.singleLineHeight;
		SOMValueAttribute value = (attribute as SOMValueAttribute);
		Type module  = value.module;
		validType = IsValidType(position, property, label, module);
		if (!validType)
			return;

		//Load list
		if (values == null){
			values = new List<string>();
			values.AddRange(MakeEntry(module, module.Name+"/"));
		}

		//Get the path from the value and deduce wether the reference is missing or not
		string path = ValueToPath(module,property.stringValue);
		int index = 0;
		missing = true;
		if (!string.IsNullOrEmpty(path)){
			path = path.Substring(path.IndexOf(module.Name));
			index = values.IndexOf(path);
			if (index != -1)
				missing = false;
		}

		//If missing reference, show warning message
		if (missing){
			Rect rect = new Rect(position);
			rect.x+=EditorGUIUtility.labelWidth;
			rect.width-=EditorGUIUtility.labelWidth;
			rect.y+=EditorGUIUtility.singleLineHeight;
			rect.height = EditorGUIUtility.singleLineHeight*2;
			EditorGUI.HelpBox(rect,"Missing String-O-Matic Reference",MessageType.Error);
			//In order to show the "Missing" text in the popup, add it to the first element in the list
			if (values.Count == 0 || values[0] != "Missing")
				values.Insert(0, "Missing");
			index = EditorGUI.Popup(position,label.text, 0, values.ToArray());
			//If selected anything other than "Missing", change the property value
			if (GUI.changed){
				if (index != 0){
					values.RemoveAt(0);
					property.stringValue = PathToValue(ModuleToPath(module)+values[index-1]);
				}
			}
				
		}
		//If all good, store the value of the constant
		else{
			property.stringValue = values[EditorGUI.Popup(position,label.text,index, values.ToArray())];
			property.stringValue = PathToValue(ModuleToPath(module)+property.stringValue);
		}
		
		//Show the value
		position.y+=missing?EditorGUIUtility.singleLineHeight*3:EditorGUIUtility.singleLineHeight;
		EditorGUI.LabelField(position," ",property.stringValue);
	}

	//This checks wether the attribute has been applied correctly or not.
	bool IsValidType(Rect position, SerializedProperty property, GUIContent label, Type module){
		//Check if the attribute has been applied to a string
		if (property.propertyType != SerializedPropertyType.String){
			EditorGUI.LabelField(position, label.text, "SOMValue con only be applied to string");
			return false;
		}
		//Check if SOM exists
		Type somType = Type.GetType("StringOMatic, "+typeof(SOMValueAttribute).Assembly.FullName);
		if (somType == null){
			EditorGUI.LabelField(position, label.text, "String-O-Matic does not exist");
			return false;
		}
		//Check if module exists
		if (module == null){
			EditorGUI.LabelField(position, label.text, "The given SOM Module does not exist");
			return false;
		}
		//Check if valid SOM Module
		Type declaringType = module;
		while (declaringType != somType){
			declaringType = declaringType.DeclaringType;
			if (declaringType == null){
				EditorGUI.LabelField(position, label.text, string.Format("{0} is not a valid SOM Module", module.Name));
				return false;
			}
		}
		return true;
	}

	//This makes an entry in the popup list for the given type, listing all its constants and submodules
	List<string> MakeEntry(Type type, string parent){
		List<string> entries = new List<string>();
		FieldInfo[] fields = type.GetFields();

		//Add values
		foreach(FieldInfo field in fields)
			entries.Add(parent+field.Name);

		//Add subtypes
		Type[] subTypes = type.GetNestedTypes();
		foreach(Type sType in subTypes)
			entries.AddRange(MakeEntry(sType, parent+sType.Name+"/"));
		return entries;
	}

	//Given a valid SOM Module, returns the path to that module 
	string ModuleToPath(Type module){
		Type declaringType = module.DeclaringType;
		string path = string.Empty;
		while(declaringType != null){
			path = declaringType.Name+"/"+path;
			declaringType = declaringType.DeclaringType;
		}
		return path;
	}

	//Given the path to a constant, get the value from that constant
	string PathToValue(string path){
		//Remove the "StringOMatic" part at the beggninning
		path = path.Substring(path.IndexOf('/')+1);
		string[] parts = path.Split('/');
		Type type = Type.GetType("StringOMatic, "+typeof(SOMValueAttribute).Assembly.FullName);
		for (int i = 0; i < parts.Length-1; i++)
			type = type.GetNestedType(parts[i]);
		return (string)type.GetField(parts[parts.Length-1]).GetValue(null);
	}

	//Given some value, search the type hierarchy for a constant containing that value
	string ValueToPath(Type type, string value){
		if (value == null)
			return null;
		//First, search the value among its fields
		FieldInfo[] fields = type.GetFields();
		foreach(FieldInfo field in fields){
			if (value.Equals(field.GetValue(null)))
				return FieldToPath(field);
		}
		//If no sucess, search among its nested types
		Type[] types = type.GetNestedTypes();
		foreach(Type t in types){
			string path = ValueToPath(t,value);
			if (path != null)
				return path;
		}
		return null;
	}

	//Given a FieldInfo, get the path of its declaring types
	string FieldToPath(FieldInfo field){
		string path = field.Name;
		Type declaringType = field.DeclaringType;
		while (declaringType != null){
			path = declaringType.Name+"/"+path;
			declaringType = declaringType.DeclaringType;
		}
		return path;
	}
}
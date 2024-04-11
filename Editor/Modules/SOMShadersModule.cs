using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

namespace SOM{
	/// <summary>
	/// The Shaders Module searches the project for every shader file (plus the built-in ones).
	/// It then lists their names and properties. You can also specify white/black filters.
	/// </summary>
	public class SOMShadersModule : SOMModule {
		//========================================
		//Consts
		//========================================
		const string ADD_BUILTIN_KEY = "Shaders Module Add Builtin Key";
		const string ADD_CUSTOM_KEY = "Shaders Module Add Custom Key";
		const string ADD_PROPERTIES_KEY = "Shaders Module Add Properties Key";
		const string ADD_PROPERTIES_MODULE_KEY = "Shaders Module Add Properties Module Key";
		const string SEPARATE_SHADERS_KEY = "Shaders Module Separate Shaders key";

		const string ADD_BUILTIN_LABEL = "Add Built-in Shaders";
		const string ADD_CUSTOM_LABEL = "Add Custom Shaders";
		const string ADD_PROPERTIES_LABEL = "Add Properties";
		const string ADD_PROPERTIES_MODULE_LABEL = "Add Properties in a sub module";
		const string SEPARATE_SHADERS_LABEL = "Separate Built-in from Custom";

		const string ADD_BUILTIN_TOOLTIP = "Should built-in shaders be included or not?";
		const string ADD_CUSTOM_TOOLTIP = "Should custom shaders be included or not?";
		const string ADD_PROPERTIES_TOOLTIP = "Whether or not to add a sub-module with all of the shader's properties";
		const string ADD_PROPERTIES_MODULE_TOOLTIP = "If checked, a sub module named \"Parameters\" will be added";
		const string SEPARATE_SHADERS_TOOLTIP = "If checked, both Built-in and Custom shaders will have their own module";

		//========================================
		//Vars
		//========================================
		SOMUtils.FilterList _list;
		//This is a list of built-in shader names for every supported version of Unity
		string[] builtIn = new string[]{
			#if UNITY_5_0 || UNITY_5_1
			#region 5.0 & 5.1
			"GUI/Text Shader",
			"Sprites/Default",
			"Legacy Shaders/Transparent/Bumped Diffuse",
			"Legacy Shaders/Transparent/Bumped Specular",
			"Legacy Shaders/Transparent/Diffuse",
			"Legacy Shaders/Transparent/Specular",
			"Legacy Shaders/Transparent/Parallax Diffuse",
			"Legacy Shaders/Transparent/Parallax Specular",
			"Legacy Shaders/Transparent/VertexLit",
			"Legacy Shaders/Transparent/Cutout/Bumped Diffuse",
			"Legacy Shaders/Transparent/Cutout/Bumped Specular",
			"Legacy Shaders/Transparent/Cutout/Diffuse",
			"Legacy Shaders/Transparent/Cutout/Specular",
			"Legacy Shaders/Transparent/Cutout/Soft Edge Unlit",
			"Legacy Shaders/Transparent/Cutout/VertexLit",
			"Legacy Shaders/Decal",
			"FX/Flare",
			"Legacy Shaders/Self-Illumin/Bumped Diffuse",
			"Legacy Shaders/Self-Illumin/Bumped Specular",
			"Legacy Shaders/Self-Illumin/Diffuse",
			"Legacy Shaders/Self-Illumin/Specular",
			"Legacy Shaders/Self-Illumin/Parallax Diffuse",
			"Legacy Shaders/Self-Illumin/Parallax Specular",
			"Legacy Shaders/Self-Illumin/VertexLit",
			"Legacy Shaders/Lightmapped/Bumped Diffuse",
			"Legacy Shaders/Lightmapped/Bumped Specular",
			"Legacy Shaders/Lightmapped/Diffuse",
			"Legacy Shaders/Lightmapped/Specular",
			"Legacy Shaders/Lightmapped/VertexLit",
			"Legacy Shaders/Bumped Diffuse",
			"Legacy Shaders/Bumped Specular",
			"Legacy Shaders/Diffuse",
			"Legacy Shaders/Diffuse Detail",
			"Legacy Shaders/Diffuse Fast",
			"Legacy Shaders/Specular",
			"Legacy Shaders/Parallax Diffuse",
			"Legacy Shaders/Parallax Specular",
			"Legacy Shaders/VertexLit",
			"Particles/Additive",
			"Particles/~Additive-Multiply",
			"Particles/Additive (Soft)",
			"Particles/Alpha Blended",
			"Particles/Multiply",
			"Particles/Multiply (Double)",
			"Particles/Alpha Blended Premultiply",
			"Particles/VertexLit Blended",
			"Legacy Shaders/Reflective/Bumped Diffuse",
			"Legacy Shaders/Reflective/Bumped Unlit",
			"Legacy Shaders/Reflective/Bumped Specular",
			"Legacy Shaders/Reflective/Bumped VertexLit",
			"Legacy Shaders/Reflective/Diffuse",
			"Legacy Shaders/Reflective/Specular",
			"Legacy Shaders/Reflective/Parallax Diffuse",
			"Legacy Shaders/Reflective/Parallax Specular",
			"Legacy Shaders/Reflective/VertexLit",
			"Skybox/Cubemap",
			"Skybox/Procedural",
			"Skybox/6 Sided",
			"Sprites/Diffuse",
			"Standard",
			"Standard (Specular setup)",
			"Mobile/Bumped Diffuse",
			"Mobile/Bumped Specular (1 Directional Light)",
			"Mobile/Bumped Specular",
			"Mobile/Diffuse",
			"Mobile/Unlit (Supports Lightmap)",
			"Mobile/Particles/Additive",
			"Mobile/Particles/VertexLit Blended",
			"Mobile/Particles/Alpha Blended",
			"Mobile/Particles/Multiply",
			"Mobile/Skybox",
			"Mobile/VertexLit (Only Directional Lights)",
			"Mobile/VertexLit",
			"Nature/SpeedTree",
			"Nature/SpeedTree Billboard",
			"Nature/Tree Soft Occlusion Bark",
			"Nature/Tree Soft Occlusion Leaves",
			"Nature/Tree Creator Bark",
			"Nature/Tree Creator Leaves",
			"Nature/Tree Creator Leaves Fast",
			"Nature/Terrain/Diffuse",
			"Nature/Terrain/Specular",
			"Nature/Terrain/Standard",
			"UI/Default",
			"UI/Default Font",
			"UI/Lit/Bumped",
			"UI/Lit/Detail",
			"UI/Lit/Refraction (Pro Only)",
			"UI/Lit/Refraction Detail (Pro Only)",
			"UI/Lit/Transparent",
			"UI/Unlit/Detail",
			"UI/Unlit/Text",
			"UI/Unlit/Text Detail",
			"UI/Unlit/Transparent",
			"Unlit/Transparent",
			"Unlit/Transparent Cutout",
			"Unlit/Color",
			"Unlit/Texture",
			#endregion
			#elif UNITY_5_2 || UNITY_5_3
			#region 5.2 & 5.3
			"GUI/Text Shader",
			"Sprites/Default",
			"Legacy Shaders/Transparent/Bumped Diffuse",
			"Legacy Shaders/Transparent/Bumped Specular",
			"Legacy Shaders/Transparent/Diffuse",
			"Legacy Shaders/Transparent/Specular",
			"Legacy Shaders/Transparent/Parallax Diffuse",
			"Legacy Shaders/Transparent/Parallax Specular",
			"Legacy Shaders/Transparent/VertexLit",
			"Legacy Shaders/Transparent/Cutout/Bumped Diffuse",
			"Legacy Shaders/Transparent/Cutout/Bumped Specular",
			"Legacy Shaders/Transparent/Cutout/Diffuse",
			"Legacy Shaders/Transparent/Cutout/Specular",
			"Legacy Shaders/Transparent/Cutout/Soft Edge Unlit",
			"Legacy Shaders/Transparent/Cutout/VertexLit",
			"Legacy Shaders/Decal",
			"FX/Flare",
			"Legacy Shaders/Self-Illumin/Bumped Diffuse",
			"Legacy Shaders/Self-Illumin/Bumped Specular",
			"Legacy Shaders/Self-Illumin/Diffuse",
			"Legacy Shaders/Self-Illumin/Specular",
			"Legacy Shaders/Self-Illumin/Parallax Diffuse",
			"Legacy Shaders/Self-Illumin/Parallax Specular",
			"Legacy Shaders/Self-Illumin/VertexLit",
			"Legacy Shaders/Lightmapped/Bumped Diffuse",
			"Legacy Shaders/Lightmapped/Bumped Specular",
			"Legacy Shaders/Lightmapped/Diffuse",
			"Legacy Shaders/Lightmapped/Specular",
			"Legacy Shaders/Lightmapped/VertexLit",
			"Legacy Shaders/Bumped Diffuse",
			"Legacy Shaders/Bumped Specular",
			"Legacy Shaders/Diffuse",
			"Legacy Shaders/Diffuse Detail",
			"Legacy Shaders/Diffuse Fast",
			"Legacy Shaders/Specular",
			"Legacy Shaders/Parallax Diffuse",
			"Legacy Shaders/Parallax Specular",
			"Legacy Shaders/VertexLit",
			"Particles/Additive",
			"Particles/~Additive-Multiply",
			"Particles/Additive (Soft)",
			"Particles/Alpha Blended",
			"Particles/Multiply",
			"Particles/Multiply (Double)",
			"Particles/Alpha Blended Premultiply",
			"Particles/VertexLit Blended",
			"Legacy Shaders/Reflective/Bumped Diffuse",
			"Legacy Shaders/Reflective/Bumped Unlit",
			"Legacy Shaders/Reflective/Bumped Specular",
			"Legacy Shaders/Reflective/Bumped VertexLit",
			"Legacy Shaders/Reflective/Diffuse",
			"Legacy Shaders/Reflective/Specular",
			"Legacy Shaders/Reflective/Parallax Diffuse",
			"Legacy Shaders/Reflective/Parallax Specular",
			"Legacy Shaders/Reflective/VertexLit",
			"Skybox/Cubemap",
			"Skybox/Procedural",
			"Skybox/6 Sided",
			"Sprites/Diffuse",
			"Standard",
			"Standard (Specular setup)",
			"Mobile/Bumped Diffuse",
			"Mobile/Bumped Specular (1 Directional Light)",
			"Mobile/Bumped Specular",
			"Mobile/Diffuse",
			"Mobile/Unlit (Supports Lightmap)",
			"Mobile/Particles/Additive",
			"Mobile/Particles/VertexLit Blended",
			"Mobile/Particles/Alpha Blended",
			"Mobile/Particles/Multiply",
			"Mobile/Skybox",
			"Mobile/VertexLit (Only Directional Lights)",
			"Mobile/VertexLit",
			"Nature/SpeedTree",
			"Nature/SpeedTree Billboard",
			"Nature/Tree Soft Occlusion Bark",
			"Nature/Tree Soft Occlusion Leaves",
			"Nature/Tree Creator Bark",
			"Nature/Tree Creator Leaves",
			"Nature/Tree Creator Leaves Fast",
			"Nature/Terrain/Diffuse",
			"Nature/Terrain/Specular",
			"Nature/Terrain/Standard",
			"UI/Default",
			"UI/Default Font",
			"UI/Lit/Bumped",
			"UI/Lit/Detail",
			"UI/Lit/Refraction",
			"UI/Lit/Refraction Detail",
			"UI/Lit/Transparent",
			"UI/Unlit/Detail",
			"UI/Unlit/Text",
			"UI/Unlit/Text Detail",
			"UI/Unlit/Transparent",
			"Unlit/Transparent",
			"Unlit/Transparent Cutout",
			"Unlit/Color",
			"Unlit/Texture",
			#endregion
			#endif 
		};

		//==============================
		//Properties
		//==============================
		static bool addBuiltin{
			get{
				if (!SOMPreferences.bools.Contains(ADD_BUILTIN_KEY))
					addBuiltin = true;
				return SOMPreferences.bools[ADD_BUILTIN_KEY];
			}
			set{
				SOMPreferences.bools[ADD_BUILTIN_KEY] = value;
			}
		}
		static bool addCustom{
			get{
				if (!SOMPreferences.bools.Contains(ADD_CUSTOM_KEY))
					addCustom = true;
				return SOMPreferences.bools[ADD_CUSTOM_KEY];
			}
			set{
				SOMPreferences.bools[ADD_CUSTOM_KEY] = value;
			}
		}
		static bool addProperties{
			get{
				if (!SOMPreferences.bools.Contains(ADD_PROPERTIES_KEY))
					addProperties = true;
				return SOMPreferences.bools[ADD_PROPERTIES_KEY];
			}
			set{
				SOMPreferences.bools[ADD_PROPERTIES_KEY] = value;
			}
		}
		static bool addPropertiesModule{
			get{
				if (!SOMPreferences.bools.Contains(ADD_PROPERTIES_MODULE_KEY))
					addPropertiesModule = true;
				return SOMPreferences.bools[ADD_PROPERTIES_MODULE_KEY];
			}
			set{
				SOMPreferences.bools[ADD_PROPERTIES_MODULE_KEY] = value;
			}
		}
		static bool separateShaders{
			get{
				if (!SOMPreferences.bools.Contains(SEPARATE_SHADERS_KEY))
					separateShaders = false;
				return SOMPreferences.bools[SEPARATE_SHADERS_KEY];
			}
			set{
				SOMPreferences.bools[SEPARATE_SHADERS_KEY] = value;
			}
		}
		SOMUtils.FilterList list{
			get{
				if (_list == null)
					_list = new SOMUtils.FilterList(moduleName, "Shader names", SOMUtils.FilterList.FilterType.Black);
				return _list;
			}
		}
		public override string moduleName {
			get {
				return "Shaders";
			}
		}

		//========================================
		//Refresh
		//========================================
		public override void Refresh(){

			//Compute built-in shaders
			if (addBuiltin){
				string builtInModule = moduleName;
				if (separateShaders){
					builtInModule+=".BuiltIn";
					SOMXmlHandler.AddModule(builtInModule);
				}
				for (int i = 0; i < builtIn.Length; i++)
					AddShader(builtIn[i], builtInModule, true);
			}

			if (addCustom){
				//For custom shaders, get all .shader files in the while project
				string customModule = moduleName;
				if (separateShaders){
					customModule+=".Custom";
					SOMXmlHandler.AddModule(customModule);
				}
				string[] shaders = Directory.GetFiles(Application.dataPath,"*.shader", SearchOption.AllDirectories);

				//For each one...
				for (int i = 0; i < shaders.Length; i++){
					//Omit those in an editor folder
					if (shaders[i].IndexOf("/editor/", StringComparison.InvariantCultureIgnoreCase) != -1)
						continue;

					//Get the shader name based on the first line starting with "Shader "
					StreamReader file = new StreamReader(shaders[i]);
					string shaderName = string.Empty;
					while (!shaderName.StartsWith("Shader ") && !file.EndOfStream)
						shaderName = file.ReadLine();

					//Filter invalid names and get shader name inbetween ""
					if (!shaderName.Contains("\""))
						continue;
					shaderName = shaderName.Substring(shaderName.IndexOf('\"')+1);
					if (!shaderName.Contains("\""))
						continue;
					shaderName = shaderName.Substring(0, shaderName.IndexOf('\"'));

					//Process the shader
					AddShader(shaderName, customModule, false);
				}
			}
		}
		//Given a shader name and a parent module...
		void AddShader(string name, string parent, bool isBuiltIn){
			//Check if the shader in in the list
			if (list.hasFilter){
				bool isFiltered = false;
				for (int i = 0; i < list.values.Length; i++){
					if (list.values[i].EndsWith("/")){
						if (name.StartsWith(list.values[i],StringComparison.InvariantCultureIgnoreCase)){
							isFiltered = true;
							break;
						}
					}
					else if (name.Equals(list.values[i],StringComparison.InvariantCultureIgnoreCase)){
						isFiltered = true;
						break;
					}
				}
				//And act accordingly
				if (!(isFiltered^list.isBlack))
					return;
			}

			//Check for valid shader
			Shader shader = Shader.Find(name);
			if (shader == null)
				throw new ShaderDoesNotExistException(name, isBuiltIn);
			if (SOMXmlHandler.ModuleExists(parent+"."+name)){
				SOMUtils.LogWarning("Shaders--> The shader name \"{0}\" is duplicated. As a result, the shader at path \"{1}\" has been skipped",name, AssetDatabase.GetAssetPath(shader));
				Resources.UnloadAsset(shader);
				return;
			}
			if (!isBuiltIn){
				for (int i = 0; i < builtIn.Length; i++){
					if (name.Equals(builtIn[i])){
						SOMUtils.LogWarning("Shaders--> The shader at path \"{0}\" has the same name as the built-in shader \"{1}\". As a result, it has been skipped",AssetDatabase.GetAssetPath(shader), name);
						Resources.UnloadAsset(shader);
						return;
					}
				}
			}

			//Split the shader name in differents parts separated by "/"
			string[] subModules = name.Split('/');
			string subModuleName = parent;
			//And check if a module with that part name already exists
			for (int j = 0; j < subModules.Length; j++){
				subModuleName+="."+subModules[j];
				//If it does not, add it
				if (!SOMXmlHandler.ModuleExists(subModuleName))
					SOMXmlHandler.AddModule(subModuleName);
			}
			//And finally add the shader final name to the module
			SOMXmlHandler.AddConstant(subModuleName,"name",name);

			//For that shader, add all of its properties as constants
			if (addProperties){
				string propertiesModule = subModuleName;
				if (addPropertiesModule){
					propertiesModule+=".Properties";
					SOMXmlHandler.AddModule(propertiesModule);
				}
				int count = ShaderUtil.GetPropertyCount(shader);
				for (int j = 0; j <count; j++){
					string propertyName = ShaderUtil.GetPropertyName(shader,j);
					SOMXmlHandler.AddConstant(propertiesModule,SOMUtils.NicifyConstantName(propertyName),propertyName);
				}
			}
			Resources.UnloadAsset(shader);
		}

		//========================================
		//Preferences
		//========================================
		public override void DrawPreferences(){
			separateShaders = EditorGUILayout.ToggleLeft(new GUIContent(SEPARATE_SHADERS_LABEL, SEPARATE_SHADERS_TOOLTIP), separateShaders);
			addBuiltin = EditorGUILayout.ToggleLeft(new GUIContent(ADD_BUILTIN_LABEL, ADD_BUILTIN_TOOLTIP), addBuiltin);
			addCustom = EditorGUILayout.ToggleLeft(new GUIContent(ADD_CUSTOM_LABEL, ADD_CUSTOM_TOOLTIP), addCustom);
			addProperties = EditorGUILayout.ToggleLeft(new GUIContent(ADD_PROPERTIES_LABEL, ADD_PROPERTIES_TOOLTIP), addProperties);
			EditorGUI.indentLevel++;
			GUI.enabled = addProperties;
			addPropertiesModule = EditorGUILayout.ToggleLeft(new GUIContent(ADD_PROPERTIES_MODULE_LABEL, ADD_PROPERTIES_MODULE_TOOLTIP), addPropertiesModule);
			GUI.enabled = true;
			EditorGUI.indentLevel--;
			list.DrawLayout();
			base.DrawPreferences();
		}
	}
	public class ShaderDoesNotExistException:SOMException{
		public ShaderDoesNotExistException(string shaderName, bool builtIn):base(string.Format("The {0}shader \"{1}\" does not exist as for version {2}", builtIn?"built-in ":"",shaderName, Application.unityVersion)){}
	}
}
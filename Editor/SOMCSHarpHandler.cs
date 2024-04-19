using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;


namespace SOM
{
	/// <summary>
	/// This class is responsible for converting the .xml file into a C# file
	/// </summary>
	public static class SOMCSHarpHandler
	{

		#region Consts
		//========================================
		//Consts
		//========================================
		// const string NAME = "StringOMatic";
		const string WITH_ERRORS = "WithErrors";
		const string MECANIM_NAME = "Mecanim";
		const string COMPILATION_SUCCESSFUL = "Compilation Successful";
		const string _public = "public";
		const string _namespace = "namespace";
		const string _static = "static";
		const string _const = "const";
		const string _class = "class";
		const string _struct = "struct";
		const string _string = "string";
		const string _int = "int";
		const string _openBrace = "{";
		const string _closeBrace = "}";
		const string _doublefslash = "//";
		const string _blockQuoteOpen = "/*";
		const string _blockQuoteClose = "*/";
		private const Char _dotChar = '.';
		private const Char _indentChar = '\t';
		private const string _indentStr = "\t";
		private const string _dotStr = ".";
		private const string _dotCS = ".cs";
		private const string _dotTxt = ".txt";
		private enum BraceState { Open, Close }
		static readonly string _newLine = Environment.NewLine;

		#endregion

		#region Mecanim Consts

		const string _Layers = "Layers";
		const string _layerName = "layerName";
		const string _BaseLayer = "BaseLayer";
		const string _States = "States";
		const string _StateMachines = "StateMachines";
		const string _Parameters = "Parameters";
		const string _Controllers = "Controllers";


		#endregion




		#region Properties
		//Properties
		//========================================
		private static string OuterClassName => SOMPreferences.GetClassName();
		private static string CurrentModuleName { get; set; }
		private static int RemainingOpenBraceCount { get; set; }
		private static int IndentationLevel { get; set; }
		private static bool AddAnimatorInts { get; set; }
		private static StringBuilder _StringBuilder { get; set; }
		private static Dictionary<string, object> FoundModules { get; set; }

		#endregion

		//========================================
		//Methods
		//========================================
		/// <summary>
		/// Generates a new C# file from the xml file. If the new file contains any compilation errors, it is created as a .txt file instead.
		/// </summary>
		///


		public static void Compile()
		{
			GenerateCS();
		}
		private static void Init()
		{

			_StringBuilder = new();
			FoundModules = new();

			IndentationLevel = 0;
			RemainingOpenBraceCount = 0;

			AddAnimatorInts = SOMPreferences.GetBoolFromPrefs(SOMMecanimModule.ADD_ANIM_HASH_KEY, true);
		}
		private static void GenerateCS()
		{

			if (!SOMDataHandler.DatabaseExists())
				throw new DatabaseNotExistException();

			if (SOMDataHandler.ModuleCount == 0)
			{
				Debug.Log("No modules found");
				return;
			}

			Init();

			_StringBuilder.WriteHeaderMessage();
			_StringBuilder.WriteRootNamespace();
			_StringBuilder.WriteAllClasses();
			_StringBuilder.WriteEndRootNamespace();

			CompileCS(out bool compileSuccess, out CompilerResults results);

			LogResults(compileSuccess, OuterClassName, results);

			SaveFile(OuterClassName, compileSuccess);

		}
		private static bool HasRootNamespace()
		{
			return HasRootNamespace(out _);
		}
		private static bool HasRootNamespace(out string namespaceName)
		{
			namespaceName = SOMPreferences.GetNamespace();
			string niceNamespace = SOMUtils.NicifyModuleName(namespaceName);

			bool namespaceDefined = !String.IsNullOrEmpty(niceNamespace);

			return namespaceDefined;

		}
		private static bool CompileCS(out bool success, out CompilerResults results)
		{
			success = true;
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = new CompilerParameters();

			results = provider.CompileAssemblyFromSource(parameters, _StringBuilder.ToString());

			for (int i = 0; i < results.Errors.Count; i++)
			{
				if (!results.Errors[i].IsWarning)
				{
					success = false;
					break;
				}
			}

			return success;
		}
		private static void LogResults(bool canBeCompiled, string outerclassName, CompilerResults results)
		{
			if (!canBeCompiled)
			{
				string log = $"The newly created {outerclassName}.cs file contains the compilation errors listed below." +
							"In order to maintain previous code stability and avoid trouble, the file has been renamed to " +
							$"{outerclassName + WITH_ERRORS + _dotTxt}. That'll teach it!{_newLine + _newLine}";


				for (int i = 0; i < results.Errors.Count; i++)
					if (!results.Errors[i].IsWarning)
						log += "Line " + (results.Errors[i].Line + 1) + " --> error " + results.Errors[i].ErrorNumber + ": " + results.Errors[i].ErrorText + _newLine;
				SOMUtils.LogError(log);
			}
			else
			{
				SOMUtils.Log($"{COMPILATION_SUCCESSFUL} - File saved to {GetSavePath(outerclassName) + _dotCS}");
			}
		}

		private static string GetSavePath(string className)
		{
			return SOMUtils.GetValidTargetDir(SOMPreferences.GetTargetDir()) + className;
		}
		private static void SaveFile(string className, bool compileSuccess)
		{
			string path = GetSavePath(className);

			path += compileSuccess ? _dotCS : WITH_ERRORS + _dotTxt;

			File.WriteAllText(path, _StringBuilder.ToString());
			AssetDatabase.ImportAsset(path/*.Substring(output.IndexOf("Assets/"))*/);

		}

		private static void WriteHeaderMessage(this StringBuilder sb)
		{
			if (!SOMPreferences.GetBoolFromPrefs(SOMManager.WRITE_COMMENT_KEY, true)) return;

			sb.AppendLine("/*=========================");
			sb.AppendLine("Generated by String-O-Matic");
			sb.AppendLine("=========================*/");
			sb.AppendLine();
		}
		private static void WriteNamespace(this StringBuilder sb, string namespaceName, int indent = 0)
		{
			sb.WriteIndentations(indent);
			sb.WriteLine($"{_namespace} {namespaceName}");
			sb.WriteIndentations(indent);
			sb.WriteBrace(BraceState.Open, indent);
		}
		private static void WriteRootNamespace(this StringBuilder sb)
		{

			string namespaceName = SOMManager.DEFAULT_NAMESPACE;

			HasRootNamespace(out namespaceName);

			sb.WriteNamespace(namespaceName);

		}
		private static void WriteEndRootNamespace(this StringBuilder sb)
		{
			ResetIndentationLevel();
			sb.WriteEndNamespace();

		}
		private static void WriteEndNamespace(this StringBuilder sb, int indent = 0)
		{

			sb.WriteBrace(BraceState.Close, indent);
		}

		private static string GetClassString(string className)
		{
			return $"{_public} {_static} {_class} {className}";
		}
		private static string GetStructString(string structName)
		{
			return $"{_public} {_struct} {structName}";
		}
		private static string GetConstString(string constName, string constValue)
		{
			return $"{_public} {_const} {_string} {constName} = \"{constValue}\";";
		}
		private static void WriteClass(this StringBuilder sb, string className, int indent = 0)
		{
			sb.WriteIndentations(indent);
			sb.WriteLine(GetClassString(className));
			sb.WriteIndentations(indent);
			sb.WriteBrace(BraceState.Open, indent);

		}
		private static void WriteStruct(this StringBuilder sb, string className, int indent = 0)
		{
			sb.WriteIndentations(indent);
			sb.WriteLine(GetStructString(className));
			sb.WriteIndentations(indent);
			sb.WriteBrace(BraceState.Open, indent);

		}

		private static void WriteEndClass(this StringBuilder sb, int indentLevel = 1)
		{
			sb.WriteIndentations(indentLevel);

			sb.WriteBrace(BraceState.Close, indentLevel);
		}


		public static string Repeat(string str, int count)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				builder.Append(str);
			}
			return builder.ToString();
		}

		public static bool reachedMecanimLayers;
		public static bool isMecanimModule;
		public static string previousMecanimBaseLayer;
		public static string currentMecanimBaseLayer;
		public static List<string> mecanimKeyPath = new();

		public static bool IsValidMecanimLayerName(string layerName)
		{

			switch (layerName)
			{
				case _Layers: return false;
				case _StateMachines: return false;
				case _States: return false;
				case _Parameters: return false;
				case _Controllers: return false;
			}

			return true;

		}

		public static void UpdateMecanimKeyPath(string nameToAdd)
		{
			if (mecanimKeyPath.Count == 1 && nameToAdd == mecanimKeyPath[0])
				return;

			if (currentMecanimBaseLayer != previousMecanimBaseLayer)
			{
				previousMecanimBaseLayer = currentMecanimBaseLayer;
				mecanimKeyPath.Clear();
				mecanimKeyPath.Add(currentMecanimBaseLayer);
			}
			else
			{
				mecanimKeyPath.Add(nameToAdd);
			}

			Debug.Log(string.Join(_dotChar, mecanimKeyPath));
		}

		private static void CheckForMecanimLayer(this StringBuilder sb, string layerName)
		{
			if (reachedMecanimLayers && IsValidMecanimLayerName(layerName))
			{
				currentMecanimBaseLayer = layerName;

				UpdateMecanimKeyPath(layerName);

				// sb.WriteLineComment("mecanim layer: " + layerName);
			}

			if (!reachedMecanimLayers || !isMecanimModule)
			{
				reachedMecanimLayers = isMecanimModule && layerName == _Layers;
			}
		}
		private static void CheckAddMecanimConst(this StringBuilder sb, string constName, string constValue, int indent)
		{
			if (!isMecanimModule) return;

			if (isMecanimModule && AddAnimatorInts)
			{
				// result.WriteLineComment(kvp.Key);
				if (constName == _layerName)
				{
					// sb.WriteLineComment("layername: " + value);
					currentMecanimBaseLayer = constValue;
				}


				sb.WriteAnimatorInt(constName, constValue, indent);
				// sb.InsertLineComment(animhashname, indent);


			}
		}

		public static string GetAllConsts(Dictionary<string, object> dict, int indentLevel = 1,
	HashSet<Dictionary<string, object>> visited = null)
		{
			visited ??= new HashSet<Dictionary<string, object>>();
			StringBuilder result = new();
			string indent = Repeat(_indentStr, indentLevel);

			bool reachedStrings = false;
			bool isRoot = indentLevel == 1;

			isMecanimModule = CurrentModuleName == MECANIM_NAME;

			foreach (var kvp in dict)
			{
				if (kvp.Value is Dictionary<string, object> nestedDict)
				{
					if (!visited.Contains(nestedDict))
					{

						if (isRoot)
						{
							result.WriteNamespace(SOMUtils.NicifyModuleName(kvp.Key + "Module"), indentLevel);
							indentLevel++;

							CurrentModuleName = kvp.Key;
						}

						result.WriteClass(kvp.Key, indentLevel);

						result.CheckForMecanimLayer(kvp.Value as string);


						visited.Add(nestedDict);
						reachedStrings = nestedDict.Values.Any(val => val is string);

						result.Append(GetAllConsts(nestedDict, indentLevel + 1, visited));

						result.WriteEndClass(indentLevel);

						if (isRoot)
						{
							indentLevel--;
							result.WriteIndentations(indentLevel);
							result.WriteEndNamespace(indentLevel);
						}

					}
					else
					{
						// Dictionary already visited
					}
				}
				else // Reached strings
				{
					result.WriteConstant(kvp.Key, kvp.Value as string, indentLevel);

					result.CheckAddMecanimConst(kvp.Key, kvp.Value as string, indentLevel);


				}
			}

			return result.ToString();
		}

		private static void WriteAllClasses(this StringBuilder sb)
		{
			SOMDictionary db = SOMDataHandler.GetRootData();

			sb.Append(GetAllConsts(db));
		}

		private static void WriteConstant(this StringBuilder sb, string constName, string constValue, int indentLevel)
		{
			sb.WriteIndentations(indentLevel);
			sb.WriteLine(GetConstString(constName, constValue));
		}


		public static void WriteAnimatorInt(this StringBuilder sb, string constantName, string constValue, int indentLevel = 1)
		{
			// if (MecanimLayerName.EndsWith(constantValue))
			// {
			// 	MecanimLayerName = MecanimLayerName.Remove(MecanimLayerName.Length - constantValue.Length);
			// }

			string animhashname = string.Join(_dotChar, currentMecanimBaseLayer, constValue);

			if (currentMecanimBaseLayer == constValue || string.IsNullOrEmpty(currentMecanimBaseLayer))
				animhashname = constValue;


			// string hashName = layerName + _dotChar + constantName;

			string niceConstKey = SOMUtils.NicifyConstantName(constantName);

			string nameIntLine = $"{_public} {_static} {_int} {niceConstKey}Hash = {Animator.StringToHash(constValue)};";
			string fullPathIntLine = $"{_public} {_static} {_int} {niceConstKey}FullPathHash = {Animator.StringToHash(animhashname)};";

			string fullpathcomment = _indentStr + _doublefslash + animhashname;

			sb.WriteIndentations(indentLevel);
			sb.WriteLine(nameIntLine);
			sb.WriteIndentations(indentLevel);
			sb.WriteLine(fullPathIntLine + fullpathcomment);
		}
		static void WriteLine(this StringBuilder sb, string content = "")
		{

			sb.AppendLine(content);

		}

		static void WriteBlockComment(this StringBuilder sb, string content = "")
		{
			sb.AppendLine(_blockQuoteOpen + content + _blockQuoteClose);
		}
		static void WriteLineComment(this StringBuilder sb, string content = "", int indent = 1)
		{
			sb.WriteIndentations(indent);
			sb.AppendLine(_doublefslash + content);
		}
		public static void InsertLineComment(this StringBuilder sb, string content = "", int indent = 1)
		{
			if (sb.Length > _newLine.Length && sb.ToString(sb.Length - _newLine.Length, _newLine.Length) == _newLine)
			{
				// If the last characters are a newline, remove them
				sb.Remove(sb.Length - _newLine.Length, _newLine.Length);

			}

			int position = sb.Length;
			sb.Insert(position, _indentStr + _doublefslash + content);
			// Append newline after the comment
			sb.Append(_newLine);
		}
		static void WriteBrace(this StringBuilder sb, BraceState braceState, int indent = 1)
		{
			if (braceState == BraceState.Open)
			{
				sb.Append(_openBrace);
				IncreaseBraceCount(1);
			}
			else
			{
				sb.Append(_closeBrace);
				DecreaseBraceCount(1);
			}

			sb.AppendLine();
		}

		static void IncreaseBraceCount(int amount = 1)
		{
			RemainingOpenBraceCount += amount;
		}
		static void DecreaseBraceCount(int amount = 1)
		{
			if (amount <= 0 || RemainingOpenBraceCount == 0) return;
			RemainingOpenBraceCount -= amount;
		}

		static string GetIndentations(int indentLevel)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < indentLevel; i++)
			{
				builder.Append(_indentStr);
			}
			return builder.ToString();
		}
		static void WriteIndentations(this StringBuilder sb, int indentations = 0)
		{
			sb.Append(_indentChar, indentations);
		}

		static void RemoveIndentations()
		{
			if (_StringBuilder.Length > 0 && _StringBuilder[_StringBuilder.Length - 1] == '\t')
			{
				_StringBuilder.Remove(_StringBuilder.Length - 1, 1);
				DecreaseIndentationLevel();
			}
		}
		static void IncreaseIndentationLevel(int indentations = 1)
		{
			IndentationLevel += indentations;
		}
		static void DecreaseIndentationLevel(int indentations = 1)
		{
			if (indentations <= 0 || IndentationLevel == 0) return;
			IndentationLevel -= Math.Max(0, indentations);
		}
		static void SetIndentationLevel(int indentations = 1)
		{
			IndentationLevel = Math.Max(0, indentations);
		}
		static void ResetIndentationLevel()
		{
			IndentationLevel = 0;
		}
		public static string GetMecanimLayername(string[] parts)
		{
			string layerName = "";
			int layersIndex = Array.IndexOf(parts, SOMMecanimModule.layers);

			if (layersIndex != -1 && layersIndex < parts.Length - 1)
			{
				StringBuilder layerNameBuilder = new();

				for (int h = layersIndex + 1; h < parts.Length; h++)
				{
					if (parts[h] == SOMMecanimModule.StateMachines || parts[h] == SOMMecanimModule.states)
					{
						continue;
					}

					layerNameBuilder.Append(parts[h]);
					if (h < parts.Length - 1)
					{
						layerNameBuilder.Append(_dotStr);
					}
				}
				layerName = layerNameBuilder.ToString();
			}

			return layerName;
		}

	}
}
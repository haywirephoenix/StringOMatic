using System.Collections.Generic;
using System;

#if SOM_ADDRESSABLES
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

namespace SOM
{
    [Serializable]
    public class SOMDataHandler : SOMScriptableSingleton<SOMDataHandler>
    {
        public static int ModuleCount { get { return DataStore.RootCount(); } }
        private static SOMDictionary DataStore;

        public enum ConstType { String, Int, Float }

        public bool IResourcesNeedAssigning = false;

        private const string SentinelValue = "__SOMDBSENTINEL__";

#if SOM_ADDRESSABLES

        public List<string> resourceLocationAssetPaths;
        public List<string> resourceLocationClassPaths;

        // public Dictionary<string, IResourceLocation> foundIResourceLocs;

        public void AddIResource(string resourceKeypath, string resourceClassPath)
        {
            resourceLocationAssetPaths.Add(resourceKeypath);
            resourceLocationClassPaths.Add(resourceClassPath);
        }
        public void ClearIResourceData()
        {
            resourceLocationAssetPaths = new();
            resourceLocationClassPaths = new();
        }

#endif
        public static void AddConstant(SOMPathData data)
        {
            ExceptionIfNullOrEmpty(DataStore, data.Path, data.ConstName, data.ConstValue);

            string nicePath = SOMPathUtil.NicifyPath(data.Path);
            string niceName = SOMUtils.NicifyConstantName(data.ConstName);

            DataStore.Add(nicePath, niceName, data.ConstValue);
        }

        public static void AddConstant(string path, string constName, object constValue)
        {
            SOMPathData somPathData = new(path, constName, constValue);
            AddConstant(somPathData);
        }
        public static bool GetConstant(string path, string constName, out string constValue)
        {
            string key = SOMPathUtil.MakeConstantKeyPath(path, constName);

            if (ConstantExists(key))
            {
                constValue = DataStore.GetValue(key);
                return true;
            }
            else
            {
                constValue = null;
                return false;
            }
        }


        public static bool ConstantExists(string constantPath)
        {
            return DataStore.ContainsKey(constantPath);
        }

        public static SOMDictionary GetRootData()
        {
            return DataStore;
        }

        public static SortedList<string, string> GetAllConstants()
        {
            return DataStore.GetFlatData();
        }
        public static int RootClassCount()
        {
            return DataStore.RootCount();
        }

        public static void AddConstants(string modulePath, string[] names, string[] value, bool addClass = false)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string className = addClass ? names[i] : modulePath.Split(".")[^1];
                string keypath = SOMPathUtil.MakeConstantKeyPath(modulePath, className);

                AddConstant(keypath, names[i], value[i]);
            }
        }

        public static bool RemoveModule(string key)
        {
            return DataStore.Remove(key);
        }
        public static bool ModuleExists(string modulePath)
        {
            return DataStore.ContainsKey(modulePath);
        }

        public static void Reset()
        {
            CreateDatabase();
        }

        public static void CreateDatabase()
        {
            if (DataStore == null)
            {
                DataStore = new();
                DataStore.Clear();
            }
            else
            {
                DataStore.Clear();
            }


        }

        public static bool DatabaseExists()
        {
            return DataStore != null;
        }

        private static bool ExceptionIfNullOrEmpty(object database, string modulePath = SentinelValue, string constName = SentinelValue, object constValue = null)
        {
            if (database == null)
                throw new DatabaseNotExistException();

            if (modulePath != SentinelValue && String.IsNullOrEmpty(modulePath))
                throw new ArgumentException("modulePath cannot be empty.", nameof(modulePath));

            if (constName != SentinelValue && String.IsNullOrEmpty(constName))
                throw new ArgumentException("constName cannot be empty.", nameof(constName));

            if (constValue == null || constValue is string conststr && String.IsNullOrEmpty(conststr))
                throw new ArgumentException("constValue cannot be empty.", nameof(constValue));

            return false;
        }

        private static void ValidateArrays(params string[][] args)
        {
            foreach (var arr in args)
            {
                if (arr == null)
                    throw new ArgumentNullException(nameof(args), "One or more string arrays are null.");
                foreach (var str in arr)
                {
                    if (str == null)
                        throw new ArgumentNullException(nameof(args), "One or more strings in the arrays are null.");
                    if (str.Trim() == "")
                        throw new ArgumentException("One or more strings in the arrays are empty.", nameof(args));
                }
            }
        }

        public static void Save()
        {
            return;
        }


    }









}
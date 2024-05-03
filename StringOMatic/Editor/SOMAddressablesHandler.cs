using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;

using static SOM.LanguageConsts;

using System.Reflection;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.Initialization;



namespace SOM
{
    public static class SOMAddressablesHandler
    {


        public const string AddressablesModule = "AddressablesModule";
        public const string ClassName = "Addressables";

        public static void ValidateAndStoreResLoc(StringBuilder pathBuilder, string NamespaceName, string constName, object constValue)
        {

            if (constValue is IResourceLocation resourceLocation)
            {
                // string fullpath = pathBuilder.ToString().Remove(pathBuilder.Length - 1, 1);
                string builtPath = pathBuilder.ToString().TrimEnd(_dotChar);

                string classPath = string.Join(_dotChar, NamespaceName, AddressablesModule, builtPath, constName);



                SOMDataHandler.Singleton.AddIResource(resourceLocation.PrimaryKey, classPath);
                SOMDataHandler.Singleton.IResourcesNeedAssigning = true;
                // Debug.Log("added iresource to dict: " + classPath);
            }

        }

        private static IResourceLocator locator;

        // [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnLoad()
        {

            // SOMDataHandler singleton = SOMScriptableSingleton<SOMDataHandler>.Singleton;

            // if (SOMDataHandler.Singleton.IResourcesNeedAssigning)
            // {
            //     // SOMDataHandler.Singleton.IResourcesNeedAssigning = false;
            //     AssignIResources();
            // }

        }

        public static void AssignIResources()
        {

            var assetPaths = SOMDataHandler.Singleton.resourceLocationAssetPaths;
            var classPaths = SOMDataHandler.Singleton.resourceLocationClassPaths;


            if (classPaths == null)
            {
                Debug.Log("classPaths is null");
                return;
            }
            if (assetPaths == null)
            {
                Debug.Log("keyPaths is null");
                return;
            }

            // Debug.Log("assiging " + assetPaths.Count + " resources");

            // foreach (var kvp in foundIResourceLocs)
            for (int i = 0; i < assetPaths.Count; i++)
            {

                string assetPath = assetPaths[i];
                string fullclassPath = classPaths[i];
                // string[] keySplit = kvp.Key.Split(_dotChar);

                int lastDotIndex = fullclassPath.LastIndexOf(_dotChar);

                if (lastDotIndex >= 0)
                {
                    string classPath = fullclassPath.Substring(0, lastDotIndex);
                    string constName = fullclassPath.Substring(lastDotIndex + 1);

                    int lastDotIndex2 = classPath.LastIndexOf(_dotChar);
                    string classPathPlused = classPath.Substring(0, lastDotIndex2) + _plusChar + classPath.Substring(lastDotIndex2 + 1);



                    GetIResourceAssignToClassPath(assetPath, classPathPlused, constName);

                }
                else
                {
                    Debug.LogError($"Invalid path format: {assetPath}");
                }
            }

        }

        public const string AssemblyCSharp = "Assembly-CSharp";

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            Assembly csassembly = Assembly.Load(AssemblyCSharp);
            type = csassembly.GetType(typeName);

            if (type != null) return type;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }


        public static async void GetIResourceAssignToClassPath(string primaryKey, string classPath, string fieldName)
        {
            Type classType = GetType(classPath);

            if (classType == null)
            {
                Debug.LogError("bad classtype:" + classPath);
                return;
            }

            FieldInfo fieldInfo = classType.GetField(fieldName);

            if (fieldInfo == null)
            {
                Debug.LogError("bad field name:" + fieldName);
                return;
            }

            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(primaryKey);
            await locationsHandle.Task;

            if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<IResourceLocation> locations = locationsHandle.Result;
                foreach (IResourceLocation location in locations)
                {
                    fieldInfo.SetValue(null, location);
                }
            }
            else
            {
                Debug.LogError("Failed to get resource locations by PrimaryKey: " + locationsHandle.OperationException);
            }
        }


        #region module methods

        public static bool addressablesReady = false;
        public static bool waitingForSettings = false;

        public static void InitalizeAddressables()
        {
            addressablesReady = false;

            bool noSettingsPath = AddressablesRuntimeProperties.EvaluateString("Addressables.SettingsPath") == null;
            bool NoAddressablesDefault = AddressableAssetSettingsDefaultObject.Settings == null;


            if (noSettingsPath || NoAddressablesDefault)
            {
                waitingForSettings = true;
                return;
            }

            waitingForSettings = false;

            Addressables.InitializeAsync().Completed += OnInitializeComplete;
        }

        static void OnInitializeComplete(AsyncOperationHandle<IResourceLocator> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                locator = handle.Result;

                addressablesReady = true;
            }
        }


        public static void GenerateAddressableConstants()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                return;
            }

            List<AddressableAssetGroup> groups = settings.groups;
            Dictionary<System.Guid, IList<IResourceLocation>> locationsDict = SOMAddressablesUtility.MakeLocationsListDict(locator);

            foreach (var group in groups)
            {
                string groupName = group.Name;

                ProcessAssetGroup(group, groupName, locationsDict);
            }
        }

        private static void ProcessAssetGroup(AddressableAssetGroup group, string groupName, Dictionary<System.Guid, IList<IResourceLocation>> locationsDict)
        {
            List<AddressableAssetEntry> results = new List<AddressableAssetEntry>();
            group.GatherAllAssets(results, true, true, true);

            foreach (var asset in results)
            {
                if (!asset.IsSubAsset)
                {
                    ProcessMainAsset(asset, groupName, locationsDict);
                }
                else
                {
                    ProcessSubAsset(asset, groupName);
                }
            }
        }

        private static void ProcessMainAsset(AddressableAssetEntry asset, string groupName, Dictionary<System.Guid, IList<IResourceLocation>> locationsDict)
        {
            string assetPath = asset.AssetPath;
            string guidString = asset.parentGroup.Guid;

            if (!Guid.TryParse(guidString, out System.Guid guid))
            {
                Debug.Log($"Invalid GUID format for asset: {asset.AssetPath}");
                return;
            }

            string constPath = $"{ClassName}.{groupName}";

            SOMDataHandler.AddConstant(constPath, "GUID", guidString);
            SOMDataHandler.AddConstant(constPath, "MainAssetPath", assetPath);


            var assetGUID = Guid.Parse(asset.guid);
            var foundResourceLoc = SOMAddressablesUtility.FindResourceDict(assetGUID, locationsDict, asset.address);

            if (foundResourceLoc != null)
            {
                SOMDataHandler.AddConstant(constPath, "iResourceLocation", foundResourceLoc);
            }
        }

        private static void ProcessSubAsset(AddressableAssetEntry asset, string groupName)
        {
            string parentAssetPath = asset.AssetPath;
            UnityEngine.Object subAsset = asset.TargetAsset;
            string subAssetName = subAsset.name;
            string subAssetType = asset.TargetAsset.GetType().Name;

            string constPath = $"{ClassName}.{groupName}.SubAssets";
            string constName = $"{subAssetName}{subAssetType}";
            string constValue = asset.address;

            SOMDataHandler.AddConstant(constPath, constName, constValue);
        }


    }

    #endregion
}
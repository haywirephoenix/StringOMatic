#if SOM_ADDRESSABLES

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Linq;

using UnityEngine.ResourceManagement.ResourceLocations;
using System.Reflection;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

using static SOM.LanguageConsts;


namespace SOM
{
    public static class SOMAddressablesHandler
    {


        public const string AddressablesModule = "AddressablesModule";

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




        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnLoad()
        {
            if (SOMDataHandler.Singleton.IResourcesNeedAssigning)
            {
                // SOMDataHandler.Singleton.IResourcesNeedAssigning = false;
                AssignIResources();
            }

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



    }
}


#endif
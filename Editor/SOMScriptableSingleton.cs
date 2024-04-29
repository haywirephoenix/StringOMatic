using UnityEngine;
using System.IO;
using UnityEditor;
using System;

namespace SOM
{
    public class SOMScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {

        public static string Name { get { return typeof(T).Name; } }
        private static readonly object lockObject = new object();
        private static T singleton;
        public static T Singleton
        {
            get
            {
                lock (lockObject)
                {
                    if (singleton == null)
                    {
                        string assetPath = GetOrCreateAssetPath();
                        singleton = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                    }
                    return singleton;
                }
            }
        }

        private static string GetOrCreateAssetPath()
        {
            string[] guids = AssetDatabase.FindAssets(Name + " t:" + typeof(T).Name);
            string assetPath;
            if (guids.Length == 0)
            {
                singleton = ScriptableObject.CreateInstance<T>();
                guids = AssetDatabase.FindAssets(typeof(T).Name);
                if (guids.Length == 0)
                    throw new FileNotFoundException("File could not be found");
                string targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                targetPath = targetPath.Substring(0, targetPath.LastIndexOf("/"));
                assetPath = targetPath + "/" + Name + ".asset";
                AssetDatabase.CreateAsset(singleton, assetPath);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(singleton));
            }
            else
            {
                assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            return assetPath;
        }

    }
}
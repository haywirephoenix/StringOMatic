using UnityEngine;
using System.IO;
using UnityEditor;
using System;

namespace SOM
{
    public class SOMScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {

        public static string Name { get { return typeof(T).Name; } }
        private static T singleton;
        public static T Singleton
        {
            get
            {
                //Get an existing file or create a new one
                if (singleton == null)
                {
                    string[] guids = AssetDatabase.FindAssets(Name + " t:" + typeof(T).Name);
                    if (guids.Length == 0)
                    {
                        singleton = ScriptableObject.CreateInstance<T>();
                        guids = AssetDatabase.FindAssets(typeof(T).Name);
                        if (guids.Length == 0)
                            throw new FileNotFoundException("File could not be found");
                        string targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        targetPath = targetPath.Substring(0, targetPath.LastIndexOf("/"));
                        AssetDatabase.CreateAsset(singleton, targetPath + "/" + Name + ".asset");
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(singleton));
                    }
                    else
                        singleton = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(T)) as T;
                }
                return singleton;
            }
        }

    }
}
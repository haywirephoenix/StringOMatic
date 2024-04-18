using UnityEngine;
using System.IO;
using UnityEditor;
using System;

namespace SOM
{
    public class SOMScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {

        protected virtual string Name { get; set; } = "SOMScriptable";

        [NonSerialized] private static string _name;
        public SOMScriptableSingleton()
        {
            _name = Name;
        }

        private static T singleton;
        public static T Singleton
        {
            get
            {
                //Get an existing file or create a new one
                if (singleton == null)
                {
                    string[] guids = AssetDatabase.FindAssets(_name + " t:" + typeof(T).Name);
                    if (guids.Length == 0)
                    {
                        singleton = ScriptableObject.CreateInstance<T>();
                        guids = AssetDatabase.FindAssets(typeof(T).Name);
                        if (guids.Length == 0)
                            throw new FileNotFoundException("File could not be found");
                        string targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        targetPath = targetPath.Substring(0, targetPath.LastIndexOf("/"));
                        AssetDatabase.CreateAsset(singleton, targetPath + "/" + _name + ".asset");
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
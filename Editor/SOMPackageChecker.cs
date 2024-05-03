using System;
using UnityEditor;
using UnityEditor.PackageManager;
namespace SOM
{

    [InitializeOnLoad]
    public class SOMPackageChecker : AssetPostprocessor
    {

        static SOMPackageChecker()
        {
            AssetDatabase.importPackageStarted += OnImportPackageStarted;
        }

        private static void OnImportPackageStarted(string packagename)
        {
            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();
        }


    }

}
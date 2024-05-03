using System;
using UnityEditor;
using UnityEditor.PackageManager;
namespace SOM
{
    public class SOMPackageChecker : AssetPostprocessor
    {

        protected void OnPreprocessAsset()
        {

            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();

        }

    }

}
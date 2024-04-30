using System;
using UnityEditor;
using UnityEditor.PackageManager;
namespace SOM
{
    public class SOMPackageChecker : UnityEditor.Editor
    {

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Events.registeringPackages += OnRegisteringPackages;
        }

        private static void OnRegisteringPackages(PackageRegistrationEventArgs args)
        {

            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();

        }

    }

}
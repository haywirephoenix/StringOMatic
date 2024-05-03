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
            Events.registeredPackages += OnRegisteredPackages;
        }

        private static void OnRegisteredPackages(PackageRegistrationEventArgs args)
        {

            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();

        }

    }

}
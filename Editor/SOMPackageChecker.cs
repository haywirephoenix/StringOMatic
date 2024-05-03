using System;
using UnityEditor;
using UnityEditor.PackageManager;
namespace SOM
{

    [InitializeOnLoad]
    public class SOMPackageChecker
    {

        [InitializeOnLoadMethod]
        static void SubscribeToEvent()
        {
            Events.registeredPackages += RegisteredPackagesEventHandler;
        }

        static void RegisteredPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();
        }


    }

}
using UnityEditor;

namespace SOM
{
    public class SOMPackageChecker : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            SOMAddressableCheck.CheckForAddressables();
            SOMRewiredCheck.CheckForRewired();
        }
    }

}
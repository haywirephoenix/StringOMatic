<details>
<summary><b>Addressables Addon Installation</b></summary>
<br/>

### Install from a Git URL
Yoy can also install this package via Git URL. To load a package from a Git URL:

* Open [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) window.
* Click the add **+** button in the status bar.
* The options for adding packages appear.
* Select Add package from git URL from the add menu. A text box and an Add button appear.
* Enter the `https://github.com/haywirephoenix/StringOMatic.git#addressables-addon` Git URL in the text box and click Add.

<br/>
</details>


<details>
<summary><b>Addressables Usage</b></summary>
<br/>


If you have the Addressables addon installed, the Addressables module should now be included in the preferences window. When enabled, it will generate constant strings from your Addressables: 

MainAsset - AddressableAssetEntry.AssetPath, AddressableAssetGroup.Guid
SubAssets - AddressableAssetEntry.address

The MainAsset GUID is stored as a string, the same as Unity stores it.

<br/>

Loading all the animation clips in a bundled fbx:

```csharp
var handle = Addressables.LoadAssetAsync<AnimationClip[]>(AddressablesStatics.MyAnimations.mainAssetPath);

await handle.Task;

if (handle.Status == AsyncOperationStatus.Succeeded)
{
    AnimationClip[] myFBXAnims = handle.Result;
}
```

Loading a SubAsset (for example an animationclip in a bundled fbx):

```csharp 
var handle = Addressables.LoadAssetAsync<AnimationClip>(AddressablesStatics.MyAnimations.SubAssets.myanimationClip);

await handle.Task;

if (handle.Status == AsyncOperationStatus.Succeeded)
{
    AnimationClip myanimationClip = handle.Result;   
}
```

<br/>
</details>
 ![Title](https://github.com/haywirephoenix/StringOMatic/assets/26271795/23f7876c-90c9-4751-bb13-929804c7064d)  


With String-O-Matic, magic strings are over. Generate all your constants at the click of a button.

<details>
<summary><b>Description</b></summary>
<br/>
 Unity's policy for implementing magic strings in almost all of its systems is one the major sources of bugs and headaches, especially for amateurs. Using constant values instead of relying on magic strings heavily reduces the amount of runtime errors.
<br/><br/>
 String-O-Matic scans your project based on your needs to automatically generate constants and static classes holding references to all of those magic strings, effectively replacing soft runtime errors by robust compile errors.  
   
 </details>

<details>
<summary><b>Key Features</b></summary>
<br/> 
 
 *   Replace Unity magic strings for static classes and constants.
 *   Changing magic strings in the Editor will produce compile errors that would otherwise cause undesired beahviour.
 *   Comes with many built-in modules, each one addressing a different system.  
     
 *   Easily configurable based on each project needs.
 *   Easy to use: you just have to click a button.  
     
 *   Easy to extend: you can create your own modules, satisfying your own project needs.
 *   Fully documented =)
 *   And last but not least, full **source code included**.
<br/>
</details>

<details>
<summary><b>Modules</b></summary>
<br/> 
 
 *   Tags
 *   Layers
 *   Sorting Layers
 *   Input axes  
     
 *   Navigation Areas
 *   Scenes -- Lists every scene name and path added to the build.  
     
 *   Audio -- Lists every mixer controller and, for each one, their exposed parameters and snaphots.
 *   Mecanim-- Lists every animator's parameters, layers, states and sub state machines, recursively.
 *   Resources-- Lists every object under your resources folder/s and subfolders.
 *   Shaders-- Lists the shader name and it's properties for every built-in and custom shader

</details>

<details>
<summary><b>Planned Features</b></summary>
<br/>


- [ ] Add "Update Available" banner with Update button in preferences
- [ ] Fix filter lists, make universal filter template
- [ ] Add filter lists for more categories
- [ ] Add directory selection shortcut
- [ ] Add whitelist object field
- [ ] Udate docs for module creation in the new format
- [x] Add Addressables feature (Complete - in testing)
- [x] Add support for multiple constant types (Complete)
- [x] Support package manager git url (Complete)
- [x] Animator hash generation (Complete)
- [x] Improved compatibility with newer versions of Unity (Complete)
- [x] Text fields to specify the namespace, target directory, and class name. (Complete)
- [x] Remove XML intermediary generation (Complete)
- [x] Improve performance (Complete)


</details>

<details>
<summary><b>Installation</b></summary>
<br/>

### Install from a Git URL
Yoy can also install this package via Git URL. To load a package from a Git URL:

* Open [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) window.
* Click the add **+** button in the status bar.
* The options for adding packages appear.
* Select Add package from git URL from the add menu. A text box and an Add button appear.
* Enter the `https://github.com/haywirephoenix/StringOMatic.git` Git URL in the text box and click Add.

<br/>
</details>

<details>
<summary><b>Getting Started</b></summary>
<br/>

* Open Preferences > String-O-Matic or Tools > String-O-Matic > Preferences
* Toggle the modules that you want to generate - click on them as some have more sub options.
* Toggle any of the customization options at the top if you would like to change them.
* Press the Refresh All button at the bottom.
* Your new consts file will be generated in your project.

<br/>
</details>


<details>
<summary><b>Usage</b></summary>
<br/>

Once you've generated your constants, in your project you will have access to all the module classes.

If you used the "Wrap modules in namespaces" option:
```csharp 
using StringOMatic.InputModule;
```
Then you can access them like this:
```csharp 
Input.GetAxis(InputStatics.horizontal)
```
Or you can create a shortcut to a specific class:
```csharp 
using MyControllerParams = StringOMatic.MecanimModule.MecanimStatics.Controllers.MyController.Parameters;
```
Then you can reference them like this:
```csharp 
animator.SetFloat(MyControllerParams.horizontalFullPathHash,x);6
```

<br/>
</details>

<details>
<summary><b>Addressables</b></summary>
<br/>

If you have the Unity Addressables package installed, the Addressables module should now be included in the preferences window. When enabled, it will generate constant strings from your Addressables: 

MainAsset - AddressableAssetEntry.AssetPath, AddressableAssetGroup.Guid
SubAssets - AddressableAssetEntry.address

The MainAsset GUID is stored as a string, the same as Unity stores it.

<br/>
</details>


<details>
<summary><b>Addressables Usage</b></summary>
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

<details>
<summary><b>Updates</b></summary>
<br/>

>##### v1.1.1:
>* Added a filter list to the Mecanim module
>* Fix Refresh from reverting animation controller changes
>* Mechanim module now adds Animator.StringToHash ints
>* Fix preferences layout and menuitem.
>* Add github repo button to preferences
>##

>##### v2.0.0:
>* Update C# generation - completely rewritten
>* Update StringToHash int generation
>* Remove XML generation intermediary step
>* Add namespace generation to modules with "Module" suffix

>* Create new data structure for storing and generating constants
>* Update all modules to support new structure
>* Update Rewired module to search for InputManager prefab or scene
>* Upate Resources module - new project scanning and const gen approach

>* Update Preferences UI + current and backwards compatibility
>* Add new fields to Preferences UI to customize Path, Class and Namespace
>* Fix MenuItem methods, with backwards compatibility
>* New and improved Animator Hash generation

>##### v2.0.1 - 2.0.3:
>* Fix minor bugs
>* Add package manager support

>##### v2.0.4:
>* Add addressables support
>* Add support for multiple constant types

</details>

<br/>

Convert **error-prone** code  
![SOM Not Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/5b56fd22-9cd0-446d-adc1-ff51c916910e)  
into **safe, robust** code  
![SOM Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/65b0cbbf-ae8f-4faa-b14b-462cba76a05e) ​

A continuation of the [work done](https://assetstore.unity.com/packages/tools/utilities/string-o-matic-53019) by Cobo Antonio [with permission](https://forum.unity.com/threads/released-string-o-matic-say-goodbye-to-magic-strings.377123#post-9764475). Pull requests welcome!
 ![Title](https://github.com/haywirephoenix/StringOMatic/assets/26271795/23f7876c-90c9-4751-bb13-929804c7064d)  


With String-O-Matic, magic strings are over. Generate all your constants at the click of a button.

<details>
<summary><b>Description</b></summary>
<br/>
 Unity's policy for implementing **magic strings** in almost all of its systems **is one the major sources of bugs** **and headaches**, especially for amateurs. **Using constant values** instead of relying on magic strings heavily **reduces** the amount of **runtime errors.**

<br/>

 **String-O-Matic** scans your project based on your needs to automatically **generate constants and static classes** holding references to all of those magic strings, effectively **replacing** soft **runtime errors by** robust **compile errors.**  
   
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


- [ ] Support package manager git url
- [ ] Add "Update Available" banner with Update button in preferences
- [ ] Add filter lists for more categories
- [x] Animator hash generation
- [x] Improved compatibility with newer versions of Unity
- [x] Text fields to specify the namespace, target directory, and class name.
- [x] Remove XML intermediary generation
- [x] Improve performance


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

</details>

<br/>

Convert **error-prone** code  
![SOM Not Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/5b56fd22-9cd0-446d-adc1-ff51c916910e)  
into **safe, robust** code  
![SOM Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/65b0cbbf-ae8f-4faa-b14b-462cba76a05e) ​

A continuation of the [work done](https://assetstore.unity.com/packages/tools/utilities/string-o-matic-53019) by Cobo Antonio [with permission](https://forum.unity.com/threads/released-string-o-matic-say-goodbye-to-magic-strings.377123#post-9764475). Pull requests welcome!
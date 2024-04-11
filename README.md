> ![Title](https://github.com/haywirephoenix/StringOMatic/assets/26271795/23f7876c-90c9-4751-bb13-929804c7064d)  
>
> A continuation of the [work done](https://assetstore.unity.com/packages/tools/utilities/string-o-matic-53019) by Cobo Antonio [with permission](https://forum.unity.com/threads/released-string-o-matic-say-goodbye-to-magic-strings.377123#post-9764475). Pull requests welcome!
> 
>#### Planned Features:
> * Improved compatibility with newer version of Unity
> * Text fields to specify the namespace, target directory, and class name.
> * Filter lists for more categories
><br></br>
>----------------
>
> With String-O-Matic, magic strings are over.
>
>
> Unity's policy for implementing **magic strings** in almost all of its systems **is one the major sources of bugs** **and headaches**, especially for amateurs. **Using constant values** instead of relying on magic strings heavily **reduces** the amount of **runtime errors.**  
>   
> **String-O-Matic** scans your project based on your needs to automatically **generate constants and static classes** holding references to all of those magic strings, effectively **replacing** soft **runtime errors by** robust **compile errors.**  
>   
> 
> Convert **error-prone** code  
> ![SOM Not Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/5b56fd22-9cd0-446d-adc1-ff51c916910e)  
> into **safe, robust** code  
> ![SOM Used](https://github.com/haywirephoenix/StringOMatic/assets/26271795/65b0cbbf-ae8f-4faa-b14b-462cba76a05e) ​
> 
>   
> **Key Features:**  
> 
> *   Replace Unity magic strings for static classes and constants.
> *   Changing magic strings in the Editor will produce compile errors that would otherwise cause undesired beahviour.
> *   Comes with many built-in modules, each one addressing a different system.  
>     
> *   Easily configurable based on each project needs.
> *   Easy to use: you just have to click a button.  
>     
> *   Easy to extend: you can create your own modules, satisfying your own project needs.
> *   Fully documented =)
> *   And last but not least, full **source code included**.
> 
> **Modules:**  
> 
> *   Tags
> *   Layers
> *   Sorting Layers
> *   Input axes  
>     
> *   Navigation Areas
> *   Scenes --> Lists every scene name and path added to the build.  
>     
> *   Audio --> Lists every mixer controller and, for each one, their exposed parameters and snaphots.
> *   Mecanim--> Lists every animator's parameters, layers, states and sub state machines, recursively.
> *   Resources--> Lists every object under your resources folder/s and subfolders.
> *   Shaders--> Lists the shader name and it's properties for every built-in and custom shader
> 
> ![Preferences](https://github.com/haywirephoenix/StringOMatic/assets/26271795/f5b29da7-abf6-4994-b3bc-6a39a41bc728)  
> ​
> 
> **Version 1.1:**  
> 
> *   Added SOMValueAttribute to reference String-O-Matic values from the inspector.
> *   Added Sorting Layers, Navigation and Shaders modules to the free version.  
>     
> *   Bug fixes.
> 
> ![SOMValueAttribute](https://github.com/haywirephoenix/StringOMatic/assets/26271795/a6cb5985-ecb4-4257-8409-9f237fea13bb)  
> ​
> 
> **Version 1.1.1:**  
>
> * Added a filter list to the Mecanim module
> * Fix Refresh from reverting animation controller changes
> * Mechanim module now adds Animator.StringToHash ints 
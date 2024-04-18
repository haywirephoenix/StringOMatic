using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.IO;

#if SOM_REWIRED
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using InputManager = Rewired.InputManager;
#endif

namespace SOM
{
    public class SOMRewiredModule : SOMModule
    {
        //==================================
        //Initialization
        //==================================
        [InitializeOnLoadMethod]
        static void CheckForRewired()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (Type.GetType("Rewired.ReInput, Rewired_Core") == null)
            {
                if (defines.Contains(defineSymbol))
                    defines = defines.Remove(defines.IndexOf(defineSymbol), defineSymbol.Length);
            }
            else
            {
                if (!defines.Contains(defineSymbol))
                    defines = defines + ";" + defineSymbol;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }

        //==================================
        //Consts
        //==================================
        const string ADD_CATEGORIES_SUFFIX_KEY = "Rewired Module Add Categories Suffix Key";
        const string ADD_CATEGORIES_SUFFIX_LABEL = "Add Categories Suffix";
        const string ADD_CATEGORIES_SUFFIX_TOOLTIP = "Add the \"Category\" suffix at the end of every category module name";

        const string GROUP_LAYOUTS_KEY = "Rewired Module Group Layouts Key";
        const string GROUP_LAYOUTS_LABEL = "Group Layouts";
        const string GROUP_LAYOUTS_TOOLTIP = "Group all layouts into a generic \"Layouts\" submodule";

        //==================================
        //Vars
        //==================================
        static string defineSymbol = "SOM_REWIRED";

        //==================================
        //Properties
        //==================================
        public override string moduleName
        {
            get { return "Rewired"; }
        }
        static bool addCategoriesSuffix
        {
            get
            {
                if (!SOMPreferences.bools.Contains(ADD_CATEGORIES_SUFFIX_KEY))
                    addCategoriesSuffix = true;
                return SOMPreferences.bools[ADD_CATEGORIES_SUFFIX_KEY];
            }
            set
            {
                SOMPreferences.bools[ADD_CATEGORIES_SUFFIX_KEY] = value;
            }
        }
        static bool groupLayouts
        {
            get
            {
                if (!SOMPreferences.bools.Contains(GROUP_LAYOUTS_KEY))
                    groupLayouts = true;
                return SOMPreferences.bools[GROUP_LAYOUTS_KEY];
            }
            set
            {
                SOMPreferences.bools[GROUP_LAYOUTS_KEY] = value;
            }
        }

        //===================================
        //Refresh
        //===================================
        public override void Refresh()
        {
#if SOM_REWIRED
            //Players
            string playersModule = moduleName + ".Players";
            //SOMDataHandler.AddModule(playersModule);


            if (!FindRewiredModule(out InputManager inputManager))
            {
                return;
            }
            UserData data = inputManager.userData;
            AddNamesTo(data.GetPlayerNames(), playersModule);

            //Actions
            string actionsModule = moduleName + ".Actions";
            //SOMDataHandler.AddModule(actionsModule);
            string[] names = data.GetActionCategoryNames();
            for (int i = 0; i < names.Length; i++)
            {
                InputCategory actionCategory = data.GetActionCategory(i);
                string categoryModule = actionsModule + "." + actionCategory.name + (addCategoriesSuffix ? "Category" : string.Empty);
                //SOMDataHandler.AddModule(categoryModule);
                SOMDataHandler.AddConstant(categoryModule, "categoryName", actionCategory.name);
                AddNamesTo(data.GetSortedActionNamesInCategory(actionCategory.id), categoryModule);
            }

            //Input Behaviours
            string inputBehavioursModule = moduleName + ".InputBehaviours";
            //SOMDataHandler.AddModule(inputBehavioursModule);
            AddNamesTo(data.GetInputBehaviorNames(), inputBehavioursModule);

            //Custom Controllers
            string customControllersModule = moduleName + ".CustomControllers";
            //SOMDataHandler.AddModule(customControllersModule);
            names = data.GetCustomControllerNames();
            for (int i = 0; i < names.Length; i++)
            {
                CustomController_Editor customController = data.GetCustomController(names[i]);
                string customControllerModule = customControllersModule + "." + customController.name;
                //SOMDataHandler.AddModule(customControllerModule);
                SOMDataHandler.AddConstant(customControllerModule, "name", customController.name);
                //Axes
                string axesModule = customControllerModule + ".Axes";
                //SOMDataHandler.AddModule(axesModule);
                for (int j = 0; j < customController.axisCount; j++)
                    SOMDataHandler.AddConstant(axesModule, SOMUtils.NicifyConstantName(customController.axes[j].name), customController.axes[j].name);

                //Buttons
                string buttonsModule = customControllerModule + ".Buttons";
                //SOMDataHandler.AddModule(buttonsModule);
                for (int j = 0; j < customController.buttonCount; j++)
                    SOMDataHandler.AddConstant(buttonsModule, SOMUtils.NicifyConstantName(customController.buttons[j].name), customController.buttons[j].name);
            }

            //Layouts
            {
                string layoutsModule = moduleName + (groupLayouts ? ".Layouts" : string.Empty);
                // if (!SOMDataHandler.ModuleExists(layoutsModule))
                //SOMDataHandler.AddModule(layoutsModule);
                //Joystick
                string joystickLayoutsModule = layoutsModule + ".Joystick" + (!groupLayouts ? "Layouts" : string.Empty);
                //SOMDataHandler.AddModule(joystickLayoutsModule);
                AddNamesTo(data.GetJoystickLayoutNames(), joystickLayoutsModule);
                //Keyboard
                string keyboardLayoutsModule = layoutsModule + ".Keyboard" + (!groupLayouts ? "Layouts" : string.Empty);
                //SOMDataHandler.AddModule(keyboardLayoutsModule);
                AddNamesTo(data.GetKeyboardLayoutNames(), keyboardLayoutsModule);
                //Mouse
                string mouseLayoutsModule = layoutsModule + ".Mouse" + (!groupLayouts ? "Layouts" : string.Empty);
                //SOMDataHandler.AddModule(mouseLayoutsModule);
                AddNamesTo(data.GetMouseLayoutNames(), mouseLayoutsModule);
                //Custom Controllers
                string customControllerLayoutsModule = layoutsModule + ".CustomController" + (!groupLayouts ? "Layouts" : string.Empty);
                //SOMDataHandler.AddModule(customControllerLayoutsModule);
                AddNamesTo(data.GetCustomControllerLayoutNames(), customControllerLayoutsModule);
            }

            //Maps
            {
                string mapsModule = moduleName + ".Maps";
                //SOMDataHandler.AddModule(mapsModule);
                //Categories
                string mapCategoriesModule = mapsModule + ".Categories";
                //SOMDataHandler.AddModule(mapCategoriesModule);
                AddNamesTo(data.GetMapCategoryNames(), mapCategoriesModule);
                //Joystick
                string joystickMapsModule = mapsModule + ".Joystick";
                //SOMDataHandler.AddModule(joystickMapsModule);
                Guid[] joystickGuids = inputManager.dataFiles.GetJoystickGuids();
                string joystickNamesModule = joystickMapsModule + ".Names";
                //SOMDataHandler.AddModule(joystickNamesModule);
                for (int i = 0; i < joystickGuids.Length; i++)
                {
                    SOMDataHandler.AddConstant(joystickNamesModule, SOMUtils.NicifyConstantName(inputManager.dataFiles.GetEditorJoystickNames()[i]),
                                              inputManager.dataFiles.GetEditorJoystickNames()[i]);
                    List<ControllerMap_Editor> maps = data.GetJoystickMaps(joystickGuids[i]);
                    if (maps.Count == 0)
                        continue;
                    string joystickModule = joystickMapsModule + "." + SOMUtils.NicifyModuleName(inputManager.dataFiles.GetEditorJoystickNames()[i]);
                    //SOMDataHandler.AddModule(joystickModule);
                    Debug.Log(maps[0].name);
                }
            }

#else
			if (SOMDataHandler.ModuleExists(moduleName))
				SOMDataHandler.RemoveModule(moduleName);
#endif
        }

#if SOM_REWIRED

        bool FindRewiredModule(out InputManager inputManager)
        {
            inputManager = GameObject.FindObjectOfType<InputManager>();

            if (inputManager != null)
            {
                return true; // Found in scene
            }

            // Search for the prefab asset containing the InputManager component
            string[] guids = AssetDatabase.FindAssets("t:GameObject");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null && prefab.GetComponent<InputManager>() != null)
                {
                    // Found a prefab containing the InputManager component
                    inputManager = prefab.GetComponent<InputManager>();
                    return true;
                }
            }

            throw new FileNotFoundException("InputManager prefab asset not found");
        }


#endif
        void AddNamesTo(string[] names, string module)
        {
            for (int i = 0; i < names.Length; i++)
                SOMDataHandler.AddConstant(module, SOMUtils.NicifyConstantName(names[i]), names[i]);
        }


        public override void DrawPreferences()
        {
#if SOM_REWIRED
            addCategoriesSuffix = EditorGUILayout.ToggleLeft(new GUIContent(ADD_CATEGORIES_SUFFIX_LABEL, ADD_CATEGORIES_SUFFIX_TOOLTIP), addCategoriesSuffix);
            groupLayouts = EditorGUILayout.ToggleLeft(new GUIContent(GROUP_LAYOUTS_LABEL, GROUP_LAYOUTS_TOOLTIP), groupLayouts);
            base.DrawPreferences();
#else
			EditorGUILayout.HelpBox("Rewired is not installed",MessageType.Error);
#endif
        }
    }
}
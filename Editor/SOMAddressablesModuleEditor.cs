using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;

namespace SOM
{
    public class SOMAddressablesEditor
    {

        const string resourcesWord = "Resources";
        const string dirWord = "Dir";
        const string assetWord = "Asset";
        const string metaWord = "meta";

        const string GENERATE_ADDRESSABLES = "Generate Addressables";
        const string GENERATE_ADDRESSABLES_TOOLTIP = "Automatically add resources to addressable groups by filter";


        const string fileExtensionSeparator = "_";

        public const string ClassName = "Addressables";

        /*       private const Char atChar = '@';
              private const Char dotChar = '.';
              private const Char _underscore = '_'; */


        #region addressables editor properties

        // static bool generateAddressables
        // {
        // 	get
        // 	{
        // 		if (!SOMPreferences.bools.Contains(GENERATE_ADDRESSABLES))
        // 			generateAddressables = false;
        // 		return SOMPreferences.bools[GENERATE_ADDRESSABLES];
        // 	}
        // 	set
        // 	{
        // 		SOMPreferences.bools[GENERATE_ADDRESSABLES] = value;
        // 	}
        // }


        #endregion


        #region editor vars

        static List<SOMAddressablesUtility.AddressableListItem> addressableListItems;

        // private Vector2 scrollPos = new();
        private int selected = -1;
        private Vector2 scrollPosition;
        private bool unfoldAssets = true;
        private bool init;

        #endregion

        #region editor consts

        const string FILES = "Files";
        const string ASSETS = "Assets";
        const string ADD_ASSET = "Add Asset";
        const string GROUP_NAME = "Group Name: ";
        const string BUILT_IN_DATA = "Built In Data";
        const string SAVE = "Save Group";
        const string DELETE = "Delete";
        const string X = "X";
        const string ADD_NEW_GROUP = "Add New Group";
        const string NEW_GROUP_UNSAVED = "New group (unsaved)";

        #endregion

        #region editor methods

        private void Init()
        {
            init = true;

            addressableListItems = new();
            // scrollPos = new();

            var groups = SOMAddressablesUtility.GetAddressableGroups();

            foreach (var group in groups)
            {

                SOMAddressablesUtility.AddressableListItem addressableListItem = new(group);

                addressableListItems.Add(addressableListItem);

            }
        }

        private void DrawAddressableFilter()
        {
            if (!init)
            {
                Init();
            }

            EditorGUILayout.Space(5);

            GUILayout.Label("Addressable Filters", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            // Check if the loop should be updated
            // if (shouldUpdateLoop)
            // {
            UpdateGuiElements();
            // shouldUpdateLoop = false; // Reset the flag
            // }

            // Render GUI elements

        }



        private bool DrawDeleteButton(Action callback = null)
        {
            bool buttonPressed = GUILayout.Button(X, GUILayout.Width(25));
            if (buttonPressed)
            {
                callback?.Invoke();
            }

            return buttonPressed;
        }


        private void UpdateGuiElements()
        {
            GUIStyle style = new((GUIStyle)"OL Title");

            // GUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace();
            // scrollPos = GUILayout.BeginScrollView(scrollPos, (GUIStyle)"OL Box");
            // GUILayout.BeginVertical();
            // int controlId = GUIUtility.GetControlID(SOMManager.CONTROL_ID.GetHashCode(), FocusType.Keyboard);
            // guiElements.Clear();
            // Display each item in the addressableListItems list
            for (int i = 0; i < addressableListItems.Count; i++)
            // foreach (SOMAddressablesUtility.AddressableListItem listItem in addressableListItems)
            {
                SOMAddressablesUtility.AddressableListItem listItem = addressableListItems[i];



                EditorGUI.BeginChangeCheck();

                bool thisIsSelected = i == selected;
                bool isBuiltInData = listItem.GroupName == BUILT_IN_DATA;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Toggle(thisIsSelected, listItem.GroupName, style);

                GUI.enabled = !isBuiltInData;
                if (DrawDeleteButton())
                {

                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                // GUILayout.Space(8);

                if (EditorGUI.EndChangeCheck())
                {
                    selected = thisIsSelected ? -1 : i;
                    // GUIUtility.keyboardControl = controlId;
                }

                if (thisIsSelected)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawAddressableListItem(listItem);
                    EditorGUILayout.EndVertical();

                }


                continue;


            }

            // GUILayout.EndVertical();

            // GUILayout.EndScrollView();

            // GUILayout.EndHorizontal();



            // GUILayout.Space(20);

            // Add a button to add a new item

            if (GUILayout.Button(ADD_NEW_GROUP))
            {
                AddressableAssetGroup tempGroup = new();
                tempGroup.Name = NEW_GROUP_UNSAVED;
                addressableListItems.Add(new SOMAddressablesUtility.AddressableListItem(tempGroup));
            }

        }

        SOMFilters.FilterList _list;

        SOMFilters.FilterList list
        {
            get
            {
                if (_list == null)
                    _list = new SOMFilters.FilterList(ClassName, ClassName, SOMFilters.FilterList.FilterType.Black);
                return _list;
            }
        }


        private void DrawAddressableListItem(SOMAddressablesUtility.AddressableListItem listItem)
        {

            // GUILayout.BeginHorizontal();


            // GUILayout.Space(-10);

            bool isBuiltInData = listItem.GroupName == BUILT_IN_DATA;

            GUI.enabled = !isBuiltInData;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GROUP_NAME, GUILayout.Width(80));
            listItem.GroupName = EditorGUILayout.TextField(listItem.GroupName);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUI.enabled = true;



            unfoldAssets = EditorGUILayout.Foldout(unfoldAssets, ASSETS);

            // If foldout is open, display the list
            if (unfoldAssets)
            {
                foreach (var asset in listItem.Assets)
                {
                    GUILayout.BeginHorizontal();

                    GUI.enabled = false;

                    EditorGUILayout.ObjectField(asset.MainAsset, typeof(UnityEngine.Object), true);
                    EditorGUILayout.TextField(asset.AssetPath);

                    // if (DrawDeleteButton())
                    // {

                    // }

                    GUILayout.EndHorizontal();

                }

                // if (GUILayout.Button(ADD_ASSET))
                // {

                // }

            }

            GUILayout.Space(20);

            if (!isBuiltInData)
            {

                list.DrawLayout();

                GUILayout.Space(20);

                if (GUILayout.Button(SAVE))
                {

                }

            }




            GUI.enabled = true;


            GUILayout.Space(10);








            // return;

            // EditorGUILayout.LabelField("Group Name:", GUILayout.Width(80));
            // // listItem.GroupName = EditorGUILayout.TextField(listItem.GroupName);


            // EditorGUILayout.Space();

            // EditorGUILayout.LabelField("Directories:");
            // foreach (var directory in listItem.Directories)
            // {
            // 	EditorGUILayout.LabelField(directory);
            // }

            // EditorGUILayout.Space();

            // EditorGUILayout.LabelField("Files:");
            // foreach (var file in listItem.Files)
            // {
            // 	EditorGUILayout.LabelField(file);
            // }

            // EditorGUILayout.Space();








        }

        #endregion
    }

}
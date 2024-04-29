using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;

namespace SOM
{

    public class SOMFilters
    {
        public class FilterList
        {
            const float minPopupWidth = 70;
            public enum FilterListType { String, Path }
            public enum FilterType { None, Black, White };
            public FilterType type;
            List<string> list;
            public ReorderableList reorderableList { get; private set; }
            readonly string title;
            readonly string prefsKey;
            readonly bool fixedFilter;

            string prefsFilterType { get { return prefsKey + "FilterType"; } }
            string prefsItem { get { return prefsKey + "Item"; } }
            public string[] values { get { return list.ToArray(); } }
            public bool hasFilter { get { return type != FilterType.None; } }
            public bool isWhite { get { return type == FilterType.White; } }
            public bool isBlack { get { return type == FilterType.Black; } }

            public FilterList(string prefsKey, string title = "", FilterType type = FilterType.None, string[] list = null, bool fixedFilter = false)
            {
                this.prefsKey = prefsKey + "FilterList";
                this.title = title;
                this.fixedFilter = fixedFilter;

                //Try and load data from SOMPreferences
                if (!SOMPreferences.ints.Contains(prefsFilterType))
                {
                    SOMPreferences.ints[prefsFilterType] = (int)type;
                    this.type = type;
                    if (list == null)
                        list = new string[0];
                    this.list = new List<string>(list);
                }
                //If no such data, create new one
                else
                {
                    this.type = (FilterType)SOMPreferences.ints[prefsFilterType];
                    this.list = new List<string>();
                    int counter = 0;
                    while (SOMPreferences.strings.Contains(prefsItem + counter))
                        this.list.Add(SOMPreferences.strings[prefsItem + counter++]);
                }
                CreateReorderableList();
            }


            void CreateReorderableList()
            {
                reorderableList = new ReorderableList(list, typeof(string), false, true, true, true);

                //The header displays a label with the Title and a auto-size-adjustable FilterType popup menu
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, title);
                    float shift = EditorStyles.label.CalcSize(new GUIContent(title)).x;
                    rect.x += EditorStyles.label.CalcSize(new GUIContent(title)).x;
                    rect.width = Mathf.Max(rect.width - shift, minPopupWidth);
                    GUI.enabled = !fixedFilter;
                    EditorGUI.BeginChangeCheck();
                    type = (FilterType)EditorGUI.Popup(rect, (int)type, new GUIContent[]{
                        new GUIContent("No filter", "No filter whatsoever (useful for debugging)"),
                        new GUIContent("Black filter", "Values on this filter will NOT be added"),
                        new GUIContent("White filter", "ONLY values on this filter will be added")});
                    if (EditorGUI.EndChangeCheck())
                        SOMPreferences.ints[prefsFilterType] = (int)type;
                    GUI.enabled = true;
                };

                //Each element draws those nice horizontal lines at the start (to make it easy-selectable) and a text field
                reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.x -= 5;
                    ReorderableList.defaultBehaviours.DrawElementDraggingHandle(rect, index, isActive, isFocused, true);
                    EditorGUI.BeginChangeCheck();
                    list[index] = EditorGUI.TextField(new Rect(rect.x + 20, rect.y + 2, rect.width - 15, EditorGUIUtility.singleLineHeight), list[index]);
                    if (EditorGUI.EndChangeCheck())
                        SOMPreferences.strings[prefsItem + index] = list[index];
                };
                reorderableList.onAddCallback = (ReorderableList listInternal) =>
                {
                    list.Add(string.Empty);
                    listInternal.list = list;
                    SOMPreferences.strings[prefsItem + (list.Count - 1)] = string.Empty;
                };
                //When deleting an element, we shift down every other value and delete the last one
                reorderableList.onRemoveCallback = (ReorderableList listInternal) =>
                {
                    list.RemoveAt(listInternal.index);
                    for (int i = 0; i < list.Count; i++)
                        SOMPreferences.strings[prefsItem + i] = list[i];
                    SOMPreferences.strings.Delete(prefsItem + list.Count);
                    listInternal.list = list;
                };
            }

            public void DrawLayout()
            {
                reorderableList.DoLayoutList();
            }
            public void Draw(Rect rect)
            {
                reorderableList.DoList(rect);
            }


        }











    }


}
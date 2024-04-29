using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOM
{
    public class SOMDictionary : Dictionary<string, object>
    {
        private const Char pathSeparator = '.';
        private SortedList<string, string> flatData = new();
        public void Add(string path, string key, object value)
        {
            string flatpath = string.Join(pathSeparator, path, key);

            if (!flatData.ContainsKey(flatpath))
            {
                flatData.Add(flatpath, value.ToString());
                AddToDict(path, key, value);
            }
        }

        private void AddToDict(string path, string key, object value)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path is empty on " + key);
                return;
            }

            string[] pathSegments = path.Split(pathSeparator);

            // Get or add the root dictionary based on the first segment of the path
            Dictionary<string, object> root = GetOrAddRoot(pathSegments[0]);
            Dictionary<string, object> currentDict = root;

            // Traverse the path segments, starting from the second segment
            for (int i = 1; i < pathSegments.Length; i++)
            {
                string segment = pathSegments[i];

                // Check if the current dictionary contains the segment as a key
                if (!currentDict.TryGetValue(segment, out object currentObject))
                {
                    // If the segment doesn't exist, add it as a new nested dictionary
                    if (GetOrAddNestedDict(currentDict, segment, default, out var newDict))
                    {
                        currentDict = newDict;
                    }
                    else
                    {
                        throw new Exception($"Unable to get or add child {segment}");
                    }
                }
                else
                {
                    // If the segment exists, update the current dictionary and continue
                    if (currentObject is Dictionary<string, object> nestedDict)
                    {
                        currentDict = nestedDict;
                    }
                    else
                    {
                        // Handle the case where the segment already exists but is not a dictionary
                        throw new Exception($"The segment {segment} already exists and is not a dictionary");
                    }
                }
            }

            // Debug.Log($"added {path}.{key}.{value}");
            // At this point, currentDict points to the deepest nested dictionary
            // Add or update the key-value pair
            // GetOrAddNestedDict(currentDict, key, value);

            AddToNestedDict(currentDict, key, value);
        }

        public new bool ContainsKey(string path)
        {
            string pathRemLast = string.Join(pathSeparator, path.Split(pathSeparator)[^1..]);

            return flatData.ContainsKey(pathRemLast);
        }

        public bool KeyExists(string key)
        {
            return KeyExists(this, key);
        }

        private bool KeyExists(Dictionary<string, object> dict, string key)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Key == key)
                {
                    if (kvp.Value is Dictionary<string, object> nestedDict)
                    {
                        return KeyExists(nestedDict, key);
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool GetNestedDict(Dictionary<string, object> nestedParent, string key, string value, out Dictionary<string, object> newDict)
        {
            if (!nestedParent.ContainsKey(key))
            {
                newDict = new Dictionary<string, object>();
                nestedParent.Add(key, newDict);

                // Set the value for the new key
                if (value != null && value != default)
                {
                    newDict[key] = value;
                }


                return true;
            }
            else
            {
                // If the key exists, update the value if provided
                if (value != null && nestedParent[key] is string currentValue && currentValue != value)
                    nestedParent[key] = value;

                newDict = nestedParent[key] as Dictionary<string, object>;
                return true;
            }
        }


        private bool GetOrAddNestedDict(Dictionary<string, object> nestedParent, string key, string value = default)
        {
            return GetOrAddNestedDict(nestedParent, key, value, out _);
        }
        private bool GetOrAddNestedDict(Dictionary<string, object> nestedParent, string key, string value, out Dictionary<string, object> newDict)
        {
            if (!nestedParent.ContainsKey(key))
            {
                newDict = new Dictionary<string, object>();
                nestedParent.Add(key, newDict);

                // Set the value for the new key
                if (value != null && value != default)
                {
                    newDict[key] = value;
                }


                return true;
            }
            else
            {
                // If the key exists, update the value if provided
                if (value != null && nestedParent[key] is string currentValue && currentValue != value)
                    nestedParent[key] = value;

                newDict = nestedParent[key] as Dictionary<string, object>;
                return true;
            }
        }



        private Dictionary<string, object> GetOrAddRoot(string rootKey)
        {
            Dictionary<string, object> childList = new();

            if (base.TryGetValue(rootKey, out object foundRoot))
            {
                return foundRoot as Dictionary<string, object>;
            }
            else
            {
                childList.Add(rootKey, childList);
                base.Add(rootKey, childList);
                return base[rootKey] as Dictionary<string, object>;
            }
        }

        private bool AddToNestedDict(Dictionary<string, object> nestedDict, string key, object value)
        {
            if (nestedDict == null)
            {
                Debug.LogError($"nestedDict is null - {key} : {value}");
                return false;
            }

            if (key == null)
            {
                Debug.LogError($"key is null - {key} : {value}");
                return false;
            }

            if (!nestedDict.ContainsKey(key))
            {
                nestedDict.Add(key, value);
                return true;
            }
            else
            {
                Debug.Log($"nestedDict already contains key - {key} : {value}");
                return false;
            }
        }
        private bool UpdateNestedDict(Dictionary<string, object> nestedDict, string key, object value)
        {
            if (nestedDict == null || key == null) return false;

            if (nestedDict.ContainsKey(key))
            {
                nestedDict[key] = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetValue(string key)
        {
            if (flatData.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }
        public new void Clear()
        {
            base.Clear();
            flatData.Clear();
        }

        public int EntryCount()
        {
            return flatData.Count;
        }
        public int RootCount()
        {
            return base.Count;
        }

        // public new int Count { 
        //     get{ return EntryCount(); } 
        // }
        public SortedList<string, string> GetFlatData()
        {
            return flatData;
        }
        public bool ContainsRoot(string key)
        {
            return flatData.ContainsKey(key);
        }


        // 
        // public override IEnumerable<object> KeysWithDuplicateValues { get; }

        // public override bool ContainsKey(object key)
        // {
        //     if (key == null || !(key is string))
        //     {
        //         return false;
        //     }

        //     return ContainsKey((string)key);
        // }

        // protected override object GestringFromIndexInternal(int index)
        // {
        //     return Keys[index];
        // }


        // protected override object GestringFromIndexInternal(int index)
        // {
        //     return Values[index];
        // }


    }
}

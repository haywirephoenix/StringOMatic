using System;
using System.Linq;
using System.Text;
using static SOM.LanguageConsts;

namespace SOM
{
    public class SOMPathData
    {
        public string Path { get; set; }
        public string ConstName { get; set; }
        public object ConstValue { get; set; }


        public string[] PathSplit { get; set; }
        public string[] PathSplitWithoutRoot { get; set; }
        public string RootModuleName { get; set; }
        public string LastModuleName { get; set; }
        public string PathWithoutRoot { get; set; }


        public SOMPathData(string path, string constName, object constValue = null)
        {
            Path = path;
            ConstName = constName;
            ConstValue = constValue;
        }

    }

    public class SOMPathUtil
    {
        public static string GetConstantClassPath(string constFullPath)
        {
            int lastDotIndex = constFullPath.LastIndexOf(_dotChar);
            if (lastDotIndex != -1)
            {
                return constFullPath.Substring(0, lastDotIndex);
            }
            else
            {
                return constFullPath;
            }
        }
        public static string MakeConstantKeyPath(string constPath, string constValue)
        {
            return string.Join(_dotChar, constPath, constValue);
        }
        public static string MakeConstantKeyPath(SOMPathData sompathData)
        {
            return string.Join(_dotChar, sompathData.Path, sompathData.ConstName);
        }
        public static string MakeConstantFullPath(string constPath, string constName, string constValue)
        {
            return string.Join(_dotChar, constPath, constName, constValue);
        }
        public static string MakeConstantFullPath(SOMPathData sompathData)
        {
            return string.Join(_dotChar, sompathData.Path, sompathData.ConstName, sompathData.ConstValue);
        }
        public static string NicifyPath(string path)
        {
            string[] patharr = path.Split(_dotChar);
            string[] newPatharr = new string[patharr.Length];

            for (int i = 0; i < patharr.Length; i++)
            {
                newPatharr[i] = SOMUtils.NicifyModuleName(patharr[i]);
            }

            return string.Join(_dotChar, newPatharr);
        }

        public static SOMPathData SplitModulePath(string modulePath, out string[] modulePathSplit, bool containsConst = false)
        {
            modulePathSplit = null;

            if (string.IsNullOrEmpty(modulePath))
            {
                throw new ArgumentException("Module path cannot be null or empty.", nameof(modulePath));
            }

            modulePathSplit = modulePath.Split(_dotChar);
            string constName = "";
            string constVal = "";

            if (containsConst)
            {
                if (modulePathSplit.Length < 3)
                {
                    throw new ModulePathShortException(modulePath);
                }

                // Remove the last two elements (const name and value) from modulePathSplit
                constName = modulePathSplit[^2]; // get the second to last element
                constVal = modulePathSplit[^1]; // get the last element
                modulePathSplit = modulePathSplit[..^2]; // remove the last two elements
            }

            // Get the root module name (the first element)
            string rootModuleName = modulePathSplit[0];

            // Get the last module name (the new last element)
            string lastModuleName = modulePathSplit[^1];

            // Remove the root module name from the path
            string pathWithoutRoot = string.Join(".", modulePathSplit.Skip(1));

            // Split the path without the root module
            string[] splitPathWithoutRoot = pathWithoutRoot.Split(_dotChar);

            return new SOMPathData(modulePath, constName, constVal)
            {
                RootModuleName = rootModuleName,
                LastModuleName = lastModuleName,
                PathSplit = modulePathSplit,
                PathWithoutRoot = pathWithoutRoot,
                PathSplitWithoutRoot = splitPathWithoutRoot
            };
        }


        private static string ShortenModulePath(string modulePath, int endIndex)
        {
            string[] modulePathSplit = modulePath.Split(_dotChar);
            return ShortenModulePath(modulePathSplit, endIndex);
        }

        private static string ShortenModulePath(string[] modulePathSplit, int endIndex)
        {
            StringBuilder pathBuilder = new();

            for (int i = 0; i <= endIndex; i++)
            {
                if (i > 0)
                {
                    pathBuilder.Append(".");
                }
                pathBuilder.Append(modulePathSplit[i]);
            }

            return pathBuilder.ToString();
        }
        private static string RemoveRootFromPath(string[] parts)
        {

            string result = string.Join(".", parts, 1, parts.Length - 1);
            return result;
        }
        private static string RemoveRootFromPath(string dottedString)
        {
            string[] parts = dottedString.Split('.');
            string result = RemoveRootFromPath(parts);
            return result;
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 文件路径相关帮助类
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 是否为绝对路径
        /// </summary>
        /// <param name="Path">路径</param>
        /// <returns>绝对路径返回True,相对路径返回False</returns>
        public static bool IsAbsolutePath(string Path)
        {
            return Path.Contains(":\\") || Path.Contains(":/");
        }

        /// <summary>
        /// 绝对路径转相对路径
        /// </summary>
        /// <param name="AbsolutePath">绝对路径</param>
        /// <param name="CurrentPath">当前路径</param>
        /// <returns>相对路径</returns>
        public static string AbsoluteToRelative(string AbsolutePath, string CurrentPath = null)
        {
            if (!IsAbsolutePath(AbsolutePath))
            {
                // 如果检测是相对路径，直接返回
                return AbsolutePath;
            }
            if (string.IsNullOrEmpty(CurrentPath))
            {
                CurrentPath = Environment.CurrentDirectory;
            }
            string[] absoluteDirectories = CurrentPath.Split('\\');
            string[] relativeDirectories = AbsolutePath.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
            {
                throw new ArgumentException($"{Application.Current.FindResource("NoPublicPath")}");
            }

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

        /// <summary>
        /// 相对路径转绝对路径
        /// </summary>
        /// <param name="RelativePath">相对路径</param>
        /// <param name="CurrentPath">当前路径</param>
        /// <returns>绝对路径</returns>
        public static string RelativeToAbsolute(string RelativePath, string CurrentPath = null)
        {
            if (IsAbsolutePath(RelativePath))
            {
                // 如果检测是绝对路径，直接返回
                return RelativePath;
            }
            return string.IsNullOrEmpty(CurrentPath) ? Path.GetFullPath(RelativePath) : Path.GetFullPath(RelativePath, CurrentPath);
        }
    }
}

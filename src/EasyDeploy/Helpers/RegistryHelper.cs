using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 注册表帮助类
    /// 创建日期:2017年6月22日
    /// </summary>
    public class RegistryHelper
    {
        #region Registry Startup Items
        /// <summary>
        /// 创建注册表启动项
        /// </summary>
        /// <param name="strName">键值名称</param>
        /// <param name="strSoftwarePath">启动项软件路径</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool CreateStartupItems(string strName, string strSoftwarePath)
        {
            try
            {
                if (string.IsNullOrEmpty(strName) || string.IsNullOrEmpty(strSoftwarePath))
                {
                    return false;
                }
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey == null)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                }
                registryKey.SetValue(strName, strSoftwarePath);
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除注册表启动项
        /// </summary>
        /// <param name="strName">键值名称</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool DeleteStartupItems(string strName)
        {
            try
            {
                if (string.IsNullOrEmpty(strName))
                {
                    return false;
                }
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey == null)
                {
                    return false;
                }
                registryKey.DeleteValue(strName, false);
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获得注册表中所有启动项
        /// </summary>
        /// <returns>注册表中启动项(键值,启动路径)</returns>
        public static Dictionary<string, string> GetAllStartupItems()
        {
            try
            {
                Dictionary<string, string> dicAllStartupItems = new Dictionary<string, string>();
                RegistryKey registryKey = null;
                //获取HKEY_CURRENT_USER中的启动项
                registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey != null)
                {
                    foreach (string strValeName in registryKey.GetValueNames())
                    {
                        if (!dicAllStartupItems.ContainsKey(strValeName))
                        {
                            dicAllStartupItems.Add(strValeName, registryKey.GetValue(strValeName).ToString());
                        }
                    }
                }
                //获取HKEY_LOCAL_MACHINE中的启动项
                registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey != null)
                {
                    foreach (string strValeName in registryKey.GetValueNames())
                    {
                        if (!dicAllStartupItems.ContainsKey(strValeName))
                        {
                            dicAllStartupItems.Add(strValeName, registryKey.GetValue(strValeName).ToString());
                        }
                    }
                }
                return dicAllStartupItems;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return null;
            }
        }
        #endregion

        #region Registry Default Icon
        /// <summary>
        /// 修改特定后缀文件默认图标(需重启电脑)
        /// </summary>
        /// <param name="strFileType">特定文件类型(例:.txt|.exe)</param>
        /// <param name="strIcoPath">替换图片路径</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool ModifyDefaultIcon(string strFileType, string strIcoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(strFileType) || string.IsNullOrEmpty(strIcoPath) || !File.Exists(strIcoPath))
                {
                    return false;
                }
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(strFileType, true);
                if (registryKey == null)
                {
                    registryKey = Registry.ClassesRoot.CreateSubKey(strFileType);
                }
                //获取(默认)中的数据
                string strDefault = registryKey.ValueCount >= 1 ? registryKey.GetValue("").ToString() : string.Empty;
                if (string.IsNullOrEmpty(strDefault))
                {
                    //如果该后缀名里(默认)没有值,则创建shell写入菜单功能
                    registryKey = Registry.ClassesRoot.OpenSubKey(strFileType + @"\DefaultIcon\", true);
                    if (registryKey == null)
                    {
                        registryKey = Registry.ClassesRoot.CreateSubKey(strFileType + @"\DefaultIcon\");
                    }
                    if (!string.IsNullOrEmpty(strIcoPath))
                    {
                        registryKey.SetValue("", strIcoPath);
                    }
                }
                else
                {
                    //如果该后缀名里(默认)存在值,读取值所在的路径创建shell写入菜单功能
                    registryKey = Registry.ClassesRoot.OpenSubKey(strDefault + @"\DefaultIcon\", true);
                    if (registryKey == null)
                    {
                        registryKey = Registry.ClassesRoot.CreateSubKey(strDefault + @"\DefaultIcon\");
                    }
                    if (!string.IsNullOrEmpty(strIcoPath))
                    {
                        registryKey.SetValue("", strIcoPath);
                    }
                }
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }
        #endregion

        #region Registry Default Programs
        /// <summary>
        /// 修改特定后缀文件默认程序
        /// </summary>
        /// <param name="strFileType">特定文件类型(例:.txt|.exe)</param>
        /// <param name="strSoftwarePath">替换程序路径</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool ModifyDefaultPrograms(string strFileType, string strSoftwarePath)
        {
            try
            {
                if (string.IsNullOrEmpty(strFileType) || string.IsNullOrEmpty(strSoftwarePath))
                {
                    return false;
                }
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(strFileType, true);
                if (registryKey == null)
                {
                    registryKey = Registry.ClassesRoot.CreateSubKey(strFileType);
                }
                //获取(默认)中的数据
                string strDefault = registryKey.ValueCount >= 1 ? registryKey.GetValue("").ToString() : string.Empty;
                if (string.IsNullOrEmpty(strDefault))
                {
                    //如果该后缀名里(默认)没有值,则创建shell写入菜单功能
                    registryKey = Registry.ClassesRoot.OpenSubKey(strFileType + @"\shell\open\command\", true);
                    if (registryKey == null)
                    {
                        registryKey = Registry.ClassesRoot.CreateSubKey(strFileType + @"\shell\open\command\");
                    }
                    if (!string.IsNullOrEmpty(strSoftwarePath))
                    {
                        registryKey.SetValue("", strSoftwarePath);
                    }
                }
                else
                {
                    //如果该后缀名里(默认)存在值,读取值所在的路径创建shell写入菜单功能
                    registryKey = Registry.ClassesRoot.OpenSubKey(strDefault + @"\shell\open\command\", true);
                    if (registryKey == null)
                    {
                        registryKey = Registry.ClassesRoot.CreateSubKey(strDefault + @"\shell\open\command\");
                    }
                    if (!string.IsNullOrEmpty(strSoftwarePath))
                    {
                        registryKey.SetValue("", strSoftwarePath);
                    }
                }
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }
        #endregion

        #region Registry URL Protocol
        /// <summary>
        /// 创建 URL Protocol 协议,通过网页打开本地应用
        /// </summary>
        /// <param name="strName">键值名称</param>
        /// <param name="strSoftwarePath">启动软件路径</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool CreateURLProtocol(string strName, string strSoftwarePath)
        {
            try
            {
                if (string.IsNullOrEmpty(strName) || string.IsNullOrEmpty(strSoftwarePath))
                {
                    return false;
                }
                //Web端调用方法:<a href="strName://"%1"参数>URL Protocol</a>
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(strName + @"\shell\open\command", true);
                if (registryKey == null)
                {
                    registryKey = Registry.ClassesRoot.CreateSubKey(strName + @"\shell\open\command");
                }
                registryKey.SetValue("", strSoftwarePath);
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除 URL Protocol 协议
        /// </summary>
        /// <param name="strName">键值名称</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool DeleteURLProtocol(string strName)
        {
            try
            {
                if (string.IsNullOrEmpty(strName))
                {
                    return false;
                }
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(strName, true);
                if (registryKey == null)
                {
                    return false;
                }
                else
                {
                    registryKey = Registry.ClassesRoot;
                    registryKey.DeleteSubKeyTree(strName);
                }
                registryKey.Close();
                return true;
            }
            catch (Exception ex)
            {
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }
        #endregion
    }
}

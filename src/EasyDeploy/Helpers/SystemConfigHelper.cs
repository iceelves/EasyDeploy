using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 系统配置文件帮助类
    /// </summary>
    public class SystemConfigHelper
    {
        /// <summary>
        /// 系统配置文件路径
        /// </summary>
        public static string SystemConfigPath
        {
            get
            {
                string strPath = $"{AppDomain.CurrentDomain.BaseDirectory}SystemConfig.ini";
                //文件夹不存在时自动创建
                string strFolderPath = Path.GetDirectoryName(strPath);
                if (!Directory.Exists(strFolderPath))
                {
                    NLogHelper.SaveDebug("系统配置文件夹不存在，自动创建！");
                    Directory.CreateDirectory(strFolderPath);
                }
                //文件不存在时自动创建
                if (!File.Exists(strPath))
                {
                    NLogHelper.SaveDebug($"系统配置文件({Path.GetFileName(strPath)})不存在，自动创建！");
                    //创建默认配置信息
                    INIHelper.INIWriteValue(strPath, "System", "StartWithWindows", "false");
                    INIHelper.INIWriteValue(strPath, "System", "Language", "en-US");
                }
                return strPath;
            }
        }

        /// <summary>
        /// 获取指定节点值
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>节点值</returns>
        public static string GetSystemConfigInfo(string section, string key)
        {
            try
            {
                if (File.Exists(SystemConfigPath))
                {
                    return INIHelper.INIGetStringValue(SystemConfigPath, section, key, null);
                }
                else
                {
                    NLogHelper.SaveDebug("获取配置文件 SystemConfig 失败！");
                    return null;
                }
            }
            catch (Exception ex)
            {
                NLogHelper.SaveDebug("获取配置文件 LoginConfig 异常！");
                NLogHelper.SaveError(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 记录系统配置文件
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>保存成功返回True</returns>
        public static bool SetSystemConfigInfo(string section, string key, string value)
        {
            try
            {
                return INIHelper.INIWriteValue(SystemConfigPath, section, key, value);
            }
            catch (Exception ex)
            {
                NLogHelper.SaveDebug("记录配置文件 SystemConfig 失败！");
                NLogHelper.SaveError(ex.ToString());
                return false;
            }
        }
    }
}

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
        /// 系统标识节点
        /// </summary>
        public const string SECTION_SYSTEM = "System";

        /// <summary>
        /// 自动启动
        /// </summary>
        public const string SYSTEM_START_WITH_WINDOWS = "StartWithWindows";

        /// <summary>
        /// 语言
        /// </summary>
        public const string SYSTEM_LANGUAGE = "Language";

        /// <summary>
        /// 终端节点标识
        /// </summary>
        public const string SECTION_TERMINAL = "Terminal";

        /// <summary>
        /// 终端默认背景颜色
        /// </summary>
        public const string TERMINAL_BACKGROUND = "Background";

        /// <summary>
        /// 终端默认文字颜色
        /// </summary>
        public const string TERMINAL_FOREGROUND = "Foreground";

        /// <summary>
        /// 终端默认文字大小
        /// </summary>
        public const string TERMINAL_FONTSIZE = "FontSize";

        /// <summary>
        /// 系统配置文件路径
        /// </summary>
        public static string SystemConfigPath
        {
            get
            {
                string strPath = $"{AppDomain.CurrentDomain.BaseDirectory}SystemConfig.ini";
                // 文件夹不存在时自动创建
                string strFolderPath = Path.GetDirectoryName(strPath);
                if (!Directory.Exists(strFolderPath))
                {
                    NLogHelper.SaveDebug("系统配置文件夹不存在，自动创建！");
                    Directory.CreateDirectory(strFolderPath);
                }
                // 文件不存在时自动创建
                if (!File.Exists(strPath))
                {
                    NLogHelper.SaveDebug($"系统配置文件({Path.GetFileName(strPath)})不存在，自动创建！");
                    // 创建默认系统配置信息
                    INIHelper.INIWriteValue(strPath, SECTION_SYSTEM, SYSTEM_START_WITH_WINDOWS, "false");
                    INIHelper.INIWriteValue(strPath, SECTION_SYSTEM, "Language", "en-US");
                    // 创建默认终端配置信息
                    INIHelper.INIWriteValue(strPath, SECTION_TERMINAL, TERMINAL_BACKGROUND, "#0C0C0C");
                    INIHelper.INIWriteValue(strPath, SECTION_TERMINAL, TERMINAL_FOREGROUND, "#FFFFFF");
                    INIHelper.INIWriteValue(strPath, SECTION_TERMINAL, TERMINAL_FONTSIZE, "14");
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

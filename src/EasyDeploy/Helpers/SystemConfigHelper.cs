using EasyDeploy.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

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
        /// 最大行数
        /// </summary>
        public const string TERMINAL_MAXROWS = "MaxRows";

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

        private static List<LanguageModel> _listLanguage;
        /// <summary>
        /// 语言资源集合
        /// </summary>
        public static List<LanguageModel> ListLanguage
        {
            get
            {
                if (_listLanguage == null)
                {
                    _listLanguage = new List<LanguageModel>();
                    _listLanguage.Add(new LanguageModel() { FileName = "en-US", Language = "English", Resource = new ResourceDictionary() { Source = new Uri("/EasyDeploy;component/Language/en-US.xaml", UriKind.RelativeOrAbsolute) } });
                    _listLanguage.Add(new LanguageModel() { FileName = "zh-CN", Language = "简体中文", Resource = new ResourceDictionary() { Source = new Uri("/EasyDeploy;component/Language/zh-CN.xaml", UriKind.RelativeOrAbsolute) } });
                }
                return _listLanguage;
            }
            set
            {
                _listLanguage = value;
            }
        }

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
                    // 获取系统语言，默认如果是中文加载中文，其余加载英文
                    var vLanguage = CultureInfo.InstalledUICulture.Name.Equals("zh-CN") ? "zh-CN" : "en-US";
                    INIHelper.INIWriteValue(strPath, SECTION_SYSTEM, "Language", vLanguage);
                    // 创建默认终端配置信息
                    INIHelper.INIWriteValue(strPath, SECTION_TERMINAL, TERMINAL_MAXROWS, "5000");
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

        /// <summary>
        /// 获取当前配置文件是否为中文
        /// </summary>
        /// <returns></returns>
        public static bool IsChinese()
        {
            var vSystemConfigInfo_Language = GetSystemConfigInfo(SECTION_SYSTEM, SYSTEM_LANGUAGE);
            return !string.IsNullOrEmpty(vSystemConfigInfo_Language) && vSystemConfigInfo_Language.Equals("zh-CN");
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        /// <param name="language"></param>
        public static void SetLanguage(string language = null)
        {
            // 把要修改的语言放置资源最后
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            if (string.IsNullOrEmpty(language))
            {
                var vSystemConfigInfo_Language = GetSystemConfigInfo(SECTION_SYSTEM, SYSTEM_LANGUAGE);
                foreach (var item in ListLanguage)
                {
                    if (item.FileName.Equals(vSystemConfigInfo_Language))
                    {
                        language = item.Resource.Source.OriginalString;
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(language))
            {
                var resourceDictionary = dictionaryList.FirstOrDefault(o => o.Source.OriginalString.Equals(language));
                if (resourceDictionary != null)
                {
                    Application.Current.Resources.BeginInit();
                    Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                    Application.Current.Resources.EndInit();
                }
            }
        }
    }
}

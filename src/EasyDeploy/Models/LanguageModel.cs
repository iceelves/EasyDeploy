using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace EasyDeploy.Models
{
    /// <summary>
    /// 语言数据模型
    /// </summary>
    public class LanguageModel
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 语言名称
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 资源文件
        /// </summary>
        public ResourceDictionary Resource { get; set; }
    }
}

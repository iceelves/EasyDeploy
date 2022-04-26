using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDeploy.Models
{
    /// <summary>
    /// 服务资源
    /// 程序运行时生成的资源（控制台、Shell）
    /// </summary>
    public class ServiceResourcesModel
    {
        /// <summary>
        /// 控制台
        /// </summary>
        public CliWrapHelper CliWrap { get; set; }
    }
}

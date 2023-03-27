using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace EasyDeploy.Models
{
    /// <summary>
    /// 服务配置模型
    /// 用于保存到 Json
    /// </summary>
    public class ServiceModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务路径
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// 程序启动时是否自动启动
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        /// 运行异常时是否自动重启
        /// </summary>
        public bool AutoReStart { get; set; }

        /// <summary>
        /// 服务状态
        /// </summary>
        [JsonIgnore]
        public ServiceState ServiceState { get; set; }

        /// <summary>
        /// 进程 PID
        /// </summary>
        [JsonIgnore]
        public string Pid { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        [JsonIgnore]
        public string Port { get; set; }

        /// <summary>
        /// 点击运行时随机生成的主键
        /// 用于关联控制台资源
        /// </summary>
        [JsonIgnore]
        public string Guid { get; set; }

        /// <summary>
        /// 是否允许打开文件目录
        /// </summary>
        [JsonIgnore]
        public bool AllowDirectoryToOpen
        {
            get
            {
                var vDirectory = Path.GetDirectoryName(ServicePath);
                return !string.IsNullOrEmpty(vDirectory) && Directory.Exists(vDirectory);
            }
        }
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public enum ServiceState
    {
        [Description("Service Default or stop")]
        None = 0,
        [Description("Service Start")]
        Start = 1,
        [Description("Service Error")]
        Error = 2,
        [Description("Service Wait")]
        Wait = 3
    }
}

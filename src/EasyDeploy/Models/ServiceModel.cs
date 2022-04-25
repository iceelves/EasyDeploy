using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public int Pid { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        [JsonIgnore]
        public string Port { get; set; }
    }

    /// <summary>
    /// 服务状态
    /// </summary>
    public enum ServiceState
    {
        None = 0,
        Start = 1,
        Error = 2
    }
}

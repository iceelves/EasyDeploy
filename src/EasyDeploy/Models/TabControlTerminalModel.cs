using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDeploy.Models
{
    /// <summary>
    /// 分页控件终端模型
    /// </summary>
    public class TabControlTerminalModel
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Terminal Control
        /// </summary>
        public object Control { get; set; }
    }
}

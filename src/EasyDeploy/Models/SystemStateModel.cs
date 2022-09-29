using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyDeploy.Models
{
    public class SystemStateModel
    {
        /// <summary>
        /// 进程 PID
        /// </summary>
        public int PID { get; set; }

        /// <summary>
        /// CPU 占用率
        /// </summary>
        public float CpuChang { get; set; }

        /// <summary>
        /// 内存占用率
        /// </summary>
        public float RamChang { get; set; }
    }
}

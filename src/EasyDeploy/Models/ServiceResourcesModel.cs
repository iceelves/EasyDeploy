using EasyDeploy.Controls;
using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

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

        /// <summary>
        /// 终端 Shell
        /// </summary>
        public IceRichTextBox Terminal { get; set; }

        /// <summary>
        /// 监听 Shell
        /// </summary>
        public void MonitorShell()
        {
            if (CliWrap != null && Terminal != null)
            {
                CliWrap.StartedCommandEvent += CliWrap_StartedCommandEvent;
                CliWrap.StandardOutputCommandEvent += CliWrap_StandardOutputCommandEvent;
                CliWrap.StandardErrorCommandEvent += CliWrap_StandardErrorCommandEvent;
                CliWrap.ExitedCommandEvent += CliWrap_ExitedCommandEvent;
            }
        }

        /// <summary>
        /// 启动命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StartedCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }

        /// <summary>
        /// 标准输出命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StandardOutputCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }

        /// <summary>
        /// 标准错误命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_StandardErrorCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }

        /// <summary>
        /// 退出命令事件
        /// </summary>
        /// <param name="obj"></param>
        private void CliWrap_ExitedCommandEvent(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Terminal?.SetText(obj);
            });
        }
    }
}

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// NLog 帮助类
    /// </summary>
    public static class NLogHelper
    {
        private readonly static ILogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 保存调试日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="memberName">堆栈信息</param>
        /// <param name="sourceFilePath">堆栈信息</param>
        /// <param name="sourceLineNumber">堆栈信息</param>
        public static void SaveDebug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.Debug($"[{Path.GetFileName(sourceFilePath)}][{memberName}] {message}");
            ConsileWriteLog(ConsoleColor.Yellow, "Debug", message);
        }

        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="memberName">堆栈信息</param>
        /// <param name="sourceFilePath">堆栈信息</param>
        /// <param name="sourceLineNumber">堆栈信息</param>
        public static void SaveInfo(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.Info($"[{Path.GetFileName(sourceFilePath)}][{memberName}] {message}");
            ConsileWriteLog(ConsoleColor.Green, "Info", message);
        }

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="memberName">堆栈信息</param>
        /// <param name="sourceFilePath">堆栈信息</param>
        /// <param name="sourceLineNumber">堆栈信息</param>
        public static void SaveError(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.Error($"[{Path.GetFileName(sourceFilePath)}][{memberName}] {message}");
            ConsileWriteLog(ConsoleColor.Red, "Error", message);
        }

        /// <summary>
        /// 保存痕迹日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="memberName">堆栈信息</param>
        /// <param name="sourceFilePath">堆栈信息</param>
        /// <param name="sourceLineNumber">堆栈信息</param>
        public static void SaveTrace(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.Trace($"[{Path.GetFileName(sourceFilePath)}][{memberName}] {message}");
            ConsileWriteLog(ConsoleColor.DarkGray, "Trace", message);
        }

        /// <summary>
        /// 控制台打印日志
        /// </summary>
        /// <param name="consoleColor">颜色</param>
        /// <param name="type">类型</param>
        /// <param name="message">消息</param>
        public static void ConsileWriteLog(ConsoleColor consoleColor, string type, string message)
        {
            lock (Console.Out)
            {
                Console.Write($"{DateTime.Now}");
                Console.ForegroundColor = consoleColor;
                Console.Write($" {type} ");
                Console.ResetColor();
                Console.Write($"{message}");
                Console.Write($"\r\n");
            }
        }
    }
}

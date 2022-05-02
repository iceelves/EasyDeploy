using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 操作Windows需要的帮助类
    /// </summary>
    public class WindowsHelper
    {
        /// <summary>
        /// 获取当前软件运行权限
        /// </summary>
        /// <returns></returns>
        public static string GetRunPermissions()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            foreach (WindowsBuiltInRole item in Enum.GetValues(typeof(WindowsBuiltInRole)))
            {
                if (windowsPrincipal.IsInRole(item))
                {
                    return item.ToString();
                }
            }
            return "unknown";
        }

        /// <summary>
        /// 当前运行是否管理员权限
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}

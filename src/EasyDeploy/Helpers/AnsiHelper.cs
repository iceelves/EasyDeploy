using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// ANSI 转义序列相关帮助类
    /// https://en.wikipedia.org/wiki/ANSI_escape_code
    /// </summary>
    public static class AnsiHelper
    {
        /// <summary>
        /// 匹配 Ansi 头
        /// </summary>
        private static char AnsiStart = '\u001b';

        /// <summary>
        /// 匹配 Ansi 正则
        /// </summary>
        private static string AnsiRegex = @"\u001b\[\d+m";

        /// <summary>
        /// 去除字符串中的 ANSI 序列
        /// </summary>
        /// <param name="text">包含 Ansi 的文本</param>
        /// <returns></returns>
        public static string RemoveAnsi(string text)
        {
            return Regex.Replace(text, AnsiRegex, string.Empty);
        }

        /// <summary>
        /// 获取通过正则 Ansi 拆分的数据集
        /// </summary>
        /// <param name="text">包含 Ansi 的文本</param>
        /// <returns></returns>
        public static List<string> GetAnsiSplit(string text)
        {
            List<string> listAnsiSplit = new List<string>();
            // 获取匹配 Ansi 数据
            var vMatches = Regex.Matches(text, AnsiRegex);
            // 获取 Ansi 下标
            List<int> indexs = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].Equals(AnsiStart))
                {
                    indexs.Add(i);
                }
            }
            // 遍历拆分数据
            int iSubscript = 0;
            for (int i = 0; i < indexs.Count; i++)
            {
                if (i == 0 && indexs[i] > 0)
                {
                    // 如果大于起始位置，先把起始数据赋值
                    listAnsiSplit.Add(text.Substring(0, indexs[i]));
                    iSubscript += indexs[i];
                }
                // 添加 Ansi 数据
                listAnsiSplit.Add(vMatches[i].Value);
                iSubscript += vMatches[i].Value.Length;

                // 添加其他数据
                int iSubCount = (indexs.Count > i + 1 ? indexs[i + 1] : text.Length - 1) - iSubscript;
                if (iSubCount > 0)
                {
                    listAnsiSplit.Add(text.Substring(iSubscript, iSubCount));
                    iSubscript += iSubCount;
                }
            }
            return listAnsiSplit;
        }
    }
}

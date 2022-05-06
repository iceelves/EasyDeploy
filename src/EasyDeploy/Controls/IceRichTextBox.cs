using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceRichTextBox : RichTextBox
    {
        static IceRichTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceRichTextBox), new FrameworkPropertyMetadata(typeof(IceRichTextBox)));

        /// <summary>
        /// 最大行数
        /// 先删除后打印，所以实际会比限制多一行
        /// </summary>
        public int MaxRows
        {
            get { return (int)GetValue(MaxRowsProperty); }
            set { SetValue(MaxRowsProperty, value); }
        }
        public static readonly DependencyProperty MaxRowsProperty =
            DependencyProperty.Register("MaxRows", typeof(int), typeof(IceRichTextBox), new PropertyMetadata(default(int)));

        /// <summary>
        /// 清空文本
        /// 第一次写入时需执行
        /// </summary>
        public void ClearText()
        {
            this.Document.Blocks.Clear();
        }

        /// <summary>
        /// 添加文本
        /// </summary>
        /// <param name="Text"></param>
        public void SetText(string Text)
        {
            Text += $" {this.Document.Blocks.Count}";
            // 根据最大显示行数删除
            int iRempveNumber = this.Document.Blocks.Count - MaxRows;
            if (iRempveNumber >= 1)
            {
                for (int i = 0; i < iRempveNumber; i++)
                {
                    Paragraph vRemoveTemp = null;
                    foreach (var item in this.Document.Blocks)
                    {
                        vRemoveTemp = item as Paragraph;
                        break;
                    }
                    this.Document.Blocks.Remove(vRemoveTemp);
                }
            }

            // 添加文本
            string ansiColor = null;
            Paragraph paragraph = new Paragraph();
            foreach (var item in AnsiHelper.GetAnsiSplit(Text))
            {
                if (item.Contains(AnsiHelper.AnsiStart))
                {
                    // 设置颜色
                    ansiColor = item;
                }
                else
                {
                    paragraph.Inlines.Add(SetColorFromAnsi(new Run() { Text = item }, ansiColor));
                }
            }
            this.Document.Blocks.Add(paragraph);

            // 滚动条超过 80% 或滚动条小于一倍控件高度 滚动到底部
            if (this.VerticalOffset / (this.ExtentHeight - this.ActualHeight) >= 0.8 || (this.ExtentHeight - this.ActualHeight) <= this.ActualHeight)
            {
                this.ScrollToEnd();
            }
        }

        /// <summary>
        /// 根据 Ansi 设置文本颜色
        /// </summary>
        /// <param name="run">文本</param>
        /// <param name="ansiColor">ansi 颜色</param>
        /// <returns></returns>
        private Run SetColorFromAnsi(Run run, string ansiColor)
        {
            if (string.IsNullOrEmpty(ansiColor))
            {
                return run;
            }
            var vMatches = Regex.Matches(ansiColor, AnsiHelper.AnsiRegex);
            if (vMatches != null && vMatches.Count >= 1 && vMatches[0].Groups != null && vMatches[0].Groups.Count >= 2)
            {
                var vSplit = vMatches[0].Groups[1].Value.Split(';');
                foreach (var item in vSplit)
                {
                    switch (item)
                    {
                        // Black
                        case "30": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)); break;
                        case "40": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)); break;

                        // Red
                        case "31": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 0, 0)); break;
                        case "41": run.Background = new SolidColorBrush(Color.FromRgb(128, 0, 0)); break;

                        // Green
                        case "32": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0)); break;
                        case "42": run.Background = new SolidColorBrush(Color.FromRgb(0, 128, 0)); break;

                        // Yellow
                        case "33": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 0)); break;
                        case "43": run.Background = new SolidColorBrush(Color.FromRgb(128, 128, 0)); break;

                        // Blue
                        case "34": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 128)); break;
                        case "44": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 128)); break;

                        // Magenta
                        case "35": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 0, 128)); break;
                        case "45": run.Background = new SolidColorBrush(Color.FromRgb(128, 0, 128)); break;

                        // Cyan
                        case "36": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)); break;
                        case "46": run.Background = new SolidColorBrush(Color.FromRgb(0, 128, 128)); break;

                        // White
                        case "37": run.Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192)); break;
                        case "47": run.Background = new SolidColorBrush(Color.FromRgb(192, 192, 192)); break;

                        // Bright Black (Gray)
                        case "90": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)); break;
                        case "100": run.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128)); break;

                        // Bright Red
                        case "91": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)); break;
                        case "101": run.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0)); break;

                        // Bright Green
                        case "92": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)); break;
                        case "102": run.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); break;

                        // Bright Yellow
                        case "93": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0)); break;
                        case "103": run.Background = new SolidColorBrush(Color.FromRgb(255, 255, 0)); break;

                        // Bright Blue
                        case "94": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255)); break;
                        case "104": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255)); break;

                        // Bright Magenta
                        case "95": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 255)); break;
                        case "105": run.Background = new SolidColorBrush(Color.FromRgb(255, 0, 255)); break;

                        // Bright Cyan
                        case "96": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255)); break;
                        case "106": run.Background = new SolidColorBrush(Color.FromRgb(0, 255, 255)); break;

                        // Bright White
                        case "97": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)); break;
                        case "107": run.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); break;
                        default:
                            break;
                    }
                }
            }
            return run;
        }
    }
}

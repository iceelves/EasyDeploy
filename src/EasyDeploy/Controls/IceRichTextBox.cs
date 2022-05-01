using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
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
            // 根据最大显示行数删除
            if (this.Document.Blocks.Count >= MaxRows)
            {
                int iRempveNumber = this.Document.Blocks.Count - MaxRows;
                List<Paragraph> listRemoveTemp = new List<Paragraph>();
                foreach (var item in this.Document.Blocks)
                {
                    if (iRempveNumber > 0)
                    {
                        iRempveNumber--;
                        listRemoveTemp.Add(item as Paragraph);
                    }
                }
                if (listRemoveTemp != null && listRemoveTemp.Count >= 1)
                {
                    foreach (var item in listRemoveTemp)
                    {
                        this.Document.Blocks.Remove(item);
                    }
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
            switch (ansiColor)
            {
                // Black
                case "\u001b[30m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)); break;
                case "\u001b[40m": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)); break;

                // Red
                case "\u001b[31m": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 0, 0)); break;
                case "\u001b[41m": run.Background = new SolidColorBrush(Color.FromRgb(128, 0, 0)); break;

                // Green
                case "\u001b[32m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0)); break;
                case "\u001b[42m": run.Background = new SolidColorBrush(Color.FromRgb(0, 128, 0)); break;

                // Yellow
                case "\u001b[33m": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 0)); break;
                case "\u001b[43m": run.Background = new SolidColorBrush(Color.FromRgb(128, 128, 0)); break;

                // Blue
                case "\u001b[34m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 128)); break;
                case "\u001b[44m": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 128)); break;

                // Magenta
                case "\u001b[35m": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 0, 128)); break;
                case "\u001b[45m": run.Background = new SolidColorBrush(Color.FromRgb(128, 0, 128)); break;

                // Cyan
                case "\u001b[36m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 128)); break;
                case "\u001b[46m": run.Background = new SolidColorBrush(Color.FromRgb(0, 128, 128)); break;

                // White
                case "\u001b[37m": run.Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192)); break;
                case "\u001b[47m": run.Background = new SolidColorBrush(Color.FromRgb(192, 192, 192)); break;

                // Bright Black (Gray)
                case "\u001b[90m": run.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)); break;
                case "\u001b[100m": run.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128)); break;

                // Bright Red
                case "\u001b[91m": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)); break;
                case "\u001b[101m": run.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0)); break;

                // Bright Green
                case "\u001b[92m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0)); break;
                case "\u001b[102m": run.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); break;

                // Bright Yellow
                case "\u001b[93m": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0)); break;
                case "\u001b[103m": run.Background = new SolidColorBrush(Color.FromRgb(255, 255, 0)); break;

                // Bright Blue
                case "\u001b[94m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 255)); break;
                case "\u001b[104m": run.Background = new SolidColorBrush(Color.FromRgb(0, 0, 255)); break;

                // Bright Magenta
                case "\u001b[95m": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 255)); break;
                case "\u001b[105m": run.Background = new SolidColorBrush(Color.FromRgb(255, 0, 255)); break;

                // Bright Cyan
                case "\u001b[96m": run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255)); break;
                case "\u001b[106m": run.Background = new SolidColorBrush(Color.FromRgb(0, 255, 255)); break;

                // Bright White
                case "\u001b[97m": run.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)); break;
                case "\u001b[107m": run.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); break;
                default:
                    break;
            }
            return run;
        }
    }
}

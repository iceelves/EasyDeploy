using EasyDeploy.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EasyDeploy.Controls
{
    /// <summary>
    /// IceRichTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class IceRichTextBox : UserControl
    {
        /// <summary>
        /// 待写入文本队列（线程安全）
        /// </summary>
        private readonly ConcurrentQueue<string> _pendingLines = new ConcurrentQueue<string>();

        /// <summary>
        /// 批量刷新定时器，每 50ms 将队列内容一次性写入 RichTextBox
        /// </summary>
        private readonly DispatcherTimer _flushTimer;

        public IceRichTextBox()
        {
            InitializeComponent();

            _flushTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _flushTimer.Tick += FlushPendingLines;
            _flushTimer.Start();
        }

        /// <summary>
        /// 将队列中积累的行批量写入 RichTextBox
        /// </summary>
        private void FlushPendingLines(object? sender, EventArgs e)
        {
            if (_pendingLines.IsEmpty) return;

            // 一次最多处理 200 行，避免单帧耗时过长
            int count = 0;
            while (count < 200 && _pendingLines.TryDequeue(out var line))
            {
                AppendLine(line);
                count++;
            }

            // 写完后滚动到底部（layout 已在本帧更新）
            ScrollToEndIfNeeded();
        }

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
        /// 终端背景颜色
        /// </summary>
        public Brush TerminalBackground
        {
            get { return (Brush)GetValue(TerminalBackgroundProperty); }
            set { SetValue(TerminalBackgroundProperty, value); }
        }
        public static readonly DependencyProperty TerminalBackgroundProperty =
            DependencyProperty.Register("TerminalBackground", typeof(Brush), typeof(IceRichTextBox), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 终端文字颜色
        /// </summary>
        public Brush TerminalForeground
        {
            get { return (Brush)GetValue(TerminalForegroundProperty); }
            set { SetValue(TerminalForegroundProperty, value); }
        }
        public static readonly DependencyProperty TerminalForegroundProperty =
            DependencyProperty.Register("TerminalForeground", typeof(Brush), typeof(IceRichTextBox), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 终端字号大小
        /// </summary>
        public int TerminalFontSize
        {
            get { return (int)GetValue(TerminalFontSizeProperty); }
            set { SetValue(TerminalFontSizeProperty, value); }
        }
        public static readonly DependencyProperty TerminalFontSizeProperty =
            DependencyProperty.Register("TerminalFontSize", typeof(int), typeof(IceRichTextBox), new PropertyMetadata(default(int)));

        /// <summary>
        /// 终端绑定数据
        /// </summary>
        public FlowDocument TerminalDocument
        {
            get { return (FlowDocument)GetValue(TerminalDocumentProperty); }
            set { SetValue(TerminalDocumentProperty, value); }
        }
        public static readonly DependencyProperty TerminalDocumentProperty =
            DependencyProperty.Register("TerminalDocument", typeof(FlowDocument), typeof(IceRichTextBox), new PropertyMetadata(default(FlowDocument)));

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            TerminalDocument = rtb.Document;
        }

        /// <summary>
        /// 滚动到最底部
        /// </summary>
        public void ScrollToEnd()
        {
            // 延迟执行，确保 TabControl 切换后 layout 已完成
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                rtb.ScrollToEnd();
            }));
        }

        /// <summary>
        /// 根据滚动位置决定是否滚动到底部
        /// </summary>
        private void ScrollToEndIfNeeded()
        {
            // 已在底部附近（80% 以上）或内容不足一屏时自动跟随
            bool nearBottom = rtb.ExtentHeight <= this.ActualHeight ||
                              rtb.VerticalOffset >= (rtb.ExtentHeight - rtb.ViewportHeight) * 0.8;
            if (nearBottom)
            {
                rtb.ScrollToEnd();
            }
        }

        /// <summary>
        /// 清空文本
        /// 第一次写入时需执行
        /// </summary>
        public void ClearText()
        {
            TerminalDocument.Blocks.Clear();
        }

        /// <summary>
        /// 垃圾回收
        /// </summary>
        public void Collect()
        {
            _flushTimer.Stop();
            // 清空未处理队列
            while (_pendingLines.TryDequeue(out _)) { }
            ClearText();
            GC.Collect();
        }

        /// <summary>
        /// 添加文本（入队，由定时器批量写入）
        /// </summary>
        /// <param name="Text"></param>
        public void SetText(string Text)
        {
            _pendingLines.Enqueue(Text);
        }

        /// <summary>
        /// 实际将一行写入 RichTextBox（仅在 UI 线程的 FlushPendingLines 中调用）
        /// </summary>
        private void AppendLine(string Text)
        {
            // 根据最大显示行数删除旧行
            int iRemoveNumber = TerminalDocument.Blocks.Count - MaxRows;
            if (iRemoveNumber >= 1)
            {
                for (int i = 0; i < iRemoveNumber; i++)
                {
                    var first = TerminalDocument.Blocks.FirstBlock;
                    if (first != null)
                        TerminalDocument.Blocks.Remove(first);
                }
            }

            // 添加文本
            string? ansiColor = null;
            Paragraph paragraph = new Paragraph();
            foreach (var item in AnsiHelper.GetAnsiSplit(Text))
            {
                if (item.Contains(AnsiHelper.AnsiStart))
                {
                    ansiColor = item;
                }
                else
                {
                    paragraph.Inlines.Add(SetColorFromAnsi(new Run() { Text = item }, ansiColor));
                }
            }
            TerminalDocument.Blocks.Add(paragraph);
        }

        /// <summary>
        /// 根据 Ansi 设置文本颜色
        /// </summary>
        /// <param name="run">文本</param>
        /// <param name="ansiColor">ansi 颜色</param>
        /// <returns></returns>
        private Run SetColorFromAnsi(Run run, string? ansiColor)
        {
            if (string.IsNullOrEmpty(ansiColor))
            {
                return run;
            }
            var vMatches = Regex.Matches(ansiColor, AnsiHelper.AnsiRegex);
            if (vMatches != null && vMatches.Any() && vMatches[0].Groups != null && vMatches[0].Groups.Count >= 2)
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

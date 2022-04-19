using EasyDeploy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace EasyDeploy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void VCliWrap_StartedCommandEvent(string obj)
        {
            LogMessagePrinting($"Process started; ID: {obj}");
        }

        private void VCliWrap_StandardOutputCommandEvent(string obj)
        {
            LogMessagePrinting($"{obj}");
        }

        private void VCliWrap_StandardErrorCommandEvent(string obj)
        {
            LogMessagePrinting($"{obj}");
        }

        private void VCliWrap_ExitedCommandEvent(string obj)
        {
            LogMessagePrinting($"Process exited; Code: {obj}");
        }

        /// <summary>
        /// 日志打印
        /// </summary>
        /// <param name="logMessage"></param>
        private void LogMessagePrinting(string logMessage)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Application.Current.Dispatcher));
                SynchronizationContext.Current.Post(pl =>
                {
                    StringBuilder strBuilder = new StringBuilder(Log.Text);
                    strBuilder.AppendLine($"{logMessage}");
                    Log.Text = strBuilder.ToString();
                }, null);
            });
        }

        private CliWrapHelper CliWrap;

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Log.Text = null;
            CliWrap = new CliWrapHelper("", "ping", new[] { "baidu.com", "-t" });
            CliWrap.StartedCommandEvent += VCliWrap_StartedCommandEvent;
            CliWrap.StandardOutputCommandEvent += VCliWrap_StandardOutputCommandEvent;
            CliWrap.StandardErrorCommandEvent += VCliWrap_StandardErrorCommandEvent;
            CliWrap.ExitedCommandEvent += VCliWrap_ExitedCommandEvent;
            CliWrap.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            CliWrap?.Stop();
        }

        /// <summary>
        /// 内容修改变更后滚动到底部
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}

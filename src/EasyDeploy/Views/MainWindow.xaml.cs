using EasyDeploy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyDeploy.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 加载和退出
            this.TitleBar.MouseDown += TitleBar_MouseDown;
            this.TitleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;

            // 主窗体拖动和缩放
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.MouseMove += MainWindow_MouseMove;
        }

        /// <summary>
        /// 拖动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 标题栏双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ClickCount)
            {
                case 2: Maximization_Click(null, null); break;
            }
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 最大化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maximization_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BaseBorder.Margin = new Thickness(10);
                this.WindowState = WindowState.Normal;
                ReSize.Visibility = Visibility.Visible;
                ReSizeBR.Visibility = Visibility.Visible;
                mainMaximization.ButtonImage = Geometry.Parse("M735.786833 62.98859 288.310382 62.98859c-123.567293 0-223.738737 100.164281-223.738737 223.738737l0 447.476451c0 123.555014 100.17042 223.751017 223.738737 223.751017l447.476451 0c123.561154 0 223.738737-100.196003 223.738737-223.751017L959.52557 286.727327C959.52557 163.15287 859.347986 62.98859 735.786833 62.98859zM831.694159 702.260252c0 70.612221-57.238632 127.850853-127.850853 127.850853L320.292793 830.111105c-70.612221 0-127.850853-57.238632-127.850853-127.850853L192.44194 318.708715c0-70.612221 57.238632-127.850853 127.850853-127.850853l383.551536 0c70.612221 0 127.850853 57.238632 127.850853 127.850853L831.695183 702.260252z");
            }
            else
            {
                this.BaseBorder.Margin = new Thickness(0);
                this.MaxHeight = SystemParameters.WorkArea.Height;
                this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                this.WindowState = WindowState.Maximized;
                ReSize.Visibility = Visibility.Collapsed;
                ReSizeBR.Visibility = Visibility.Collapsed;
                mainMaximization.ButtonImage = Geometry.Parse("M146.285714 804.571429h731.428572V365.714286H146.285714v438.857143zM1024 164.571429v694.857142c0 50.285714-41.142857 91.428571-91.428571 91.428572h-841.142858A91.684571 91.684571 0 0 1 0 859.428571v-694.857142C0 114.285714 41.142857 73.142857 91.428571 73.142857h841.142858C982.857143 73.142857 1024 114.285714 1024 164.571429z");
            }
        }

        /// <summary>
        /// 最小化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 下拉菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoreMenus_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 修改主窗体大小
        public const int WM_SYSCOMMAND = 0x112;
        public HwndSource HwndSource;

        public Dictionary<ResizeDirection, Cursor> cursors = new Dictionary<ResizeDirection, Cursor>
        {
            {ResizeDirection.Top, Cursors.SizeNS},
            {ResizeDirection.Bottom, Cursors.SizeNS},
            {ResizeDirection.Left, Cursors.SizeWE},
            {ResizeDirection.Right, Cursors.SizeWE},
            {ResizeDirection.TopLeft, Cursors.SizeNWSE},
            {ResizeDirection.BottomRight, Cursors.SizeNWSE},
            {ResizeDirection.TopRight, Cursors.SizeNESW},
            {ResizeDirection.BottomLeft, Cursors.SizeNESW}
        };

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null && !element.Name.Contains("Resize"))
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        public void ResizePressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ResizeDirection direction = (ResizeDirection)Enum.Parse(typeof(ResizeDirection), element.Name.Replace("Resize", ""));
            this.Cursor = cursors[direction];
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ResizeWindow(direction);
            }
        }

        public void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyDeploy.Controls
{
    /// <summary>
    /// IceMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class IceMessageBox : Window
    {
        private MessageBoxResult result;

        public IceMessageBox()
        {
            InitializeComponent();
        }

        public static void ShowBox(string message)
        {
            Application.Current.Dispatcher.Invoke(() => InitBox(null, message, "Tips", MessageBoxButton.OK).Show());
        }

        public static void ShowBox(string message, string caption)
        {
            Application.Current.Dispatcher.Invoke(() => InitBox(null, message, caption, MessageBoxButton.OK).Show());
        }

        public static void ShowBox(Window owner, string message, string caption)
        {
            Application.Current.Dispatcher.Invoke(() => InitBox(owner, message, caption, MessageBoxButton.OK).Show());
        }

        public static MessageBoxResult ShowDialogBox(string message)
        {
            return ShowDialogBox(null, message, "Tips", MessageBoxButton.OK);
        }

        public static MessageBoxResult ShowDialogBox(string message, string caption)
        {
            return ShowDialogBox(null, message, caption, MessageBoxButton.OK);
        }

        public static MessageBoxResult ShowDialogBox(Window owner, string message, string caption)
        {
            return ShowDialogBox(owner, message, caption, MessageBoxButton.OK);
        }

        public static MessageBoxResult ShowDialogBox(string message, string caption, MessageBoxButton button)
        {
            return ShowDialogBox(null, message, caption, button);
        }

        public static MessageBoxResult ShowDialogBox(Window owner, string message, string caption, MessageBoxButton button)
        {
            return Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
            {
                IceMessageBox box = InitBox(owner, message, caption, button);
                box.ShowDialog();
                return box.result;
            }));
        }

        public static MessageBoxResult ShowDialogBox(Window owner, string message, string caption, MessageBoxButton button, string ok = "Tips", string yes = "Yes", string no = "No", string cancel = "Cancel")
        {
            return Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
            {
                IceMessageBox box = InitBox(owner, message, caption, button);
                box.OK.Content = ok;
                box.Yes.Content = yes;
                box.No.Content = no;
                box.Cancel.Content = cancel;
                box.ShowDialog();
                return box.result;
            }));
        }

        public void CloseWindow()
        {
            this.Close();
        }

        private static IceMessageBox InitBox(Window owner, string message, string caption, MessageBoxButton button)
        {
            IceMessageBox box = new IceMessageBox();
            box.Owner = owner;
            box.Message.Text = message;
            box.Caption.Text = caption;
            switch (button)
            {
                case MessageBoxButton.OK:
                    break;

                case MessageBoxButton.OKCancel:
                    box.Cancel.Visibility = Visibility.Visible;
                    break;

                case MessageBoxButton.YesNo:
                    box.OK.Visibility = Visibility.Collapsed;
                    box.Yes.Visibility = Visibility.Visible;
                    box.No.Visibility = Visibility.Visible;
                    break;

                case MessageBoxButton.YesNoCancel:
                    box.OK.Visibility = Visibility.Collapsed;
                    box.Yes.Visibility = Visibility.Visible;
                    box.No.Visibility = Visibility.Visible;
                    box.Cancel.Visibility = Visibility.Visible;
                    box.Width = 350;
                    break;
            }
            return box;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void NO_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            CloseWindow();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            CloseWindow();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            CloseWindow();
        }
    }
}

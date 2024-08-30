using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyDeploy.Controls
{
    /// <summary>
    /// IceColorPicker.xaml 的交互逻辑
    /// </summary>
    public partial class IceColorPicker : UserControl, INotifyPropertyChanged
    {
        public IceColorPicker()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        double H = 0;
        double S = 1;
        double B = 1;

        private void ThumbPro_ValueChanged(double xpercent, double ypercent)
        {
            H = 360 * ypercent;
            HsbaColor Hcolor = new HsbaColor(H, 1, 1, 1);
            viewSelectColor.Fill = Hcolor.SolidColorBrush;

            Hcolor = new HsbaColor(H, S, B, 1);
            SelectColor = Hcolor.SolidColorBrush;

            ColorChange(Hcolor.RgbaColor);
        }

        private void ThumbPro_ValueChanged_1(double xpercent, double ypercent)
        {
            S = xpercent;
            B = 1 - ypercent;
            HsbaColor Hcolor = new HsbaColor(H, S, B, 1);

            SelectColor = Hcolor.SolidColorBrush;

            ColorChange(Hcolor.RgbaColor);
        }

        SolidColorBrush _SelectColor = System.Windows.Media.Brushes.Transparent;
        public SolidColorBrush SelectColor
        {
            get
            {
                return _SelectColor;
            }
            set
            {
                _SelectColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectColor"));
            }
        }

        int R = 255;
        int G = 255;
        int _B = 255;
        int A = 255;

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text;

                //错误的数据，则使用上次的正确数据
                if (!int.TryParse(TextR.Text, out int Rvalue) || (Rvalue > 255 || Rvalue < 0))
                {
                    TextR.Text = R.ToString();
                    return;
                }

                if (!int.TryParse(TextG.Text, out int Gvalue) || (Gvalue > 255 || Gvalue < 0))
                {
                    TextG.Text = G.ToString();
                    return;
                }

                if (!int.TryParse(TextB.Text, out int Bvalue) || (Bvalue > 255 || Bvalue < 0))
                {
                    TextB.Text = _B.ToString();
                    return;
                }
                if (!int.TryParse(TextA.Text, out int Avalue) || (Avalue > 255 || Avalue < 0))
                {
                    TextA.Text = A.ToString();
                    return;
                }
                R = Rvalue; G = Gvalue; _B = Bvalue; A = Avalue;


                RgbaColor Hcolor = new RgbaColor(R, G, _B, A);
                SelectColor = Hcolor.SolidColorBrush;

                TextHex.Text = Hcolor.HexString;
            }
        }

        private void HexTextLostFocus(object sender, RoutedEventArgs e)
        {

            RgbaColor Hcolor = new RgbaColor(TextHex.Text);

            SelectColor = Hcolor.SolidColorBrush;
            TextR.Text = Hcolor.R.ToString();
            TextG.Text = Hcolor.G.ToString();
            TextB.Text = Hcolor.B.ToString();
            TextA.Text = Hcolor.A.ToString();
        }

        private void ColorChange(RgbaColor Hcolor)
        {
            TextR.Text = Hcolor.R.ToString();
            TextG.Text = Hcolor.G.ToString();
            TextB.Text = Hcolor.B.ToString();
            TextA.Text = Hcolor.A.ToString();

            TextHex.Text = Hcolor.HexString;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            pop.IsOpen = true;

            if (btn.Background is SolidColorBrush selectColor)
            {
                SelectColor = selectColor;
                RgbaColor Hcolor = new RgbaColor(SelectColor);
                ColorChange(Hcolor);

                var xpercent = Hcolor.HsbaColor.S;
                var ypercent = 1 - Hcolor.HsbaColor.B;

                var Ypercent = Hcolor.HsbaColor.H / 360;

                thumbH.SetTopLeftByPercent(1, Ypercent);
                thumbSB.SetTopLeftByPercent(xpercent, ypercent);
            }
        }
    }

    /// <summary>
    /// 封装Canvas 到Thumb来简化 Thumb的使用，关注熟悉X,Y 表示 thumb在坐标中距离左，上的距离
    /// 默认canvas 里用一个小圆点来表示当前位置
    /// </summary>
    public class ThumbPro : Thumb
    {
        // 距离 Canvas 的 Top,模板中需要 Canvas.Top 绑定此 Top
        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(double), typeof(ThumbPro), new PropertyMetadata(0.0));


        // 距离 Canvas 的 Top,模板中需要 Canvas.Left 绑定此 Left
        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(double), typeof(ThumbPro), new PropertyMetadata(0.0));

        double FirstTop;
        double FirstLeft;

        // 小圆点的半径
        public double Xoffset { get; set; }
        public double Yoffset { get; set; }

        public bool VerticalOnly { get; set; } = false;

        public double Xpercent { get { return (Left + Xoffset) / ActualWidth; } }
        public double Ypercent { get { return (Top + Yoffset) / ActualHeight; } }

        public void SetTopLeftByPercent(double xpercent, double ypercent)
        {
            Top = ypercent * ActualHeight - Yoffset;
            if (!VerticalOnly)
                Left = xpercent * ActualWidth - Xoffset;
        }

        /// <summary>
        /// 值回调
        /// </summary>
        public event Action<double, double>? ValueChanged;

        public ThumbPro()
        {
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                if (!VerticalOnly)
                    Left = -Xoffset;
                Top = -Yoffset;
            };
            DragStarted += (object sender, DragStartedEventArgs e) =>
            {
                //当随便点击某点，把小远点移到当前位置，注意是小远点的中心位置移到当前位置
                if (!VerticalOnly)
                {
                    Left = e.HorizontalOffset - Xoffset;
                    FirstLeft = Left;
                }
                Top = e.VerticalOffset - Yoffset;
                FirstTop = Top;

                ValueChanged?.Invoke(Xpercent, Ypercent);
            };

            DragDelta += (object sender, DragDeltaEventArgs e) =>
            {
                //按住拖拽时，小远点随着鼠标移动
                if (!VerticalOnly)
                {
                    double x = FirstLeft + e.HorizontalChange;

                    if (x < -Xoffset) Left = -Xoffset;
                    else if (x > ActualWidth - Xoffset) Left = ActualWidth - Xoffset;
                    else Left = x;
                }

                double y = FirstTop + e.VerticalChange;

                if (y < -Yoffset) Top = -Yoffset;
                else if (y > ActualHeight - Yoffset) Top = ActualHeight - Yoffset;
                else Top = y;
                ValueChanged?.Invoke(Xpercent, Ypercent);
            };
        }
    }

    public class HsbaColor
    {
        double h = 0, s = 0, b = 0, a = 0;
        /// <summary>
        /// 0 - 359，360 = 0
        /// </summary>
        public double H { get { return h; } set { h = value < 0 ? 0 : value >= 360 ? 0 : value; } }
        /// <summary>
        /// 0 - 1
        /// </summary>
        public double S { get { return s; } set { s = value < 0 ? 0 : value > 1 ? 1 : value; } }
        /// <summary>
        /// 0 - 1
        /// </summary>
        public double B { get { return b; } set { b = value < 0 ? 0 : value > 1 ? 1 : value; } }
        /// <summary>
        /// 0 - 1
        /// </summary>
        public double A { get { return a; } set { a = value < 0 ? 0 : value > 1 ? 1 : value; } }
        /// <summary>
        /// 亮度 0 - 100
        /// </summary>
        public int Y { get { return RgbaColor.Y; } }

        public HsbaColor() { H = 0; S = 0; B = 1; A = 1; }
        public HsbaColor(double h, double s, double b, double a = 1) { H = h; S = s; B = b; A = a; }
        public HsbaColor(int r, int g, int b, int a = 255)
        {
            HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(r, g, b, a));
            H = hsba.H;
            S = hsba.S;
            B = hsba.B;
            A = hsba.A;
        }
        public HsbaColor(System.Windows.Media.Brush brush)
        {
            HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(brush));
            H = hsba.H;
            S = hsba.S;
            B = hsba.B;
            A = hsba.A;
        }
        public HsbaColor(string hexColor)
        {
            HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(hexColor));
            H = hsba.H;
            S = hsba.S;
            B = hsba.B;
            A = hsba.A;
        }

        public System.Windows.Media.Color Color { get { return RgbaColor.Color; } }
        public System.Windows.Media.Color OpaqueColor { get { return RgbaColor.OpaqueColor; } }
        public SolidColorBrush SolidColorBrush { get { return RgbaColor.SolidColorBrush; } }
        public SolidColorBrush OpaqueSolidColorBrush { get { return RgbaColor.OpaqueSolidColorBrush; } }

        public string HexString { get { return Color.ToString(); } }
        public string RgbaString { get { return RgbaColor.RgbaString; } }

        public RgbaColor RgbaColor { get { return Utility.HsbaToRgba(this); } }
    }

    public class RgbaColor
    {
        int r = 0, g = 0, b = 0, a = 0;
        /// <summary>
        /// 0 - 255
        /// </summary>
        public int R { get { return r; } set { r = value < 0 ? 0 : value > 255 ? 255 : value; } }
        /// <summary>
        /// 0 - 255
        /// </summary>
        public int G { get { return g; } set { g = value < 0 ? 0 : value > 255 ? 255 : value; } }
        /// <summary>
        /// 0 - 255
        /// </summary>
        public int B { get { return b; } set { b = value < 0 ? 0 : value > 255 ? 255 : value; } }
        /// <summary>
        /// 0 - 255
        /// </summary>
        public int A { get { return a; } set { a = value < 0 ? 0 : value > 255 ? 255 : value; } }
        /// <summary>
        /// 亮度 0 - 100
        /// </summary>
        public int Y { get { return Utility.GetBrightness(R, G, B); } }

        public RgbaColor() { R = 255; G = 255; B = 255; A = 255; }
        public RgbaColor(int r, int g, int b, int a = 255) { R = r; G = g; B = b; A = a; }
        public RgbaColor(System.Windows.Media.Brush brush)
        {
            if (brush != null)
            {
                R = ((SolidColorBrush)brush).Color.R;
                G = ((SolidColorBrush)brush).Color.G;
                B = ((SolidColorBrush)brush).Color.B;
                A = ((SolidColorBrush)brush).Color.A;
            }
            else
            {
                R = G = B = A = 255;
            }
        }
        public RgbaColor(double h, double s, double b, double a = 1)
        {
            RgbaColor rgba = Utility.HsbaToRgba(new HsbaColor(h, s, b, a));
            R = rgba.R;
            G = rgba.G;
            B = rgba.B;
            A = rgba.A;

        }
        public RgbaColor(string hexColor)
        {
            try
            {
                System.Windows.Media.Color color;
                if (hexColor.Substring(0, 1) == "#") color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
                else color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#" + hexColor);
                R = color.R;
                G = color.G;
                B = color.B;
                A = color.A;
            }
            catch
            {

            }
        }

        public System.Windows.Media.Color Color { get { return System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B); } }
        public System.Windows.Media.Color OpaqueColor { get { return System.Windows.Media.Color.FromArgb((byte)255.0, (byte)R, (byte)G, (byte)B); } }
        public SolidColorBrush SolidColorBrush { get { return new SolidColorBrush(Color); } }
        public SolidColorBrush OpaqueSolidColorBrush { get { return new SolidColorBrush(OpaqueColor); } }

        public string HexString { get { return Color.ToString(); } }
        public string RgbaString { get { return R + "," + G + "," + B + "," + A; } }

        public HsbaColor HsbaColor { get { return Utility.RgbaToHsba(this); } }
    }

    /// <summary>
    /// 实用工具
    /// </summary>
    internal class Utility
    {
        /// <summary>
        /// Rgba转Hsba
        /// </summary>
        /// <param name="rgba"></param>
        /// <returns></returns>
        internal static HsbaColor RgbaToHsba(RgbaColor rgba)
        {
            int[] rgb = new int[] { rgba.R, rgba.G, rgba.B };
            Array.Sort(rgb);
            int max = rgb[2];
            int min = rgb[0];

            double hsbB = max / 255.0;
            double hsbS = max == 0 ? 0 : (max - min) / (double)max;
            double hsbH = 0;

            if (rgba.R == rgba.G && rgba.R == rgba.B)
            {

            }
            else
            {
                if (max == rgba.R && rgba.G >= rgba.B) hsbH = (rgba.G - rgba.B) * 60.0 / (max - min) + 0.0;
                else if (max == rgba.R && rgba.G < rgba.B) hsbH = (rgba.G - rgba.B) * 60.0 / (max - min) + 360.0;
                else if (max == rgba.G) hsbH = (rgba.B - rgba.R) * 60.0 / (max - min) + 120.0;
                else if (max == rgba.B) hsbH = (rgba.R - rgba.G) * 60.0 / (max - min) + 240.0;
            }

            return new HsbaColor(hsbH, hsbS, hsbB, rgba.A / 255.0);
        }

        /// <summary>
        /// Hsba转Rgba
        /// </summary>
        /// <param name="hsba"></param>
        /// <returns></returns>
        internal static RgbaColor HsbaToRgba(HsbaColor hsba)
        {
            double r = 0, g = 0, b = 0;
            int i = (int)((hsba.H / 60) % 6);
            double f = (hsba.H / 60) - i;
            double p = hsba.B * (1 - hsba.S);
            double q = hsba.B * (1 - f * hsba.S);
            double t = hsba.B * (1 - (1 - f) * hsba.S);
            switch (i)
            {
                case 0:
                    r = hsba.B;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = hsba.B;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = hsba.B;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = hsba.B;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = hsba.B;
                    break;
                case 5:
                    r = hsba.B;
                    g = p;
                    b = q;
                    break;
                default:
                    break;
            }
            return new RgbaColor((int)(255.0 * r), (int)(255.0 * g), (int)(255.0 * b), (int)(255.0 * hsba.A));
        }

        /// <summary>
        /// 获取颜色亮度
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static int GetBrightness(int r, int g, int b)
        {
            return (int)((0.2126 * r + 0.7152 * g + 0.0722 * b) / 2.55);
        }
    }
}

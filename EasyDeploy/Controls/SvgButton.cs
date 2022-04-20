using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasyDeploy.Controls
{
    /// <summary>
    /// 仅SVG图片按钮
    /// </summary>
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class SvgButton : Button
    {
        static SvgButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SvgButton), new FrameworkPropertyMetadata(typeof(SvgButton)));

        /// <summary>
        /// 控件中 SVG 图片
        /// </summary>
        public Geometry ButtonImage
        {
            get { return (Geometry)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }
        public static readonly DependencyProperty ButtonImageProperty =
            DependencyProperty.Register("ButtonImage", typeof(Geometry), typeof(SvgButton));

        /// <summary>
        /// 常规状态下的颜色
        /// </summary>
        public Brush NormalFill
        {
            get { return (Brush)GetValue(NormalFillProperty); }
            set { SetValue(NormalFillProperty, value); }
        }
        public static readonly DependencyProperty NormalFillProperty =
            DependencyProperty.Register("NormalFill", typeof(Brush), typeof(SvgButton), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 鼠标放置时背景颜色
        /// </summary>
        public Thickness ImageMargin
        {
            get { return (Thickness)GetValue(ImageMarginProperty); }
            set { SetValue(ImageMarginProperty, value); }
        }
        public static readonly DependencyProperty ImageMarginProperty =
            DependencyProperty.Register("ImageMargin", typeof(Thickness), typeof(SvgButton));

        /// <summary>
        /// 鼠标放置时 SVG 颜色
        /// </summary>
        public Brush IsMouseOverFill
        {
            get { return (Brush)GetValue(IsMouseOverFillProperty); }
            set { SetValue(IsMouseOverFillProperty, value); }
        }
        public static readonly DependencyProperty IsMouseOverFillProperty =
            DependencyProperty.Register("IsMouseOverFill", typeof(Brush), typeof(SvgButton), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 鼠标放置时背景颜色
        /// </summary>
        public Brush IsMouseOverBackground
        {
            get { return (Brush)GetValue(IsMouseOverBackgroundProperty); }
            set { SetValue(IsMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty IsMouseOverBackgroundProperty =
            DependencyProperty.Register("IsMouseOverBackground", typeof(Brush), typeof(SvgButton), new PropertyMetadata(default(Brush)));
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasyDeploy.Controls
{
    /// <summary>
    /// 优化原生控件
    /// 增加圆角
    /// </summary>
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceButton : Button
    {
        static IceButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceButton), new FrameworkPropertyMetadata(typeof(IceButton)));

        /// <summary>
        /// 鼠标放置时颜色
        /// </summary>
        public Brush IsMouseOverFill
        {
            get { return (Brush)GetValue(IsMouseOverFillProperty); }
            set { SetValue(IsMouseOverFillProperty, value); }
        }
        public static readonly DependencyProperty IsMouseOverFillProperty =
            DependencyProperty.Register("IsMouseOverFill", typeof(Brush), typeof(IceButton), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 按钮圆角值
        /// </summary>
        public CornerRadius ButtonCornerRadius
        {
            get { return (CornerRadius)GetValue(ButtonCornerRadiusProperty); }
            set { SetValue(ButtonCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty ButtonCornerRadiusProperty =
            DependencyProperty.Register("ButtonCornerRadius", typeof(CornerRadius), typeof(IceButton));
    }
}

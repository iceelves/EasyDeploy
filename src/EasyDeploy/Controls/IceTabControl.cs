using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceTabControl : TabControl
    {
        static IceTabControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceTabControl), new FrameworkPropertyMetadata(typeof(IceTabControl)));
    }
}

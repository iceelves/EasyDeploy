using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceTabItem : TabItem
    {
        static IceTabItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceTabItem), new FrameworkPropertyMetadata(typeof(IceTabItem)));
    }
}

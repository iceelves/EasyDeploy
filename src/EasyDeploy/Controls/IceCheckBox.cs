using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceCheckBox : CheckBox
    {
        static IceCheckBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceCheckBox), new FrameworkPropertyMetadata(typeof(IceCheckBox)));
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceDataGrid : DataGrid
    {
        static IceDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IceDataGrid), new FrameworkPropertyMetadata(typeof(IceDataGrid)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EasyDeploy.Controls
{
    [TemplatePart(Name = "Part_Path", Type = typeof(FrameworkElement))]
    public class IceRichTextBox : RichTextBox
    {
        static IceRichTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(IceRichTextBox), new FrameworkPropertyMetadata(typeof(IceRichTextBox)));

        /// <summary>
        /// 添加文本
        /// </summary>
        /// <param name="Text"></param>
        public void SetText(string Text)
        {
            for (int i = 0; i < 20; i++)
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run() { Foreground = Brushes.Red, Text = "R" });
                paragraph.Inlines.Add(new Run() { Foreground = Brushes.Green, Text = "G" });
                paragraph.Inlines.Add(new Run() { Foreground = Brushes.Blue, Text = "B" });
                this.Document.Blocks.Add(paragraph);
            }
        }
    }
}

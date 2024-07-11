using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace EasyDeploy.Helpers
{
    public class RichTextBoxHelper : DependencyObject
    {
        private static List<Guid> _recursionProtection = new List<Guid>();

        public static string GetDocumentXaml(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentXamlProperty);
        }

        public static void SetDocumentXaml(DependencyObject obj, string value)
        {
            var fw1 = (FrameworkElement)obj;
            if (fw1.Tag == null || (Guid)fw1.Tag == Guid.Empty)
                fw1.Tag = Guid.NewGuid();
            _recursionProtection.Add((Guid)fw1.Tag);
            obj.SetValue(DocumentXamlProperty, value);
            _recursionProtection.Remove((Guid)fw1.Tag);
        }

        public static readonly DependencyProperty DocumentXamlProperty = DependencyProperty.RegisterAttached(
            "DocumentXaml",
            typeof(string),
            typeof(RichTextBoxHelper),
            new FrameworkPropertyMetadata(
                "",
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (obj, e) =>
                {
                    var richTextBox = (RichTextBox)obj;
                    if (richTextBox.Tag != null && _recursionProtection.Contains((Guid)richTextBox.Tag))
                        return;

                    // Parse the XAML to a document (or use XamlReader.Parse())

                    try
                    {
                        string docXaml = GetDocumentXaml(richTextBox);
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(docXaml));
                        FlowDocument doc;
                        if (!string.IsNullOrEmpty(docXaml))
                        {
                            doc = (FlowDocument)XamlReader.Load(stream);
                        }
                        else
                        {
                            doc = new FlowDocument();
                        }

                        // Set the document
                        richTextBox.Document = doc;
                    }
                    catch (Exception)
                    {
                        richTextBox.Document = new FlowDocument();
                    }

                    // When the document changes update the source
                    richTextBox.TextChanged += (obj, e) =>
                    {
                        if (obj is RichTextBox richTextBox2)
                        {
                            SetDocumentXaml(richTextBox, XamlWriter.Save(richTextBox2.Document));
                        }
                    };
                }
            )
        );
    }
}

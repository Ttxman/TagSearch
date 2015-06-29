using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TagSearch
{
    /// <summary>
    /// Interaction logic for FileWindow.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        private FoundInfo fi;

        public FileWindow()
        {
            InitializeComponent();
        }

        public FileWindow(FoundInfo fi):this()
        {
            this.fi = fi;
        }

        static NonEditableBlockGenerator Generator = new NonEditableBlockGenerator();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox.Text = fi.Parent.LoadedFiles[fi.Fileindex];
            this.Title = fi.Parent.Files[fi.Fileindex].FullName;

            TextBox.Select(fi.Match.Index, fi.Match.Value.TrimEnd().Length);
            TextBox.CaretOffset = fi.Match.Index;
            TextBox.TextArea.Caret.BringCaretToView();

            TextBox.TextArea.TextView.ElementGenerators.Add(Generator);

            checkbox.IsChecked = !NonEditableBlockGenerator.HideNSE;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var cbox = sender as CheckBox;
            if (cbox == null)
                return;

            NonEditableBlockGenerator.HideNSE = cbox.IsChecked == false;

            TextBox.TextArea.TextView.ElementGenerators.Remove(Generator);
            TextBox.TextArea.TextView.ElementGenerators.Add(Generator);
        }
    }


    public class NonEditableBlockGenerator : VisualLineElementGenerator
    {
        static bool _hidneNSE = true;

        /// <summary>
        /// hide non speech events
        /// </summary>
        public static bool HideNSE
        {
            get { return _hidneNSE; }
            set
            {
                _hidneNSE = value;
            }
        }

        public static readonly Regex ignoredGroup = new Regex(@"<.*?>", RegexOptions.Singleline | RegexOptions.Compiled);
        Match FindMatch(int startOffset)
        {
            // fetch the end offset of the VisualLine being generated
            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            TextDocument document = CurrentContext.Document;
            string relevantText = document.GetText(startOffset, endOffset - startOffset);
            return ignoredGroup.Match(relevantText);
        }

        /// Gets the first offset >= startOffset where the generator wants to construct
        /// an element.
        /// Return -1 to signal no interest.
        public override int GetFirstInterestedOffset(int startOffset)
        {
            Match m = FindMatch(startOffset);
            return m.Success ? (startOffset + m.Index) : -1;
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        public override VisualLineElement ConstructElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                TextBlock t;
                if (_hidneNSE)
                    t = new TextBlock() { Text = "" };
                else
                    t = new TextBlock() { Text = m.Value };


                return new InlineObjectElement(m.Length, t);
            }
            return null;
        }
    }
}

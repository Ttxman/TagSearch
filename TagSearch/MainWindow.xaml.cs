using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace TagSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SearchBox[] boxes;

        List<DatasetInfo> Data = new List<DatasetInfo>();

        public MainWindow()
        {
            InitializeComponent();
            searchboxes.Children.Clear();

            boxes = new SearchBox[]
            {
                new SearchBox(),
                new SearchBox(),
                new SearchBox(),
                new SearchBox(),
            };
            foreach (var box in boxes)
                searchboxes.Children.Add(box);

            Mouse.OverrideCursor = Cursors.Wait;

            var dirs = new DirectoryInfo("DATA").GetDirectories();
            foreach (var dir in dirs)
            {
                var din = new DatasetInfo(dir);
                Data.Add(din);
                tabs.Items.Add(din.Tab);
            }

            Mouse.OverrideCursor = null;
        }

        static string breakPattern = @"((?!<w|<c|<head>|<\\head>).)*";//@"((?!<w|<c|<p>|</text>|<s c=|\^|<head>|<\\head>).)*";
        static string positivePattern = @"<[{1}] ({0})>" + breakPattern; //<w((?! (?:AT|IO)>).)*?>((?!<w).)*
        static string negativePattern = @"<[{1}]((?!({0}|>)).)*>" + breakPattern;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string regex = "";

                for (int i = 0; i < boxes.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(boxes[i].Text))
                        continue;


                    string text = string.Join("|", boxes[i].Text.Split(new[] { '|', ',', ' ', '/', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(w => Regex.Escape(w)));

                    string types = "" + (boxes[i].UseC == true ? "c" : "") + (boxes[i].UseW == true ? "w" : "");

                    if (string.IsNullOrWhiteSpace(text))
                        continue;


                    if (boxes[i].IsChecked == true)
                        regex += string.Format(positivePattern, text, types);
                    else
                        regex += string.Format(negativePattern, text, types);
                }

                if (string.IsNullOrWhiteSpace(regex))
                {
                    MessageBox.Show("Vyhledávací řetězec je prázdný nebo obsahuje pouze neplatné znaky", "Chyba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                Regex r = new Regex(regex, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
                List<Match> found = new List<Match>();
                foreach (var item in Data)
                {
                    item.Find(r);
                }
            }
            finally
            {
                // list.ItemsSource = found;
                Mouse.OverrideCursor = null;
            }
        }




        private void Sort2(ref double d1, ref double d2)
        {
            double bfr;
            if (d1 < d2)
            {
                bfr = d1;
                d1 = d2;
                d2 = bfr;
            }
        }
        private void Max(double d1, double d2, double d3, double d4)
        {
            Sort2(ref d1, ref d2);
            Sort2(ref d3, ref d2);

            //d1 a d3 jsou ty vetsi

            Sort2(ref d1, ref d3);

            //d1 je maximalni z d1,2,3,4

            Sort2(ref d2, ref d3);
            Sort2(ref d2, ref d4);

            //d2 je maximalni d2,3,4


            Sort2(ref d3, ref d4);

            //d3 je maximalni z d3,4

            //serazeno...
        }
        private void bubblesort(double d1, double d2, double d3, double d4)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3 - i; j++)
                {
                    switch (j)
                    {
                        case 0:
                            Sort2(ref d1, ref d2);
                            break;
                        case 1:
                            Sort2(ref d2, ref d3);
                            break;
                        case 2:
                            Sort2(ref d3, ref d4);
                            break;
                    }
                }
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            if(lb == null )
                return;


            if (lb.SelectedIndex < 0)
                return;

            var fi = lb.SelectedItem as FoundInfo;

            if (fi == null)
                return;

            var file = fi.Parent.Files[fi.Fileindex];

            new FileWindow(fi) {Owner = this }.ShowDialog( );
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabControl tc = sender as TabControl;
            if (tc == null) return;

            TabItem ti = tc.SelectedItem as TabItem;
            if (ti == null) return;

            ListBox ls = ti.Content as ListBox;
            if (ls == null) return;

            var data = string.Join("\r\n", ls.Items.OfType<FoundInfo>().Select(i => i.ToString()));

            Clipboard.SetText(data);
            

        }
    }


    public class FoundInfo
    {


        public FoundInfo(Match match, DatasetInfo parent, int fileIndex)
        {
            this.Match = match;
            this.Parent = parent;
            this.Fileindex = fileIndex;
        }

        public Match Match { get; set; }
        public DatasetInfo Parent { get; set; }
        public int Fileindex { get; set; }


        public override string ToString()
        {
            return Match.ToString().Trim().Replace('\n',' ').Replace('\r',' ').Replace("  "," ");
        }

        public int Index { get; set; }
    }

    public class DatasetInfo
    {
        public DirectoryInfo Directory { get; set; }
        public FileInfo[] Files { get; set; }
        public string[] LoadedFiles { get; set; }

        public TabItem Tab { get; set; }
        public ListBox ListBox { get; set; }

        public DatasetInfo(DirectoryInfo nfo)
        {
            Directory = nfo;
            Files = nfo.GetFiles("*.txt", SearchOption.AllDirectories).OrderBy(f => f.FullName).ToArray();
            LoadedFiles = Files.Select(f => File.ReadAllText(f.FullName)).ToArray();

            Tab = new TabItem();
            Tab.Header = Directory.Name;
            Tab.DataContext = this;
            ListBox = new ListBox();
            Tab.Content = ListBox;
        }

        internal void Find(Regex r)
        {
            var list = new List<FoundInfo>();
            for (int i = 0; i < LoadedFiles.Length; i++)
			{
                var item = LoadedFiles[i];
                list.AddRange(
                    r.Matches(item)
                    .Cast<Match>()
                    .Select(m => new FoundInfo(m,this,i)));
			}
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Index = i+1;
            }
           // File.WriteAllLines(Directory.Name+ "_found.txt", list.Select(li => li.ToString()));
            ListBox.ItemsSource = list;
            Tab.Header = Directory.Name + " (" + list.Count + ")";
        }
    }
}

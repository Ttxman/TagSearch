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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagSearch
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    [System.Windows.Markup.ContentProperty("Text")]
    public partial class SearchBox : UserControl
    {

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public bool? UseW
        {
            get { return (bool?)GetValue(UseWProperty); }
            set { SetValue(UseWProperty, value); }
        }

        public bool? UseC
        {
            get { return (bool?)GetValue(UseCProperty); }
            set { SetValue(UseCProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Ischecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(SearchBox), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for Ischecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseWProperty =
            DependencyProperty.Register("UseW", typeof(bool?), typeof(SearchBox), new PropertyMetadata(true, UseChanged));

        private static void UseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (SearchBox)d;
            var oldw = (bool?)e.OldValue;
            var neww = (bool?)e.NewValue;
            bool? ow =(e.Property == UseWProperty)?box.UseC:box.UseW;

            if(!ow.HasValue || !ow.Value) //both properties cannot be false
                if (!neww.HasValue || !neww.Value)
                {
                    box.SetValue(e.Property, true);
                }

            
                
        }

        // Using a DependencyProperty as the backing store for Ischecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseCProperty =
            DependencyProperty.Register("UseC", typeof(bool?), typeof(SearchBox), new PropertyMetadata(false, UseChanged));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchBox), new PropertyMetadata(""));


        public SearchBox()
        {
            InitializeComponent();
        }
    }
}

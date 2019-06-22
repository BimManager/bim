using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for SelectFontWindow.xaml
    /// </summary>
    public partial class SelectFontWindow : Window
    {
        public string SelectedFontFamily { get; private set; }
        public bool Families { get; private set; }
        public bool TextNoteTypes { get; private set; }
        public bool DimensionTypes { get; private set; }

        public SelectFontWindow()
        {
            InitializeComponent();
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedFontFamily = "";
            this.Close();
        }

        private void ok_btn_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedFontFamily = this.fontsListBox.SelectedItem.ToString();
            this.Families = (bool)this.chbx_families.IsChecked;
            this.TextNoteTypes = (bool)this.chbx_txt_notes_types.IsChecked;
            this.DimensionTypes = (bool)this.chbx_dim_types.IsChecked;
            this.Close();
        }
    }
}

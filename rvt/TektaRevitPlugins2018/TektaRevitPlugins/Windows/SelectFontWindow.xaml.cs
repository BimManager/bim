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
        public Font PickedFont { get; private set; }
        public bool Families { get; private set; }
        public bool TextNoteTypes { get; private set; }
        public bool DimensionTypes { get; private set; }

        public SelectFontWindow()
        {
            InitializeComponent();
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.PickedFont = null;
            this.Close();
        }

        private void ok_btn_Click(object sender, RoutedEventArgs e)
        {
            double widthFactor = Double.Parse(this.widthFactor.Text.Replace('.', ','),
                System.Globalization.NumberStyles.Float);

            if (widthFactor == 0)
                widthFactor = 1;

            Font font = new Font(
                this.fontsListBox.SelectedItem.ToString(),
                0,
                widthFactor,
                (bool)this.isBold.IsChecked,
                (bool)this.isItalic.IsChecked,
                (bool)this.isUnderlined.IsChecked,
                (bool)this.isOpaque.IsChecked
                );

            this.PickedFont = font;
            this.Families = (bool)this.chbx_families.IsChecked;
            this.TextNoteTypes = (bool)this.chbx_txt_notes_types.IsChecked;
            this.DimensionTypes = (bool)this.chbx_dim_types.IsChecked;
            this.Close();
        }

        private bool isValid(string text)
        {
            //if (System.Text.RegularExpressions.Regex.IsMatch(text, "[0-9]") ||
            // (System.Text.RegularExpressions.Regex.IsMatch(text, "[.]") && 
            //!this.widthFactor.Text.Contains('.')) && this.widthFactor.Text.Length < 4)

            // return true;
            // else
            //eturn false;

            double val;
            return Double.TryParse(text.Replace('.',','), out val) && val >= 0 && val < 3;
        }

        private void widthFactor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isValid(((TextBox)sender).Text + e.Text);
        }
    }
}

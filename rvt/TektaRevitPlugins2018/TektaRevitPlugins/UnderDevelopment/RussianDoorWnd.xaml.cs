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
    /// Interaction logic for RussianDoorWnd.xaml
    /// </summary>
    public partial class RussianDoorWnd : Window
    {
        public RussianDoorWnd()
        {
            InitializeComponent();
        }

        private void mainComboBox_Selected(object sender, RoutedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            ComboBoxItem item = (ComboBoxItem)cb.SelectedItem;
            switch (item.Name) {
                case "item1":
                    this.g6629.Visibility = Visibility.Visible;
                    this.lbg6629.Visibility = Visibility.Visible;

                    break;
                case "item2":
                    this.g6629.Visibility = Visibility.Collapsed;
                    this.lbg6629.Visibility = Visibility.Collapsed;
                    break;
                case "item3":
                    this.g6629.Visibility = Visibility.Collapsed;
                    this.lbg6629.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void itemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenLabel();
        }

        private void doorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenLabel();
        }

        private void height_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenLabel();
        }

        private void width_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenLabel();
        }

        private void props_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegenLabel();
        }

        private void RegenLabel()
        {
            if (this.itemType.SelectedItem != null && this.doorType.SelectedItem != null &&
               this.height.SelectedItem != null && this.width.SelectedItem != null &&
               this.props.SelectedItem != null) {
                string text = ((ComboBoxItem)this.itemType.SelectedItem).Content.ToString()
                    + ((ComboBoxItem)this.doorType.SelectedItem).Content.ToString() + " " +
                    ((ComboBoxItem)this.height.SelectedItem).Content.ToString() + "-" +
                    ((ComboBoxItem)this.width.SelectedItem).Content.ToString() +
                    ((ComboBoxItem)this.props.SelectedItem).Content.ToString() +
                    " ГОСТ 6629-88";

                if (this.lbg6629 != null)
                    this.lbg6629.Content = text;

                this.btnCreate.Visibility = Visibility.Visible;
            }
        }
    }
}

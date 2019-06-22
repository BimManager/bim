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

using ViewSheetSet = Autodesk.Revit.DB.ViewSheetSet;
using RvtDocument = Autodesk.Revit.DB.Document;
using RvtElement = Autodesk.Revit.DB.Element;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for PrintWnd.xaml
    /// </summary>
    public partial class PrintWnd : Window
    {
        public IList<RvtElement> SelectedSets { get; private set; }
        public PrintWnd(RvtDocument doc)
        {
            InitializeComponent();
            IList<ViewSheetSet> viewSheetSets =
                SheetSelectorCmd.GetViewSheetSets(doc);

            this.lb_view_sets.ItemsSource
                = viewSheetSets;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedSets = 
                GetSelectedSets(this.lb_view_sets, "cb_view_set");

            if(this.SelectedSets.Count==0)
            {
                MessageBox.Show("Please select at least one set or click 'Close'.");
                return;
            }

            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Helper Methods
        private static IList<RvtElement> GetSelectedSets(ListBox listBox, string cbName)
        {
            IList<RvtElement> theSelected = new List<RvtElement>();

            for(int i=0; i<listBox.Items.Count; ++i)
            {
                ListBoxItem lbItem = (ListBoxItem)listBox.
                    ItemContainerGenerator.ContainerFromIndex(i);

                if (lbItem == null)
                    continue;

                ContentPresenter contentPresenter =
                    ExportImportExcel.FindVisualChild<ContentPresenter>(lbItem);

                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox =(CheckBox)dataTemplate
                    .FindName(cbName, contentPresenter);
                if ((bool)checkBox.IsChecked)
                    theSelected.Add(listBox.Items[i] as RvtElement);
            }

            return theSelected;
        }
        #endregion
    }
}

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

using Microsoft.Win32;

using Autodesk.Revit.DB;
using RvtBinding = Autodesk.Revit.DB.Binding;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for SharedParameterSaveDialog.xaml
    /// </summary>
    public partial class SharedParameterSaveDialog : Window
    {
        Document m_doc;

        public SharedParameterSaveDialog()
        {
            InitializeComponent();
            
        }

        public SharedParameterSaveDialog(Document doc, IList<SharedParameter> parameters)
        {
            InitializeComponent();
            this.m_doc = doc;
            this.sp_names.ItemsSource = parameters;
        }

        private void export_btn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Text file (*.txt)|*.txt";
            if((bool)saveDialog.ShowDialog())
            {
                using (System.IO.Stream stream = saveDialog.OpenFile())
                { stream.Close(); }
                SpHandlerCmd.ExportParameters(m_doc, saveDialog.FileName);
            }
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void import_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Text file (*.txt) | *.txt";
            if((bool)openDialog.ShowDialog())
            {
                //FileName = openDialog.FileName;
            }
        }

        private void sp_names_Selected(object sender, RoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            SharedParameter sharedParam = listBox.SelectedItem as SharedParameter;
            this.detailed_info.DataContext = sharedParam;
            IList<string> categories = sharedParam.GetCategories
                                        .Select<Category, string>(c => c.Name)
                                        .ToList();
            this.categories.ItemsSource = categories;

        }
    }


    
}

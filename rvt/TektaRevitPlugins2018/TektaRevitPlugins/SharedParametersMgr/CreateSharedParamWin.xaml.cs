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

using RvtDocument = Autodesk.Revit.DB.Document;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for ReadOnlySharedParamWin.xaml
    /// </summary>
    public partial class CreateSharedParamWin : Window
    {
        RvtDocument m_doc;

        internal string ParameterName { get; private set; }
        internal Autodesk.Revit.DB.ParameterType ParameterType { get; private set; }
        internal Autodesk.Revit.DB.BuiltInParameterGroup ParameterGroup { get; private set; }
        internal Autodesk.Revit.DB.BuiltInCategory Category { get; private set; }
        internal bool IsInstance { get; private set; }
        internal bool IsModifiable { get; private set; }
        internal bool IsVisible { get; private set; }
        internal string GroupName { get; private set; }
        internal bool CanVaryBtwGroups { get; private set; }

        public CreateSharedParamWin(RvtDocument doc)
        {
            InitializeComponent();
            m_doc = doc;
            this.cb_param_type.ItemsSource = Enum.GetValues(typeof(Autodesk.Revit.DB.ParameterType));
            this.cb_param_group.ItemsSource = Enum.GetValues(typeof(Autodesk.Revit.DB.BuiltInParameterGroup));
            this.cb_category.ItemsSource = Enum.GetValues(typeof(Autodesk.Revit.DB.BuiltInCategory));
            this.tb_file_path.Text = doc.Application.SharedParametersFilename!=null ? doc.Application.SharedParametersFilename : string.Empty;
            IList<string> emptyArr = new List<string>();
            this.cb_def_group.ItemsSource = this.tb_file_path.Text != string.Empty ? 
                doc.Application.OpenSharedParameterFile().Groups.Select(dg => dg.Name) : emptyArr;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.ParameterName = this.tb_param_name.Text;
            this.ParameterType = (Autodesk.Revit.DB.ParameterType)this.cb_param_type.SelectedItem;
            this.ParameterGroup = (Autodesk.Revit.DB.BuiltInParameterGroup)this.cb_param_group.SelectedItem;
            this.Category = (Autodesk.Revit.DB.BuiltInCategory)this.cb_category.SelectedItem;
            this.IsInstance = (bool)this.chb_is_instance.IsChecked;
            this.IsModifiable = (bool)this.chb_is_modifiable.IsChecked;
            this.IsVisible = (bool)this.chb_is_visible.IsChecked;
            this.GroupName = this.cb_def_group.Text;
            this.CanVaryBtwGroups = (bool)this.chb_is_vary_btw_groups.IsChecked;
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            ParameterName = string.Empty;
            this.Close();
        }
    }
}

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

using Autodesk.Revit.DB;

namespace TektaRevitPlugins.Windows
{
    // Define a class to hold custom event info
    public class CpArgs : EventArgs
    {
        internal string CopyFromParam { get; private set; }
        internal string CopyToParam { get; private set; }

        internal CpArgs(string cpFrom, string cpTo) {
            CopyFromParam = cpFrom;
            CopyToParam = cpTo;
        }
    }

    /// <summary>
    /// Interaction logic for CpValuesWnd.xaml
    /// </summary>
    public partial class CpValuesWnd : Window
    {
        #region Data Fields
        SortedList<string, string> m_paramsValuesCopyFrom;
        List<string> m_paramsCopyTo;
        #endregion

        #region Events
        // Declare an event using EventHandler
        internal event EventHandler<CpArgs> OkClicked;

        void PassInfo() {
            string cpFrom = lb_copy_from.SelectedValue as string;
            string cpTo = lb_copy_to.SelectedValue as string;

            CpArgs args = new CpArgs(cpFrom, cpTo);
            OnRaiseOkClicked(args);
        }

        // Wrap event invocations inside a protected virtual method
        protected virtual void OnRaiseOkClicked(CpArgs e) {
            EventHandler<CpArgs> handler = OkClicked;

            if(handler != null) {
                handler(this, e);
            }
        }

        #endregion


        public CpValuesWnd(
            SortedList<string,string> paramsValuesCpFrom, 
            List<string> paramsCpTo) {
            m_paramsValuesCopyFrom = paramsValuesCpFrom;
            m_paramsCopyTo = paramsCpTo;
            InitializeComponent();

            // Assign elements to the select-from list box
            lb_copy_from.ItemsSource = m_paramsValuesCopyFrom.Keys;
            lb_copy_from.SelectedIndex = 0;
            txt_value.Text = m_paramsValuesCopyFrom[(string)lb_copy_from.SelectedItem];

            // Assign elements to the copy-to list box
            lb_copy_to.ItemsSource = paramsCpTo;
            lb_copy_to.SelectedItem = 0;
        }
        

        private void btn_cancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void lb_copy_from_SelectionChanged(
            object sender, SelectionChangedEventArgs e) {
            txt_value.Text = m_paramsValuesCopyFrom[(string)lb_copy_from.SelectedItem];
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e) {
            if (lb_copy_to.SelectedIndex == -1) {
                MessageBox.Show("Please select a parameter to which the value is to be copied.");
                return;
            }
            PassInfo();
            this.Close();
        }
    }
}

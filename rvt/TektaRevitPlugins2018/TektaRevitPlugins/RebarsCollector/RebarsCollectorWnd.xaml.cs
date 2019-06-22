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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

using ElementId = Autodesk.Revit.DB.ElementId;
using Document = Autodesk.Revit.DB.Document;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace TektaRevitPlugins
{
    // Define a class to hold the custom event info
    internal class RebarCollectorArgs : EventArgs
    {
        #region Properties
        internal string Partition { get; private set; }
        internal string HostMark { get; private set; }
        internal string AssemblyMark { get; private set; }
        internal Nullable<bool> IsSpecifiable { get; private set; }
        internal Nullable<bool> IsCalculable { get; private set; }
        internal Nullable<bool> IsAssembly { get; private set; }
        #endregion
        #region Constructors
        internal RebarCollectorArgs(
            string partition, string hostMark, string assemblyMark,
            bool? isSpecifiable, bool? isCalculable, bool? isAssembly)
        {
            Partition = partition;
            HostMark = hostMark;
            AssemblyMark = assemblyMark;
            IsSpecifiable = isSpecifiable;
            IsCalculable = isCalculable;
            IsAssembly = isAssembly;
        }
        #endregion

    }
    /// <summary>
    /// Interaction logic for RebarWnd.xaml
    /// </summary>
    public partial class RebarsCollectorWnd : Window
    {
        IDictionary<string, ISet<string>> m_partsHostMarks;
        IDictionary<string, ISet<string>> m_partsMarksAssemblies;

        #region Events
        // declare an event using EventHandler<T>
        internal event EventHandler<RebarCollectorArgs> ButtonClicked;

        // Event invocation function 
        virtual internal void OnButtonClicked(RebarCollectorArgs e)
        {
            EventHandler<RebarCollectorArgs> handler = ButtonClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        public RebarsCollectorWnd(IDictionary<string, ISet<string>> partsMarks,
            IDictionary<string, ISet<string>> partsMarksAssemblies)
        {
            InitializeComponent();

            m_partsHostMarks = partsMarks;
            m_partsMarksAssemblies = partsMarksAssemblies;

            // Populate the partitions
            cb_partitions.ItemsSource =
                m_partsHostMarks.Keys;

            // Fill up the host marks with values
            cb_host_marks.ItemsSource =
                m_partsMarksAssemblies.Values
                .SelectMany(s => s)
                .Distinct()
                .OrderBy(s => s);

            // Charge the assemblies
            cb_assemblies.ItemsSource = m_partsMarksAssemblies.Values
                .SelectMany(s => s)
                .Distinct()
                .OrderBy(s => s);
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            string partition = cb_partitions.IsEnabled ? cb_partitions.Text : null;
            string mark = cb_host_marks.IsEnabled ? cb_host_marks.Text : null;
            string assembly = cb_assemblies.IsEnabled ? cb_assemblies.Text : null;

            RebarCollectorArgs rebarArgs = new RebarCollectorArgs(
                partition,
                mark,
                assembly,
                chb_is_scheduled.IsChecked,
                chb_is_calculable.IsChecked,
                chb_is_assembly.IsChecked);

            OnButtonClicked(rebarArgs);

            Close();
        }

        private void cb_partitions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if ((string)this.cb_partitions.SelectedValue != null &&
            //    m_partsHostMarks[(string)this.cb_partitions.SelectedValue] != null)
            //{
            //    this.cb_host_marks.ItemsSource =
            //        m_partsHostMarks[(string)this.cb_partitions.SelectedValue];
            //}
            //else
            //{
            //    this.cb_host_marks.ItemsSource =
            //        m_partsHostMarks.Values
            //        .SelectMany(s => s).Distinct().OrderBy(s => s);
            //}

            //GetHostMarks();
            //GetAssemblies();

            //this.cb_host_marks.SelectedIndex = 0;
        }

        private void cb_host_marks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //GetAssemblies();
        }

        #region Helper Methods
        private void GetHostMarks()
        {
            ISet<string> hostMarks;
            if ((string)cb_partitions.SelectedValue != null &&
                m_partsHostMarks.TryGetValue(
                (string)cb_partitions.SelectedValue, out hostMarks))
            {
                cb_host_marks.ItemsSource = hostMarks;
            }
        }

        private void GetAssemblies()
        {
            if ((string)cb_partitions.SelectedValue != null &&
                (string)cb_host_marks.SelectedValue != null)
            {
                string partMark = (string)cb_partitions.SelectedValue +
                    (string)cb_host_marks.SelectedValue;
                ISet<string> assemblies;
                if (m_partsMarksAssemblies.TryGetValue(partMark, out assemblies))
                {
                    cb_host_marks.ItemsSource = assemblies;
                }
            }
        }

        void ToggleCombobox(MouseDevice mouse, ComboBox comboBox)
        {
            Point cursorPos = mouse.GetPosition(comboBox);
            Size size = comboBox.RenderSize;

            if (cursorPos.X >= 0 &&
                cursorPos.X <= size.Width &&
                cursorPos.Y >= 0 &&
                cursorPos.Y <= size.Height)
            {
                EnableDisableCombobox(comboBox);
            }
            //MessageBox.Show(string.Format(
            //    "{0} = ({1}, {2})\n[{3};{4}]",
            //    comboBox.Name, cursorPos.X, cursorPos.Y,
            //    size.Width, size.Height));
        }
        void EnableDisableCombobox(ComboBox comboBox)
        {
            if (comboBox.IsEnabled)
                comboBox.IsEnabled = false;
            else
                comboBox.IsEnabled = true;
        }
        #endregion

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToggleCombobox(e.MouseDevice, cb_partitions);
            ToggleCombobox(e.MouseDevice, cb_host_marks);
            ToggleCombobox(e.MouseDevice, cb_assemblies);
        }
    }
}

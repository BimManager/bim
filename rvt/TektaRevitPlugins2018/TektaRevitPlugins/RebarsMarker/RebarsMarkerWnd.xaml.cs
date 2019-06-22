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

namespace TektaRevitPlugins
{
    // Define a class to hold custom event info
    class RebarMarkerArgs : EventArgs
    {
        internal string Partition { get; set; }
        internal string HostMark { get; set; }
        internal SortedSet<string> Assemblies { get; set; }
    }


    /// <summary>
    /// Interaction logic for RebarsMarkerWnd.xaml
    /// </summary>
    public partial class RebarsMarkerWnd : Window
    {
        #region Data Fields
        IDictionary<string, ISet<string>> m_partsHostMarks;
        IDictionary<string, ISet<string>> m_hostMarksAssemblies;
        #endregion

        #region Propeties
        ObservableCollection<string> AvailableAssemblies { get; set; }
        ObservableCollection<string> SelectedAssemblies { get; set; }
        #endregion

        public RebarsMarkerWnd(IDictionary<string, ISet<string>> partsMarks,
            IDictionary<string, ISet<string>> marksAssemblies)
        {
            AvailableAssemblies = new ObservableCollection<string>();
            SelectedAssemblies = new ObservableCollection<string>();

            InitializeComponent();

            m_partsHostMarks = partsMarks;
            m_hostMarksAssemblies = marksAssemblies;

            cb_partitions.ItemsSource = m_partsHostMarks.Keys;
            cb_partitions.SelectedIndex = 0;

            lb_available.ItemsSource = AvailableAssemblies;
            lb_selected.ItemsSource = SelectedAssemblies;

            GetHostMarks();
            //GetAssemblies();
            GetAssembliesListBox();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            string part = (string)cb_partitions.SelectedValue;
            string mark = null;

            if (cb_host_marks.IsEnabled)
            {
                mark = (string)cb_host_marks.SelectedValue;
            }
            PassData(part, mark, SelectedAssemblies);

            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cb_partitions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetHostMarks();
            //GetAssemblies();
            GetAssembliesListBox();
        }

        private void cb_host_marks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //GetAssemblies();
            GetAssembliesListBox();
        }

        private void chb_assemblies_Checked(object sender, RoutedEventArgs e)
        {
            //cb_assemblies.IsEnabled = true;
        }

        private void chb_assemblies_Unchecked(object sender, RoutedEventArgs e)
        {
            //cb_assemblies.IsEnabled = false;
        }

        private void btn_in_Click(object sender, RoutedEventArgs e)
        {

            MoveInOut(Movement.IN);
        }

        private void btn_out_Click(object sender, RoutedEventArgs e)
        {
            MoveInOut(Movement.OUT);
        }

        private void gb_host_mark_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point cursorPos = e.MouseDevice.GetPosition(cb_host_marks);
            Size hmComboBoxSize = cb_host_marks.RenderSize;

            if (cursorPos.X >= 0 &&
                cursorPos.X <= hmComboBoxSize.Width &&
                cursorPos.Y >= 0 &&
                cursorPos.Y <= hmComboBoxSize.Height)
            {
                if (cb_host_marks.IsEnabled)
                    cb_host_marks.IsEnabled = false;
                else
                    cb_host_marks.IsEnabled = true;

                GetAssembliesListBox();
            }
        }

        #region Events
        // Declare an event using EventHandler
        internal event EventHandler<RebarMarkerArgs> OkClicked;

        void PassData(string part, string host, IList<string> assemblies)
        {
            EventHandler<RebarMarkerArgs> handler = OkClicked;

            if (handler != null)
            {
                // Raise the event
                handler(this, new RebarMarkerArgs
                {
                    Partition = part,
                    HostMark = host,
                    Assemblies = new SortedSet<string>(assemblies)
                });
            }
        }

        #endregion

        #region Helper Methods
        void GetHostMarks()
        {
            ISet<string> hostMarks;
            if (m_partsHostMarks.TryGetValue(
                (string)cb_partitions.SelectedValue,
                out hostMarks))
            {
                cb_host_marks.ItemsSource = hostMarks;
            }
            else
            {
                hostMarks = new HashSet<string>();
                cb_host_marks.ItemsSource = hostMarks;
            }
            cb_host_marks.SelectedIndex = 0;
        }
        //void GetAssemblies()
        //{
        //    string partMark =
        //            (string)cb_partitions.SelectedValue +
        //            (string)cb_host_marks.SelectedValue;

        //    ISet<string> assemblies;
        //    if (partMark != null && 
        //        m_hostMarksAssemblies.TryGetValue(
        //        partMark,
        //        out assemblies))
        //    {
        //        cb_assemblies.ItemsSource = assemblies;
        //    }
        //    else
        //    {
        //        cb_assemblies.ItemsSource = string.Empty;
        //    }
        //    cb_assemblies.SelectedIndex = 0;
        //}
        void GetAssembliesListBox()
        {
            if (cb_host_marks.IsEnabled)
            {
                AvailableAssemblies.Clear();

                string partMark =
                        (string)cb_partitions.SelectedValue +
                        (string)cb_host_marks.SelectedValue;

                ISet<string> assemblies;
                if (partMark != null &&
                    m_hostMarksAssemblies.TryGetValue(
                    partMark,
                    out assemblies))
                {
                    foreach (string asmbl in assemblies)
                    {
                        AvailableAssemblies.Add(asmbl);
                    }
                }
            }
            else
            {
                string part = (string)cb_partitions.SelectedValue;

                if (part != null)
                {
                    AvailableAssemblies.Clear();

                    ICollection<string> partsHosts = m_hostMarksAssemblies.Keys;
                    IEnumerator<string> itr = partsHosts.GetEnumerator();
                    while (itr.MoveNext())
                    {
                        if (itr.Current.StartsWith(part))
                        {
                            ISet<string> asmMarks = m_hostMarksAssemblies[itr.Current];

                            IEnumerator<string> it = asmMarks.GetEnumerator();
                            while (it.MoveNext())
                            {
                                AvailableAssemblies.Add(it.Current);
                            }
                        }
                    }
                }
            }
        }

        enum Movement { IN, OUT };
        void MoveInOut(Movement move)
        {
            System.Collections.IList selected = null;

            switch (move)
            {
                case Movement.IN:
                    selected = lb_available.SelectedItems;
                    break;
                case Movement.OUT:
                    selected = lb_selected.SelectedItems;
                    break;
            }

            if (selected != null)
            {
                //MessageBox.Show("Count: " + selected.Count);
                Array selElems = Array.CreateInstance(typeof(string), selected.Count);
                selected.CopyTo(selElems, 0);

                for (int i = 0; i < selElems.Length; ++i)
                {
                    switch (move)
                    {
                        case Movement.IN:
                            this.SelectedAssemblies.Add((string)selElems.GetValue(i));
                            this.AvailableAssemblies.Remove((string)selElems.GetValue(i));
                            break;
                        case Movement.OUT:
                            this.SelectedAssemblies.Remove((string)selElems.GetValue(i));
                            this.AvailableAssemblies.Add((string)selElems.GetValue(i));
                            break;
                    }
                }
            }
        }
        #endregion
    }
}

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

using TektaRevitPlugins.Multischedule;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for ScheduleCreatorWnd.xaml
    /// </summary>
    public partial class WndMultitableSchedule : Window
    {
        #region Field Data
        IDictionary<string, ISet<string>> m_partitions_hostMarks;
        IDictionary<string, ISet<string>> m_partsHostMarks_assemblies;
        IDictionary<string, ISet<string>> m_partitions_strTypes;
        #endregion

        public WndMultitableSchedule(
            IDictionary<string, ISet<string>> partsHostMarks,
            IDictionary<string, ISet<string>> hostsAssemblies,
            IDictionary<string, ISet<string>> partsStrTypes)
        {
            m_partitions_hostMarks = partsHostMarks;
            m_partsHostMarks_assemblies = hostsAssemblies;
            m_partitions_strTypes = partsStrTypes;

            InitializeComponent();

            // Fill up the schedule types combo box
            cb_schedules.ItemsSource = 
                Enum.GetValues(typeof(MultischeduleType));

            // Fill up the partitions combobox
            cb_partitions.ItemsSource = 
                m_partitions_hostMarks.Keys;

            // Fill up the host marks combobox
            GetHostMarks();

            // Fill up the structure types combobox
            GetStructureTypes();
            
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            FireEventAndPassValues();
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cb_partitions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_host_marks.ItemsSource =
                m_partitions_hostMarks[(string)cb_partitions.SelectedValue];
            cb_host_marks.SelectedIndex = 0;

            GetStructureTypes();
        }

        private void cb_host_marks_SelectionChanged
            (object sender, SelectionChangedEventArgs e)
        {
            GetAssemblies();
        }

        private void cb_schedules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((MultischeduleType)cb_schedules.SelectedValue) {
                case MultischeduleType.StructureSchedule:
                    cb_host_marks.IsEnabled = true;
                    cb_assemblies.IsEnabled = false;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    break;
                case MultischeduleType.AssemblySchedule:
                    cb_host_marks.IsEnabled = true;
                    cb_assemblies.IsEnabled = true;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = true;
                    GetAssemblies();
                    break;
                case MultischeduleType.BarBendingByStructure:
                    cb_host_marks.IsEnabled = true;
                    cb_assemblies.IsEnabled = false;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    break;
                case MultischeduleType.BarBendingByAssembly:
                    cb_host_marks.IsEnabled = true;
                    cb_assemblies.IsEnabled = true;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    GetAssemblies();
                    break;
                case MultischeduleType.RebarQuantityTakeoff:
                    cb_host_marks.IsEnabled = false;
                    cb_assemblies.IsEnabled = false;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    break;
                case MultischeduleType.ScheduleOfWork:
                    cb_host_marks.IsEnabled = false;
                    cb_assemblies.IsEnabled = false;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    break;
                case MultischeduleType.PartitionDrawingSets:
                    cb_host_marks.IsEnabled = false;
                    cb_assemblies.IsEnabled = false;
                    cb_structure_type.IsEnabled = false;
                    chb_concrete_quantity.IsEnabled = false;
                    break;
            }
        }

        #region Events
        // 1. Declare an event
        internal event EventHandler<ScheduleDataEventArgs> OkClicked;

        // 2. Wrap event invocation inside a protected virtual method
        // to allow derived classed to override the event 
        // invocation behaviour
        protected virtual void OnOkClicked(ScheduleDataEventArgs e)
        {
            if (OkClicked == null)
                return;

            // make a temp copy of the event
            EventHandler<ScheduleDataEventArgs> tempHandler =
                OkClicked;

            // Use the () operator to raise the event
            tempHandler(this, e);
        }

        // 3. Define a method for raising the event
        void FireEventAndPassValues()
        {
            IDictionary<string, string> fltrsVals =
                new Dictionary<string, string>();

            fltrsVals.Add(MultischeduleParameters.PARTITION, 
                (string)cb_partitions.SelectedValue);

            if (cb_host_marks.IsEnabled)
            {
                fltrsVals.Add(MultischeduleParameters.HOST_MARK,
                    (string)cb_host_marks.SelectedValue);
            }

            if (cb_assemblies.IsEnabled)
            {
                fltrsVals.Add(MultischeduleParameters.ASSEMBLY_MARK,
                    (string)cb_assemblies.SelectedValue);
            }

            // Extra features
            fltrsVals.Add(MultischeduleParameters.SHOW_TITLE,
                (bool)chb_show_title.IsChecked ? "1" : "0");
            if (chb_concrete_quantity.IsEnabled)
            {
                fltrsVals.Add(MultischeduleParameters.SHOW_CONCRETE_QUANTITY,
                    (bool)chb_concrete_quantity.IsChecked ? "1" : "0");
            }
            fltrsVals.Add(MultischeduleParameters.BLOCK_NUMBER, 
                ((ComboBoxItem)cb_block_num.SelectedItem).Content.ToString());
            if (cb_structure_type.IsEnabled)
            {
                fltrsVals.Add(MultischeduleParameters.STRUCTURE_TYPE,
                    (string)cb_structure_type.SelectedValue);
            }
            if ((MultischeduleType)cb_schedules.SelectedValue == 
                MultischeduleType.PartitionDrawingSets)
            {
                fltrsVals.Add(MultischeduleParameters.PROJECT_PARTITION, 
                    (string)cb_partitions.SelectedValue);
            }

            OnOkClicked(new ScheduleDataEventArgs {
                MultischeduleType = 
                (MultischeduleType)cb_schedules.SelectedValue,
                ParametersValues = fltrsVals
            });
        }
        #endregion

        #region Helper Methods
        void GetHostMarks()
        {
            ISet<string> hostMarks;
            if (m_partitions_hostMarks
                .TryGetValue((string)cb_partitions.SelectedValue, out hostMarks))
            {
                cb_host_marks.ItemsSource = hostMarks;
            }
            else
            {
                hostMarks = new SortedSet<string>();
                cb_host_marks.ItemsSource = hostMarks;
            }
        }

        void GetAssemblies()
        {
            ISet<string> assemblies;

            if ((ScheduleType)cb_schedules.SelectedItem ==
                ScheduleType.AssemblySchedule ||
                (ScheduleType)cb_schedules.SelectedItem ==
                ScheduleType.AssemblyBarBending)
            {
                string partitionHostMark = 
                    (string)cb_partitions.SelectedValue +
                    (string)cb_host_marks.SelectedValue;

                
                if (m_partsHostMarks_assemblies
                    .TryGetValue(partitionHostMark, out assemblies))
                {
                    cb_assemblies.IsEnabled = true;
                    cb_assemblies.ItemsSource = assemblies;
                    cb_assemblies.SelectedIndex = 0;
                }
                else
                {
                    assemblies = new SortedSet<string>();
                    cb_assemblies.ItemsSource = assemblies;
                    cb_assemblies.IsEnabled = false;
                }
            }
            else
            {
                assemblies = new SortedSet<string>();
                cb_assemblies.ItemsSource = assemblies;
                cb_assemblies.IsEnabled = false;
            }
        }

        void GetStructureTypes()
        {
            ISet<string> strTypes;
            if ((MultischeduleType)cb_schedules.SelectedValue == MultischeduleType.ScheduleOfWork)
            {
                if (m_partitions_strTypes
                    .TryGetValue((string)cb_partitions.SelectedValue, out strTypes))
                {
                    cb_structure_type.IsEnabled = true;
                    cb_structure_type.ItemsSource = strTypes;
                    cb_structure_type.SelectedIndex = 0;
                }
                else
                {
                    strTypes = new SortedSet<string>();
                    cb_structure_type.ItemsSource = strTypes;
                    cb_structure_type.IsEnabled = false;
                }
            }
            else
            {
                strTypes = new SortedSet<string>();
                cb_structure_type.IsEnabled = false;
            }
        }
        #endregion
    }

    public class ScheduleDataEventArgs: EventArgs
    {
        #region Properties
        internal MultischeduleType MultischeduleType { get; set; }
        internal IDictionary<string, string> ParametersValues { get; set; }
        #endregion

        #region Constructors
        internal ScheduleDataEventArgs() { }
        #endregion
    }
}

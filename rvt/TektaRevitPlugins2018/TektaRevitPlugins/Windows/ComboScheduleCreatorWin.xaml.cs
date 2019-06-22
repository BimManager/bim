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
    class RebarEventArgs: EventArgs
    {
        // Field Data
        IDictionary<string, string> m_paramsValues;
        ComboScheduleType m_scheduleType;

        // Properties
        internal IDictionary<string, string> Values {
            get { return m_paramsValues; }
        }
        internal ComboScheduleType ScheduleType {
            get { return m_scheduleType; }
        }

        // Constructors
        internal RebarEventArgs(IDictionary<string, string> paramsVals, ComboScheduleType scheduleType) {
            m_scheduleType = scheduleType;
            m_paramsValues = paramsVals;
        }

        // Methods
        internal string GetValue(int index)
        {
            return m_paramsValues.ElementAt(index).Value;
        }
    }
    /// <summary>
    /// Interaction logic for ComboScheduleCreatorWin.xaml
    /// </summary>
    public partial class ComboScheduleCreatorWin : Window
    {
        RebarContainer m_rebars;
        internal ComboScheduleType SelectedComboScheduleType { get; private set; }
        internal string SelectedPartition { get; private set; }
        internal string SelectedHostMark { get; private set; }
        internal string SelectedAssemblyMark { get; private set; }

        internal ComboScheduleCreatorWin(RebarContainer rebars)
        {
            InitializeComponent();
            m_rebars = rebars;
            this.cb_schedule_types.ItemsSource = 
                Enum.GetValues(typeof(ComboScheduleType));
            this.cb_partitions.ItemsSource = 
                m_rebars.GetPartitions();
            this.cb_host_marks.ItemsSource = 
                m_rebars.GetHostMarks((string)this.cb_partitions.SelectedValue);
            this.cb_assembly_marks.ItemsSource =
                m_rebars.GetAssemblies((string)this.cb_partitions.SelectedValue,
                (string)this.cb_host_marks.SelectedValue);
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedComboScheduleType = (ComboScheduleType)this.cb_schedule_types.SelectedItem;
            SelectedPartition = this.cb_partitions.SelectedItem.ToString();
            if(this.cb_host_marks.IsEnabled)
                SelectedHostMark = this.cb_host_marks.SelectedItem.ToString();
            if(this.cb_assembly_marks.IsEnabled)
                SelectedAssemblyMark = this.cb_assembly_marks.SelectedItem.ToString();
            this.Close();

            /*IDictionary<string, string> selectedValues 
                = new Dictionary<string, string>();
            selectedValues.Add(RebarsUtils.PARTITION, (string)this.cb_partitions.SelectedItem);
            selectedValues.Add(RebarsUtils.HOST_MARK, (string)this.cb_host_marks.SelectedItem);
            selectedValues.Add(RebarsUtils.ASSEMBLY_MARK, (string)this.cb_assembly_marks.SelectedItem);
            RebarEventArgs rebarArgs = 
                new RebarEventArgs(selectedValues, (ComboScheduleType)this.cb_schedule_types.SelectedItem);
            SubmitValues(rebarArgs);
            this.Close();*/
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled();
            this.Close();
        }

        private void cb_partitions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.cb_host_marks.ItemsSource =
               m_rebars.GetHostMarks((string)this.cb_partitions.SelectedValue);
            this.cb_host_marks.SelectedIndex = 0;
        }
        
        private void cb_host_marks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.cb_assembly_marks.ItemsSource =
                m_rebars.GetAssemblies((string)this.cb_partitions.SelectedValue,
                (string)this.cb_host_marks.SelectedValue);
            this.cb_assembly_marks.SelectedIndex = 0;
        }

        private void cb_schedule_types_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboScheduleType selected = (ComboScheduleType)this.cb_schedule_types.SelectedItem;
            switch (selected) {
                case ComboScheduleType.StructuralElementSchedule:
                    this.cb_assembly_marks.IsEnabled = false;
                    break;
                case ComboScheduleType.SheetList:
                    this.cb_host_marks.IsEnabled = false;
                    this.cb_assembly_marks.IsEnabled = false;
                    break;
                case ComboScheduleType.MaterialTakeOff:
                    this.cb_host_marks.IsEnabled = false;
                    this.cb_assembly_marks.IsEnabled = false;
                    break;
                case ComboScheduleType.BarBendingStrElement:
                    this.cb_assembly_marks.IsEnabled = false;
                    break;
                case ComboScheduleType.AssemblySchedule:
                    this.cb_host_marks.IsEnabled = true;
                    this.cb_assembly_marks.IsEnabled = true;
                    break;
                case ComboScheduleType.BarBendingAssembly:
                    this.cb_host_marks.IsEnabled = true;
                    this.cb_assembly_marks.IsEnabled = true;
                    break;
                case ComboScheduleType.LabourSchedule:
                    this.cb_host_marks.IsEnabled = false;
                    this.cb_assembly_marks.IsEnabled = false;
                    break;
            }           
        }

        #region Events
        // Declare an event using an EventHandler<T>
        internal event EventHandler WinClosed;
        internal event EventHandler<RebarEventArgs> PassValues;

        // Raise the event
        internal void Canceled()
        {
            OnWinClosed(new EventArgs());
        }
        
        internal void SubmitValues(RebarEventArgs args)
        {
            OnPassValues(args);
        }

        // Wrap event invocations inside a protected virtual method
        protected virtual void OnWinClosed(EventArgs e)
        {
            // make a temporary copy of the event
            EventHandler handler = WinClosed;

            // event will be null if there is no subscriber
            if(handler != null) {
                // raise the event
                handler(this, e);
            }
        }

        internal void OnPassValues(RebarEventArgs args)
        {
            // make a temporary copy of the event
            EventHandler<RebarEventArgs> handler = PassValues;
            if(handler != null) {
                // raise the event
                handler(this, args);
            }
        }
        #endregion
    }
}

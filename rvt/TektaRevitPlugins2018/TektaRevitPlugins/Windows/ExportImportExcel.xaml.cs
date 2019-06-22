using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using RvtSchedule = Autodesk.Revit.DB.ViewSchedule;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for ExportImportExcel.xaml
    /// </summary>
    public partial class ExportImportExcel : Window
    {
        #region Constructors
        public ExportImportExcel(IList<RvtSchedule> schedules)
        {
            InitializeComponent();
            // preselect the output folder
            this.filepath.Text =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // populate the ListBox
            this.schedules.ItemsSource = schedules;
        }
        #endregion

        #region Event Handlers
        private void browse_btn_Click(object sender, RoutedEventArgs e)
        {
            // Let the user select the export directory
            System.Windows.Forms.FolderBrowserDialog fbd =
                new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult result =
                fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) {
                this.filepath.Text = fbd.SelectedPath;
            }
            else
                MessageBox.Show
                    ("Something has gone wrong.\nUnable to obtain the path.");
        }

        private void checkall_btn_Click(object sender, RoutedEventArgs e)
        {
            // When invoked, the command will select all the checkBoxes
            foreach (CheckBox cb in GetAllCheckBoxes(this.schedules, "checkBox_schedules")) {
                cb.IsChecked = true;
            }
        }

        private void checknone_btn_Click(object sender, RoutedEventArgs e)
        {
            // The opposite to the check_btn_Click
            foreach (CheckBox cb in GetAllCheckBoxes(this.schedules, "checkBox_schedules")) {
                cb.IsChecked = false;
            }
        }

        private void export_btn_Click(object sender, RoutedEventArgs e)
        {
            // acquire the schedules chosen 
            IList<RvtSchedule> pickedSchedules =
                GetSelectedSchedules(this.schedules, "checkBox_schedules");

            // take care to proceed only if at least one checkbox is ticked
            if (pickedSchedules.Count == 0) {
                MessageBox.Show("Please pick at least one schedule.");
                return;
            }

            // export the schedules as one file or a set of files
            if ((bool)this.oneFile_rbtn.IsChecked)
                ExcelUtils.ExportToExcel(pickedSchedules, this.filepath.Text);
            else {
                foreach (RvtSchedule schedule in pickedSchedules) {
                    ExcelUtils.ExportToExcel(schedule, this.filepath.Text);
                }
            }
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            this.Close();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Unearth a particular child of the control provided
        /// </summary>
        /// <typeparam name="ChildControl"></typeparam>
        /// <param name="obj"></param>
        /// <returns>A child of the control</returns>
        internal static ChildControl FindVisualChild<ChildControl>(DependencyObject obj)
            where ChildControl : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ChildControl) {
                    return (ChildControl)child;
                }
                else {
                    ChildControl childOfChild =
                        FindVisualChild<ChildControl>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private static IList<CheckBox> GetAllCheckBoxes(ListBox listBox, string checkBoxName)
        {
            IList<CheckBox> checkBoxes = new List<CheckBox>();

            for (int i = 0; i < listBox.Items.Count; i++) {

                ListBoxItem listBoxItem = (ListBoxItem)listBox
                    .ItemContainerGenerator.ContainerFromIndex(i);

                if (listBoxItem == null)
                    continue;

                ContentPresenter contentPresenter =
                    FindVisualChild<ContentPresenter>(listBoxItem);

                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox =
                    (CheckBox)dataTemplate.FindName(checkBoxName, contentPresenter);

                checkBoxes.Add(checkBox);
            }

            return checkBoxes;
        }

        private static IList<RvtSchedule> GetSelectedSchedules(ListBox listBox, string checkBoxName)
        {
            IList<RvtSchedule> pickedSchedules = new List<RvtSchedule>();

            for (int i = 0; i < listBox.Items.Count; i++) {

                ListBoxItem listBoxItem = (ListBoxItem)listBox
                    .ItemContainerGenerator.ContainerFromIndex(i);

                if (listBoxItem == null)
                    continue;

                ContentPresenter contentPresenter =
                    FindVisualChild<ContentPresenter>(listBoxItem);

                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox =
                    (CheckBox)dataTemplate.FindName(checkBoxName, contentPresenter);

                if ((bool)checkBox.IsChecked) {
                    pickedSchedules.Add((RvtSchedule)listBox.Items[i]);
                }
            }
            return pickedSchedules;
        }
        #endregion
    }
}

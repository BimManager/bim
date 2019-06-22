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

using System.Data;

namespace TektaRevitPlugins
{
    /// <summary>
    /// Interaction logic for RoomProperties.xaml
    /// </summary>
    public partial class RoomProperties : Window
    {
        static DataTable dataTable;
        public IDictionary<string, string> Values { get; private set; }

        public RoomProperties()
        {
            InitializeComponent();

            //DataSet dataSet = ExcelUtils.GetExcelDataAsDataSet();
            DataSet dataSet = ExcelUtils
                .LoadAsBinary
                (@"Z:\Департамент проектирования\private\ПРОЕКТЫ\Нежинская\BIM\01-WIP\Families\NEZH_Rooms_2018.bin");
            dataTable = dataSet.Tables[0];

            for (int i = 0; i < dataTable.Columns.Count; i++) {
                if (dataTable.Columns[i].ColumnName.StartsWith("F") ||
                    dataTable.Columns[i].ColumnName.Contains("ID"))
                    continue;

                IList<string> values = new List<string>();

                for (int j = 0; j < dataTable.Rows.Count; j++) {
                    object[] rowItems = 
                        dataTable.Rows[j].ItemArray;

                    if (rowItems[i].ToString().Length == 0)
                        continue;
                    else
                        values.Add(rowItems[i].ToString());
                }

                // Add the current column's title as a label to the stack panel
                this.stackPanel.Children
                    .Add(new Label { Content = dataTable.Columns[i].ColumnName });

                ComboBox comboBox = new ComboBox();
                comboBox.Name = dataTable.Columns[i].ColumnName;
                comboBox.ItemsSource = values;
                comboBox.SelectedIndex = 0;
                comboBox.SelectionChanged += comboBox_SelectedChanged;

                this.stackPanel.Children.Add(comboBox);
            }
        }

        private void comboBox_SelectedChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            foreach (UIElement uiElem in this.wrapper.Children) {
                TextBlock textBlock = uiElem as TextBlock;
                if (textBlock.Name == comboBox.Name) {
                    DataColumnCollection columns = dataTable.Columns;
                    DataColumn column = dataTable.Columns[comboBox.Name];
                    int j = columns.IndexOf(column);
                    textBlock.Text = dataTable.Rows[comboBox.SelectedIndex].ItemArray[j + 1].ToString();
                }
            }
        }

        private void ok_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Values = GetValues(this.stackPanel);
            this.Close();
        }

        private void cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Values = null;
            this.Close();
        }

        IDictionary<string, string> GetValues(Panel panel)
        {
            IDictionary<string, string> values =
                new Dictionary<string, string>();

            foreach (DependencyObject dObj in panel.Children) {
                if (dObj is ComboBox) {
                    ComboBox comboBox = dObj as ComboBox;
                    DataColumn idCol = dataTable.Columns[dataTable.Columns[comboBox.Name].Ordinal + 1];
                    DataRow idRow = dataTable.Rows[comboBox.SelectedIndex];
                    values.Add(comboBox.Name, comboBox.SelectedValue.ToString());
                    values.Add(idCol.ColumnName, idRow.ItemArray[idCol.Ordinal].ToString());
                }
            }
            return values;
        }

    }
}

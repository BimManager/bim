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

namespace TektaRevitPlugins.Windows
{
    // Class for passing on some data
    // via events
    public class EnumValuesArgs : EventArgs
    {
        #region Properties
        public string Prefix { get; private set; }
        public int Position { get; private set; }
        public string Suffix { get; private set; }
        public string ParameterName { get; private set; }
        #endregion
        #region Constructor
        public EnumValuesArgs(string prefix, int position, string suffix, string paramName)
        {
            Prefix = prefix;
            Position = position;
            Suffix = suffix;
            ParameterName = paramName;
        }
        #endregion
    }

    /// <summary>
    /// Interaction logic for EnumValuesWnd.xaml
    /// </summary>
    public partial class EnumValuesWnd : Window
    {
        #region Events
        // Declare the event using EventHandler<T>
        public event EventHandler<EnumValuesArgs> OkClicked;

        // Delcare the event that raises the event
        public bool PassValue()
        {
            int position;
            if (Int32.TryParse(txt_position.Text, out position))
            {
                OnOkClicked(
                    txt_prefix.Text, 
                    position, 
                    txt_suffix.Text, 
                    (string)cb_parameter_names.SelectedValue
                    );
                return true;
            }
            else
            {
                MessageBox.Show("Only integers are permitted in the Position field");
                return false;
            }
        }

        // Wrap event invocations indside a protected virtual method
        protected virtual void OnOkClicked(string prefix, int position, string suffix, string paramName)
        {
            // Make a temporary copy of the event to avoid 
            // the possibility of a race condition if the last 
            // subscriber unsubscribes
            EventHandler<EnumValuesArgs> handler = OkClicked;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Fill in the custom argument class
                EnumValuesArgs args = new EnumValuesArgs(prefix, position, suffix, paramName);

                // Use the () operator to raise the event
                handler(this, args);
            }
        }
        #endregion

        public EnumValuesWnd(IList<string> paramNames)
        {
            InitializeComponent();
            cb_parameter_names.ItemsSource = paramNames;
            cb_parameter_names.SelectedIndex = 0;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            // Raise the event
            if (PassValue())
            {
                this.Close();
            }
        }

        private void btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

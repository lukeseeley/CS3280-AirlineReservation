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

namespace AirlineReservation
{
    /// <summary>
    /// Interaction logic for Passenger_Info.xaml
    /// </summary>
    public partial class Passenger_Info : Window
    {
        #region Methods
        public Passenger_Info()
        {
            InitializeComponent();
            txtFirstName.Text = "";
            txtLastName.Text = "";
        }

        /// <summary>
        /// This button will process the input and then close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// This method will hide the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        #endregion
    }
}

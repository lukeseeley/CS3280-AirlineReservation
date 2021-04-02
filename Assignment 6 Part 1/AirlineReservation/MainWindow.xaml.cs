using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AirlineReservation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        private Passenger_Info Passenger_Info;

        /// <summary>
        /// The database access class for Flight data
        /// </summary>
        private Flights flights;

        /// <summary>
        /// The database access class for Passenger data
        /// </summary>
        private Passengers passengers;

        /// <summary>
        /// This stores a copy of the currently selected passenger
        /// </summary>
        private Passenger selectedPassenger;

        /// <summary>
        /// This stores a 2 char string of the currently selected seat
        /// </summary>
        private string selectedSeat;

        /// <summary>
        /// Stores the currently selected flight
        /// </summary>
        private int flight;

        /// <summary>
        /// This is a bool flag to prevent the seat click event from triggering the cbx event from triggering
        /// </summary>
        private bool isChangingSeat;

        //////////////////// Assets
        /// <summary>
        /// The brush asset for a seat that is taken (Red)
        /// </summary>
        private Brush SeatTaken = new SolidColorBrush(Color.FromRgb(185, 0, 0));
        /// <summary>
        /// The brush asset for a seat that is not taken (Blue)
        /// </summary>
        private Brush SeatEmpty = new SolidColorBrush(Color.FromRgb(0, 85, 185));
        /// <summary>
        /// The brush asset for a seat that is the currently selected passenger (Green)
        /// </summary>
        private Brush SeatSelected = new SolidColorBrush(Color.FromRgb(50, 185, 0));

        #endregion

        #region Methods

        /// <summary>
        /// The Constructor method for the Main Window, also initializes starting UI state
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            flights = new Flights();
            passengers = new Passengers();

            //Initialize UI state
            grdFlightMap118.Visibility = Visibility.Hidden;
            grdFlightMap583.Visibility = Visibility.Hidden;
            btnAddPassenger.IsEnabled = false;
            btnChangeSeat.IsEnabled = false;
            btnDeletePassanger.IsEnabled = false;
            txtSeat.Text = "";
            txtSeat.IsEnabled = false;
            cbxPassenger.IsEnabled = false;

            //Bind Flight Combo box
            try
            {
                cbxFlight.ItemsSource = flights.GetFlights();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Warning! An Exception has occurred.\n" + MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is for getting the associated seat element for UI updates.
        /// This is intended for use of getting which seat to mark as selected.
        /// </summary>
        /// <param name="seatName"></param>
        /// <returns>A Border object for the seat being searched for</returns>
        private Border getSeat(string seatName)
        {
            try
            {
                if (flight == 118)
                {
                    string seatRowNum; 
                    foreach (Border seat in grdSeating118.Children)
                    {
                        seatRowNum = seat.Name.Substring(seat.Name.Length - 2, 2);
                        if (seatName == seatRowNum)
                        {
                            return seat;
                        }
                    }

                    throw new Exception("Seat not found");
                }
                else if (flight == 583)
                {
                    string seatRowNum;
                    foreach (Border seat in grdSeating583.Children)
                    {
                        seatRowNum = seat.Name.Substring(seat.Name.Length - 2, 2);
                        if (seatName == seatRowNum)
                        {
                            return seat;
                        }
                    }

                    throw new Exception("Seat not found");
                }
                
                throw new Exception("Unexpected Flight");
                
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method will render the seat map based on the flight data, marking seats as taken or empty.
        /// This will not determine which seat has been selected
        /// </summary>
        private void renderSeats()
        {
            try
            {
                if (flight == 118)
                {
                    foreach (Border seat in grdSeating118.Children)
                    {
                    
                        string row = seat.Name.Substring(seat.Name.Length - 2, 1);
                        int num = Int32.Parse(seat.Name.Substring(seat.Name.Length - 1, 1));

                        //Check the seat's availability
                        if(passengers.checkSeat(row, num)) //Then the seat is taken
                        {
                            seat.Background = SeatTaken;
                        }
                        else
                        {
                            seat.Background = SeatEmpty;
                        }
                    }
                }
                else if (flight == 583)
                {
                    foreach (Border seat in grdSeating583.Children)
                    {
                        string row = seat.Name.Substring(seat.Name.Length - 2, 1);
                        int num = Int32.Parse(seat.Name.Substring(seat.Name.Length - 1, 1));

                        //Check the seat's availability
                        if (passengers.checkSeat(row, num)) //Then the seat is taken
                        {
                            seat.Background = SeatTaken;
                        }
                        else
                        {
                            seat.Background = SeatEmpty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Warning! An Exception has occurred.\n" + MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This opens up the window for creating a new passenger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddPassenger__Click(object sender, RoutedEventArgs e)
        {
            Passenger_Info = new Passenger_Info();
            Passenger_Info.ShowDialog();
        }

        /// <summary>
        /// This method is the event handler for when a new flight is selected from the flight selection box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxFlight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                flight = Int32.Parse(cbxFlight.SelectedItem.ToString().Substring(0, 3));
                passengers.retrieveData(flight);

                if (flight == 118)
                {
                    grdFlightMap583.Visibility = Visibility.Hidden;
                    grdFlightMap118.Visibility = Visibility.Visible;
                }
                else if(flight == 583)
                {
                    grdFlightMap118.Visibility = Visibility.Hidden;
                    grdFlightMap583.Visibility = Visibility.Visible;
                }
            
                cbxPassenger.ItemsSource = passengers.GetPassengers();
                renderSeats();

                //Reset UI tracking
                cbxPassenger.SelectedIndex = -1;
                selectedSeat = null;
                selectedPassenger = null;

                //Enable UI
                btnAddPassenger.IsEnabled = true;
                cbxPassenger.IsEnabled = true;
                txtSeat.IsEnabled = false;
                txtSeat.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Warning! An Exception has occurred.\n" + MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is the event handler for when a new passenger is selected from the flight selection box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxPassenger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isChangingSeat)
                {
                    return; //As the change was caused by the seatClick event
                }
                if (cbxPassenger.SelectedItem == null)
                {
                    return; //As the selection box is empty
                }

                Passenger newPassenger = new Passenger(cbxPassenger.SelectedItem.ToString().Split(' ')[0], cbxPassenger.SelectedItem.ToString().Split(' ')[1]);
                string newSeat = passengers.getPassengerSeat(newPassenger);
                txtSeat.IsEnabled = true;
                txtSeat.Text = newSeat;

                if (selectedPassenger != null && newPassenger == selectedPassenger)
                {
                    return; //As nothing has changed
                }

                if (selectedSeat != null)
                {
                    getSeat(selectedSeat).Background = SeatTaken; //Then unselect the last seat
                }

                getSeat(newSeat).Background = SeatSelected;

                selectedPassenger = newPassenger;
                selectedSeat = newSeat;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Warning! An Exception has occurred.\n" + MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is for handling an event for when a seat (Border) is clicked on, which will then select that seat if it is taken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void seatClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Border seat = (Border)sender;
                string seatName = seat.Name.Substring(seat.Name.Length - 2, 2);
                string row = seat.Name.Substring(seat.Name.Length - 2, 1);
                int num = Int32.Parse(seat.Name.Substring(seat.Name.Length - 1, 1));

                if (passengers.checkSeat(row, num))
                {
                    //Then this seat is taken
                    if (seatName != selectedSeat)
                    {
                        //Then this seat is not already selected
                        //Get passenger for that seat
                        Passenger newPassenger = passengers.getPassengerBySeat(row, num);
                        int index = passengers.getPassengerIndex(newPassenger);

                        //This is kind of like a mutex lock for multi-threading
                        isChangingSeat = true;
                        //Update UI
                        cbxPassenger.SelectedIndex = index;
                        isChangingSeat = false;

                        txtSeat.IsEnabled = true;
                        txtSeat.Text = seatName;

                        getSeat(seatName).Background = SeatSelected;
                        if (selectedSeat != null)
                        {
                            getSeat(selectedSeat).Background = SeatTaken;
                        }

                        //Now update selection variables
                        selectedPassenger = newPassenger;
                        selectedSeat = seatName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Warning! An Exception has occurred.\n" + MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        #endregion
    }
}

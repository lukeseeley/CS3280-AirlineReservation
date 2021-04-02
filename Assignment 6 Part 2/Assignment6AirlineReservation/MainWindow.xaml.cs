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

namespace Assignment6AirlineReservation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        /// <summary>
        /// Window for adding passengers
        /// </summary>
        wndAddPassenger wndAddPass;

        /// <summary>
        /// The database access class for Flight data
        /// </summary>
        Flights flights;

        /// <summary>
        /// The database access class for Passenger data
        /// </summary>
        Passengers passengers;

        /// <summary>
        /// This stores a copy of the currently selected passenger
        /// </summary>
        private Passenger selectedPassenger;

        /// <summary>
        /// This stores a 2 char string of the currently selected seat
        /// </summary>
        private int selectedSeat;

        /// <summary>
        /// Stores the currently selected flight
        /// </summary>
        private int flight;

        /// <summary>
        /// This is a bool flag to prevent the seat click event from triggering the cb event from triggering
        /// </summary>
        private bool isChangingSeat;

        /// <summary>
        /// This is a bool flag to handle when the user is actively selecting a new seat for a new passenger
        /// </summary>
        private bool seatforNewPassenger;

        /// <summary>
        /// This is a bool flag to handle when the user is actively selecting a new seat for an existing passenger
        /// </summary>
        private bool changingSeat;

        #endregion

        #region Methods
        /// <summary>
        /// This is the main window constructor method, which also sets UI's initial state
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                flights = new Flights();
                passengers = new Passengers();

                cbChooseFlight.ItemsSource = flights.GetFlights();

                //Set Initial UI state
                CanvasA380.Visibility = Visibility.Hidden;
                Canvas767.Visibility = Visibility.Hidden;
                lblSelectSeatMessage.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// This is an event handler for the flight combo box Selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbChooseFlight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                flight = Int32.Parse(cbChooseFlight.SelectedItem.ToString().Substring(0, 3));
                passengers.retrieveData(flight);

                if (flight == 102)
                {
                    Canvas767.Visibility = Visibility.Hidden;
                    CanvasA380.Visibility = Visibility.Visible;
                }
                else if (flight == 412)
                {
                    CanvasA380.Visibility = Visibility.Hidden;
                    Canvas767.Visibility = Visibility.Visible;
                }

                cbChoosePassenger.ItemsSource = passengers.GetPassengers();
                renderSeats();

                //Reset UI elements
                cbChoosePassenger.IsEnabled = true;
                cmdAddPassenger.IsEnabled = true;
                cmdChangeSeat.IsEnabled = false;
                cmdDeletePassenger.IsEnabled = false;

                cbChoosePassenger.SelectedIndex = -1;
                selectedSeat = 0;
                selectedPassenger = null;
                lblPassengersSeatNumber.Content = "";
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// This is an event handler for the passenger combo box Selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbChoosePassenger_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isChangingSeat || cbChoosePassenger.SelectedItem == null)
                {
                    return; //As there is nothing further to be done
                }

                Passenger newPassenger = new Passenger(cbChoosePassenger.SelectedItem.ToString().Split(' ')[0], cbChoosePassenger.SelectedItem.ToString().Split(' ')[1]);
                int newSeat = passengers.getPassengerSeatNum(newPassenger);
                lblPassengersSeatNumber.Content = newSeat;

                if (selectedPassenger != null && newPassenger == selectedPassenger)
                {
                    return; //As nothing has changed
                }

                if (selectedSeat != 0)
                {
                    getSeat(selectedSeat).Background = Brushes.Red; //Then unselect the last seat
                }

                getSeat(newSeat).Background = Brushes.Lime;

                selectedPassenger = newPassenger;
                selectedSeat = newSeat;

                //Enable AI
                cmdChangeSeat.IsEnabled = true;
                cmdDeletePassenger.IsEnabled = true;
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Even handler for when a seat is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdSeat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Label seat = (Label)sender;

                int seatNum;
                if (flight == 102)
                {
                    seatNum = Int32.Parse(seat.Name.Substring(5));
                }
                else if (flight == 412)
                {
                    seatNum = Int32.Parse(seat.Name.Substring(4));
                }
                else
                {
                    return;
                }

                if(seatforNewPassenger) //Then the user is trying to select a seat for a new passenger
                {
                    if(!passengers.checkSeat(seatNum)) //The seat must be empty
                    {
                        passengers.addNewPassenger(selectedPassenger, seatNum);

                        //Now update seating
                        Passenger newPassenger = passengers.getPassengerBySeat(seatNum);
                        int index = passengers.getPassengerIndex(newPassenger);


                        isChangingSeat = true; //Set lock while updating UI
                        //Update UI
                        cbChoosePassenger.SelectedIndex = index;
                        isChangingSeat = false; //Release lock
                        seatforNewPassenger = false;

                        renderSeats();
                        lblPassengersSeatNumber.Content = seatNum;
                        getSeat(seatNum).Background = Brushes.Lime;

                        selectedPassenger = newPassenger;
                        selectedSeat = seatNum;

                        //Enable UI
                        gbPassengerInformation.IsEnabled = true;
                        gPassengerCommands.IsEnabled = true;
                        lblSelectSeatMessage.Visibility = Visibility.Hidden;
                    }
                }
                else if(changingSeat) // Then the user is trying to change the seat of a user
                {
                    if (!passengers.checkSeat(seatNum)) //The seat must be empty
                    {
                        passengers.changePassengerSeat(selectedPassenger, seatNum);

                        //Now update seating
                        Passenger newPassenger = passengers.getPassengerBySeat(seatNum);
                        int index = passengers.getPassengerIndex(newPassenger);


                        isChangingSeat = true; //Set lock while updating UI
                        //Update UI
                        cbChoosePassenger.SelectedIndex = index;
                        isChangingSeat = false; //Release lock
                        changingSeat = false;

                        lblPassengersSeatNumber.Content = seatNum;

                        renderSeats();
                        getSeat(seatNum).Background = Brushes.Lime;

                        selectedPassenger = newPassenger;
                        selectedSeat = seatNum;

                        //Enable UI
                        gbPassengerInformation.IsEnabled = true;
                        gPassengerCommands.IsEnabled = true;
                        lblSelectSeatMessage.Visibility = Visibility.Hidden;
                    }
                }
                else //Then we are not trying to select a seat
                {
                    if(passengers.checkSeat(seatNum)) //Then this seat is taken
                    {
                        if (seatNum != selectedSeat) //Then it is not already selected
                        {
                            //Then get passenger data
                            Passenger newPassenger = passengers.getPassengerBySeat(seatNum);
                            int index = passengers.getPassengerIndex(newPassenger);

                            
                            isChangingSeat = true; //Set lock while updating UI
                            //Update UI
                            cbChoosePassenger.SelectedIndex = index;
                            isChangingSeat = false; //Release lock

                            lblPassengersSeatNumber.Content = seatNum;

                            if (selectedSeat != 0)
                            {
                                getSeat(selectedSeat).Background = Brushes.Red;
                            }
                            
                            getSeat(seatNum).Background = Brushes.Lime;

                            selectedPassenger = newPassenger;
                            selectedSeat = seatNum;

                            //Enable UI
                            cmdChangeSeat.IsEnabled = true;
                            cmdDeletePassenger.IsEnabled = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Event handler for when the new passenger button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAddPassenger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wndAddPass = new wndAddPassenger(ref passengers);
                wndAddPass.ShowDialog();

                Passenger newPassenger = wndAddPass.newPassenger;

                if(newPassenger == null)
                {
                    return; //Because no new passenger was created
                }
                else
                {
                    selectedPassenger = newPassenger;
                }

                //Now we need to get a seat for said passenger
                seatforNewPassenger = true;

                //Update UI
                lblSelectSeatMessage.Visibility = Visibility.Visible;
                gPassengerCommands.IsEnabled = false;
                gbPassengerInformation.IsEnabled = false;
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void cmdChangeSeat_Click(object sender, RoutedEventArgs e)
        {
            //Now we need to have the user select which seat they want to give the user
            changingSeat = true;
            
            //Update UI
            lblSelectSeatMessage.Visibility = Visibility.Visible;
            gPassengerCommands.IsEnabled = false;
            gbPassengerInformation.IsEnabled = false;
        }

        private void cmdDeletePassenger_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Method for rendering the seat map based on flight data, to mark seats that are taken or empty
        /// This will not determine selected seats
        /// </summary>
        private void renderSeats()
        {
            try
            {
                if (flight == 102)
                {
                    foreach (Label seat in cA380_Seats.Children)
                    {
                        int seatNum = Int32.Parse(seat.Name.Substring(5));

                        //Check the seat's availability
                        if(passengers.checkSeat(seatNum))
                        {
                            seat.Background = Brushes.Red;
                        }
                        else
                        {
                            seat.Background = Brushes.Blue;
                        }
                    }
                }
                else if (flight == 412)
                {
                    foreach (Label seat in c767_Seats.Children)
                    { 
                        int seatNum = Int32.Parse(seat.Name.Substring(4));

                        //Check the seat's availability
                        if (passengers.checkSeat(seatNum))
                        {
                            seat.Background = Brushes.Red;
                        }
                        else
                        {
                            seat.Background = Brushes.Blue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name,
                    MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// This method is for getting the associated seat in the UI for the requested seat number
        /// </summary>
        /// <param name="seatNum"></param>
        /// <returns></returns>
        private Label getSeat(int seatNum)
        {
            try
            {
                if (flight == 102)
                {
                    foreach(Label seat in cA380_Seats.Children)
                    {
                        if (seatNum == Int32.Parse(seat.Name.Substring(5)))
                        {
                            return seat;
                        }
                    }

                    throw new Exception("Seat not found");
                }
                else if (flight == 412)
                {
                    foreach (Label seat in c767_Seats.Children)
                    {
                        if (seatNum == Int32.Parse(seat.Name.Substring(4)))
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
        /// A method for handling when an exception is thrown somewhere in the program
        /// </summary>
        /// <param name="sClass"></param>
        /// <param name="sMethod"></param>
        /// <param name="sMessage"></param>
        private void HandleError(string sClass, string sMethod, string sMessage)
        {
            try
            {
                MessageBox.Show(sClass + "." + sMethod + " -> " + sMessage);
            }
            catch (System.Exception ex)
            {
                System.IO.File.AppendAllText(@"C:\Error.txt", Environment.NewLine + "HandleError Exception: " + ex.Message);
            }
        }
        #endregion
    }
}

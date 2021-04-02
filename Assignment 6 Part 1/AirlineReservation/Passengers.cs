using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AirlineReservation
{
    class Passengers
    {
        #region Attributes
        /// <summary>
        /// This is the data access class for accessing the database
        /// </summary>
        private DataAccess db;

        private DataSet PassengerSet;

        private DataSet PassengerFlightSet;

        private int flightNumber;

        private int flightId;

        /// <summary>
        /// An Observable Collection for retrieval of passenger data
        /// </summary>
        private ObservableCollection<Passenger> passengerCollection;

        #endregion

        #region Methods
        public Passengers()
        {
            db = new DataAccess();
            passengerCollection = new ObservableCollection<Passenger>();
        }

        /// <summary>
        /// This loads in the datasets from the database
        /// </summary>
        /// <param name="flightNum"></param>
        public void retrieveData(int flightNum)
        {
            flightNumber = flightNum;
            try
            {
                string SQL = "SELECT Flight_ID FROM Flights WHERE Flight_Number = " + flightNumber;
                flightId = Int32.Parse(db.ExecuteScalarSQL(SQL));

                SQL = "SELECT * FROM Passengers";
                int rows = 0;
                PassengerSet = db.ExecuteSQL(SQL, ref rows);

                SQL = "SELECT * FROM Flight_Passengers WHERE Flight_ID = " + flightId;
                PassengerFlightSet = db.ExecuteSQL(SQL, ref rows);
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// Finds the passenger's ID corresponding to the given Person object
        /// </summary>
        /// <param name="passenger"></param>
        /// <returns>The Passenger id integer</returns>
        private int getPassengerId(Passenger passenger)
        {
            try
            {
                foreach (DataRow row in PassengerSet.Tables[0].Rows)
                {
                    if (row[1].ToString() == passenger.FirstName && row[2].ToString() == passenger.LastName)
                    {
                        return Int32.Parse(row[0].ToString());
                    }
                }

                throw new Exception("Passenger not found");
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        private Passenger getPassengerById(int id)
        {
            try
            {
                foreach (DataRow row in PassengerSet.Tables[0].Rows)
                {
                    if (Int32.Parse(row[0].ToString()) == id)
                    {
                        return new Passenger(row[1].ToString(), row[2].ToString());
                    }
                }

                throw new Exception("Passenger not found");
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// Returns a list of the passenger integer IDs that are in the PassengerFlight DataSet
        /// </summary>
        /// <returns>List of integer IDs</returns>
        private List<int> getFlightPassengerIds()
        {
            List<int> ids = new List<int>();

            try
            {
                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    ids.Add(Int32.Parse(row[1].ToString()));
                }

                return ids;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// Builds the collection of passengers for the current flight for display in the combo box
        /// </summary>
        /// <param name="flightNum">The number of the flight</param>
        /// <returns>Returns the passenger collection</returns>
        public ObservableCollection<Passenger> GetPassengers()
        {
            try
            {
                passengerCollection.Clear();
                List<int> flightPassengerids = getFlightPassengerIds();

                foreach (DataRow row in PassengerSet.Tables[0].Rows)
                {
                    int passengerId = Int32.Parse(row[0].ToString());
                    if (flightPassengerids.Contains(passengerId))
                    {
                        passengerCollection.Add(new Passenger(row[1].ToString(), row[2].ToString()));
                    }
                }

                return passengerCollection;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is for determining if a given seat is taken
        /// </summary>
        /// <param name="flightNum"></param>
        /// <param name="seatRow"></param>
        /// <param name="seatNum"></param>
        /// <returns>True if seat is taken, false if it is free</returns>
        public bool checkSeat(string seatRow, int seatNum)
        {
            try
            {
                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (row[2].ToString() == seatRow && Int32.Parse(row[3].ToString()) == seatNum)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This is for getting what seat a passenger is sitting in
        /// </summary>
        /// <param name="passenger"></param>
        /// <returns>A string version of the seat row + seat number</returns>
        public string getPassengerSeat(Passenger passenger)
        {
            try
            {
                int id = getPassengerId(passenger);

                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (Int32.Parse(row[1].ToString()) == id)
                    {
                        return row[2].ToString() + row[3].ToString();
                    }
                }

                throw new Exception("Passenger Not Found");
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// Get a Passenger based from their seat location
        /// </summary>
        /// <param name="seatRow"></param>
        /// <param name="seatNum"></param>
        /// <returns></returns>
        public Passenger getPassengerBySeat(string seatRow, int seatNum)
        {
            try
            {
                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (row[2].ToString() == seatRow && Int32.Parse(row[3].ToString()) == seatNum)
                    {
                        return getPassengerById(Int32.Parse(row[1].ToString()));
                    }
                }

                throw new Exception("Passenger Seat not found");
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        
        public int getPassengerIndex(Passenger passenger)
        {
            try
            {
                for (int i = 0; i < passengerCollection.Count; i++)
                {
                    if (passenger.FirstName == passengerCollection[i].FirstName && passenger.LastName == passengerCollection[i].LastName)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        #endregion
    }

    /// <summary>
    /// A data class for storing information about a passenger
    /// </summary>
    class Passenger
    {
        #region Attributes
        public string FirstName { get; set; }

        public string LastName { get; set; }

        #endregion

        #region Methods 
        public Passenger(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
        #endregion
    }
}

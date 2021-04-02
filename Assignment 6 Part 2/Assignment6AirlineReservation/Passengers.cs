using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assignment6AirlineReservation
{
    public class Passengers
    {
        #region Attributes
        /// <summary>
        /// This is the data access class for accessing the database
        /// </summary>
        private clsDataAccess db;

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
        /// <summary>
        /// Constructor method for Passengers class
        /// </summary>
        public Passengers()
        {
            db = new clsDataAccess();
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
                string SQL = "SELECT Flight_ID FROM Flight WHERE Flight_Number = \"" + flightNumber + "\"";
                flightId = Int32.Parse(db.ExecuteScalarSQL(SQL));

                SQL = "SELECT * FROM Passenger";
                int rows = 0;
                PassengerSet = db.ExecuteSQLStatement(SQL, ref rows);

                SQL = "SELECT * FROM Flight_Passenger_Link WHERE Flight_ID = " + flightId;
                PassengerFlightSet = db.ExecuteSQLStatement(SQL, ref rows);

                buildPassengerList();

            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method will load in the datasets, intended for when checking validity of datasets, once flight number has been determined
        /// </summary>
        private void refreshData()
        {
            try
            {
                string SQL = "SELECT Flight_ID FROM Flight WHERE Flight_Number = \"" + flightNumber + "\"";
                flightId = Int32.Parse(db.ExecuteScalarSQL(SQL));

                SQL = "SELECT * FROM Passenger";
                int rows = 0;
                PassengerSet = db.ExecuteSQLStatement(SQL, ref rows);

                SQL = "SELECT * FROM Flight_Passenger_Link WHERE Flight_ID = " + flightId;
                PassengerFlightSet = db.ExecuteSQLStatement(SQL, ref rows);

                buildPassengerList();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method will only rebuild the PassengerSet, it should only be run after setting up the flight dataset
        /// </summary>
        private void refreshPassengers()
        {
            try
            {
                string SQL = "SELECT * FROM Passenger";
                int rows = 0;
                PassengerSet = db.ExecuteSQLStatement(SQL, ref rows);

                buildPassengerList();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        private void buildPassengerList()
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
        }

        /// <summary>
        /// Builds the collection of passengers for the current flight for display in the combo box
        /// </summary>
        /// <returns>Returns the passenger collection</returns>
        public ref ObservableCollection<Passenger> GetPassengers()
        {
            try
            {
                return ref passengerCollection;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is used to add a new passenger to the database
        /// </summary>
        /// <param name="passenger">An object of the new passenger</param>
        /// <param name="seatNum">The seat location of the new passenger</param>
        public void addNewPassenger(Passenger passenger, int seatNum)
        {
            try
            {
                string SQL = "INSERT INTO Passenger (First_Name, Last_Name) VALUES('" + passenger.FirstName + "', '" + passenger.LastName + "')";
                db.ExecuteNonQuery(SQL);

                refreshPassengers();
                int passengerid = getPassengerId(passenger);

                SQL = "INSERT INTO Flight_Passenger_Link (Flight_ID, Passenger_ID, Seat_Number) VALUES(" + flightId + ", " + passengerid + ", '" + seatNum + "')";
                db.ExecuteNonQuery(SQL);

                refreshData();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is used to update a passenger's seat in the database
        /// </summary>
        /// <param name="passenger">The object of the passenger to update</param>
        /// <param name="seatNum">The new seat location for the passenger</param>
        public void changePassengerSeat(Passenger passenger, int seatNum)
        {
            try
            {
                string SQL = "UPDATE Flight_Passenger_Link SET Seat_Number = '" + seatNum + "' WHERE Flight_ID = " + flightId + " AND Passenger_ID = " + getPassengerId(passenger);
                db.ExecuteNonQuery(SQL);

                refreshData();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This method is for determining if a given seat is taken
        /// </summary>
        /// <param name="seatNum"></param>
        /// <returns>True if seat is taken, false if it is free</returns>
        public bool checkSeat(int seatNum)
        {
            try
            {
                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (Int32.Parse(row[2].ToString()) == seatNum)
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
        /// This method is for determining if a passenger already exists in the database
        /// </summary>
        /// <param name="firstName">First Name of the Passenger</param>
        /// <param name="lastName">Last Name of the Passenger</param>
        /// <returns>True if the passenger exists, false if they don't</returns>
        public bool checkExistingPassenger(string firstName, string lastName)
        {
            try
            {
                //Ensure that datasets are up to date
                refreshData();

                foreach (DataRow row in PassengerSet.Tables[0].Rows)
                {
                    if (row[1].ToString() == firstName && row[2].ToString() == lastName)
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

        /// <summary>
        /// Used to lookup from the passenger dataset to retrieve a passenger object based on it's ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a passenger object based on the ID number</returns>
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
        /// This is for getting what seat a passenger is sitting in
        /// </summary>
        /// <param name="passenger"></param>
        /// <returns>A string version of the seat row + seat number</returns>
        public int getPassengerSeatNum(Passenger passenger)
        {
            try
            {
                int id = getPassengerId(passenger);

                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (Int32.Parse(row[1].ToString()) == id)
                    {
                        return Int32.Parse(row[2].ToString());
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
        /// <param name="seatNum"></param>
        /// <returns></returns>
        public Passenger getPassengerBySeat(int seatNum)
        {
            try
            {
                foreach (DataRow row in PassengerFlightSet.Tables[0].Rows)
                {
                    if (Int32.Parse(row[2].ToString()) == seatNum)
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
    public class Passenger
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

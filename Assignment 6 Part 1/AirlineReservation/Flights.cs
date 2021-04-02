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
    /// <summary>
    /// This is the class for acquiring the flight data from the database
    /// </summary>
    class Flights
    {
        #region Attributes
        /// <summary>
        /// This is the data access class for accessing the database
        /// </summary>
        private DataAccess db;

        /// <summary>
        /// An Observable Collection for retrieval of flight data
        /// </summary>
        private ObservableCollection<Flight> collection;

        #endregion

        #region Methods

        /// <summary>
        /// The constructor for the Flights class
        /// </summary>
        public Flights()
        {
            db = new DataAccess();
            collection = new ObservableCollection<Flight>();
        }

        /// <summary>
        /// This method is used to retrieve the flight data, which also reacquires the flights table from the database
        /// </summary>
        /// <returns>A collection of Flight objects</returns>
        public ObservableCollection<Flight> GetFlights()
        {
            try
            {
                collection.Clear();

                string SQL = "SELECT * FROM Flights";
                int rows = 0;
                DataSet ds = db.ExecuteSQL(SQL, ref rows);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    collection.Add(new Flight(Int32.Parse(dr[1].ToString()), dr[2].ToString()));
                }

                return collection;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        #endregion
    }

    /// <summary>
    /// A data class for storing the information of a flight
    /// </summary>
    class Flight
    {
        #region Attributes
        public int flightNum { get; set; }
        public string aircraft { get; set; }

        #endregion

        #region Methods

        public Flight(int flightNum, string aircraft)
        {
            this.flightNum = flightNum;
            this.aircraft = aircraft;
        }

        public override string ToString()
        {
            return flightNum + " - " + aircraft;
        }

        #endregion
    }
}

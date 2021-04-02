using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AirlineReservation
{
    class DataAccess
    {
        #region Attributes
        /// <summary>
        /// Connection string to the database
        /// </summary>
        private string connectionString;

        #endregion

        #region Methods

        public DataAccess()
        {
            connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + Directory.GetCurrentDirectory() + "\\ReservationDatabase.accdb";
        }

        /// <summary>
        /// This method takes an SQL statement and returns the resulting DataSet 
        /// </summary>
        /// <param name="SQL">Input SQL Statement</param>
        /// <param name="rows">Reference parameter for the number of selected rows</param>
        /// <returns>Returns a DataSet containing the data retrieved by the SQL statement</returns>
        public DataSet ExecuteSQL(string SQL, ref int rows)
        {
            try
            {
                DataSet ds = new DataSet();

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                    {
                        //Open connection to class
                        conn.Open();

                        //Using the connected database, run the SQL statement
                        adapter.SelectCommand = new OleDbCommand(SQL, conn);
                        adapter.SelectCommand.CommandTimeout = 0;

                        //Fill dataset with retrieved data from the SQL statement
                        adapter.Fill(ds);
                    }
                }

                //Determine the number of rows that were acquired from the data
                rows = ds.Tables[0].Rows.Count;

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This is for retrieving a single value from the database.
        /// Generally for retrieving a database ID.
        /// </summary>
        /// <param name="SQL">Input SQL Statement</param>
        /// <returns>A string of a single value</returns>
        public string ExecuteScalarSQL(string SQL)
        {
            try
            {
                //An object for holding return value
                object obj;

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                    {
                        //Open connection to class
                        conn.Open();

                        //Using the connected database, run the SQL statement
                        adapter.SelectCommand = new OleDbCommand(SQL, conn);
                        adapter.SelectCommand.CommandTimeout = 0;

                        obj = adapter.SelectCommand.ExecuteScalar();
                    }
                }

                //Check if an object was actually retrieved 
                if (obj == null)
                {
                    return "";
                }
                else
                {
                    return obj.ToString();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        /// <summary>
        /// This is for updating, inserting, deleting, etc., data in the database
        /// </summary>
        /// <param name="SQL">Input SQL Statement</param>
        /// <returns>The number of rows effected</returns>
        public int ExecuteNonQuery(string SQL)
        {
            try
            {
                //An object for holding return value
                int rowCount;

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                    {
                        //Open connection to class
                        conn.Open();

                        //Using the connected database, run the SQL statement
                        OleDbCommand cmd = new OleDbCommand(SQL, conn);
                        cmd.CommandTimeout = 0;

                        //Execute a non query statement
                        rowCount = cmd.ExecuteNonQuery();
                    }

                    //Now return the number of effected rows
                    return rowCount;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + "." + MethodInfo.GetCurrentMethod().Name + " -> " + ex.Message);
            }
        }

        #endregion
    }
}

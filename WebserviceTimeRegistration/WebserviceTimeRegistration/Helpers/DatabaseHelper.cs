using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Helpers
{
    public static class DatabaseHelper
    {
        #region MSSQL

        /***********************************************************/
        // Take a dictionary and create an object of the specified type
        /***********************************************************/
        public static Object GetObject(this Dictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null) continue;

                object value = kv.Value;
                if (value is Dictionary<string, object>)
                    value = GetObject((Dictionary<string, object>)value, prop.PropertyType);

                if (value is DBNull)
                    value = value.ToString();

                if (value is DateTime)
                    value = ((DateTime)value).ToString("dd-MM-yyyy HH:mm");

                prop.SetValue(obj, value, null);
            }
            return obj;
        }

        /***********************************************************/
        // Read data from sqlreader and put it into a dictionary
        /***********************************************************/
        public static Dictionary<string, object> GetObjectData(SqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            Dictionary<string, object> dataList = new Dictionary<string, object>();

            foreach (var item in columns)
                dataList.Add(item, reader[item]);

            return dataList;
        }

        /***********************************************************/
        // Takes a dictionary and creates an object of the specified type
        /***********************************************************/
        public static List<object> GetObjectDataList(SqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            List<object> dataList = new List<object>();

            foreach (var item in columns)
                dataList.Add(reader[item]);

            return dataList;
        }

        /***********************************************************/
        // Return the sqlconnection object
        /***********************************************************/
        public static SqlConnection GetDatabaseConnection()
        {
            string conString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
            return new SqlConnection(conString);
        }

        /***********************************************************/
        // Return a list of objects from sql call with specified type
        /***********************************************************/
        public static List<object> GetObjectsFromSQLReader(string cmd, Type type)
        {
            SqlConnection con = GetDatabaseConnection();

            List<object> objectList = new List<object>();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = GetObjectData(reader);
                            objectList.Add(GetObject(data, type));
                        }
                    }
                }
            }

            return objectList;
        }

        /***********************************************************/
        // Return a list of objects from sql call
        /***********************************************************/
        public static List<object> GetObjectsFromSQLReader(string cmd)
        {
            SqlConnection con = GetDatabaseConnection();

            List<object> objectList = new List<object>();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = GetObjectDataList(reader);
                            objectList.Add(data);
                        }
                    }
                }
            }

            return objectList;
        }

        /***********************************************************/
        // Return a list of objects from sql stored procedure with specified type 
        /***********************************************************/
        public static List<object> GetObjectsFromSQLReader(SqlCommand sqlCommand, Type type)
        {
            SqlConnection con = GetDatabaseConnection();

            List<object> objectList = new List<object>();

            using (con)
            {
                sqlCommand.Connection = con;
                con.Open();
                using (sqlCommand)
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = GetObjectData(reader);

                            if (type == typeof(List<object>))
                            {
                                List<object> list = new List<object>();

                                foreach (KeyValuePair<string, object> item in data)
                                    list.Add(item.Value);

                                return list;
                            }
                            objectList.Add(GetObject(data, type));
                        }
                    }
                }
            }

            return objectList;
        }

        /***********************************************************/
        // Execute command
        /***********************************************************/
        public static bool ExecuteCommand(string cmd)
        {
            SqlConnection con = GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();
            }

            return true;
        }

        /***********************************************************/
        // Execute command stored procedures
        /***********************************************************/
        public static bool ExecuteCommand(SqlCommand cmd)
        {
            SqlConnection con = GetDatabaseConnection();

            using (con)
            {
                cmd.Connection = con;
                con.Open();
                using (cmd)
                    cmd.ExecuteNonQuery();
            }

            return true;
        }

        #endregion
    }
}
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Helpers
{
    public class DatabaseHelper
    {
        public static List<string> GetObjectData(SqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            List<string> dataList = new List<string>();

            foreach (var item in columns)
                dataList.Add(reader[item].ToString());

            return dataList;
        }

        public static List<string> GetObjectData(MySqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            List<string> dataList = new List<string>();

            foreach (var item in columns)
                dataList.Add(reader[item].ToString());

            return dataList;
        }

        public static SqlConnection GetDatabaseConnection()
        {
            string conString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
            return new SqlConnection(conString);
        }
    }
}
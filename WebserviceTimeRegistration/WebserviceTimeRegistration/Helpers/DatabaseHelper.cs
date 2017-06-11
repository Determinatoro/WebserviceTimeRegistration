﻿using MySql.Data.MySqlClient;
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
                            var data = GetObjectData(reader);
                            objectList.Add(data);
                        }
                    }
                }
            }

            return objectList;
        }

        public static List<object> GetObjectsFromSQLReader(SqlCommand sqlCommand)
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
                            objectList.Add(data);
                        }
                    }
                }
            }

            return objectList;
        }

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
    }
}
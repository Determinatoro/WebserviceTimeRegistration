using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using WebserviceTimeRegistration.Database_objects;
using WebserviceTimeRegistration.Webservice;

namespace WebserviceTimeRegistration
{
    /// <summary>
    /// Summary description for TimeRegistration
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class TimeRegistration : WebService
    {
        private SqlConnection GetDatabaseConnection()
        {
            string conString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
            return new SqlConnection(conString);
        }

        private string GetErrorMessage(int id)
        {
            string errorMessage = "Error";

            switch (id)
            {
                case 1:
                    errorMessage = "Please give an User ID";
                    break;
                case 2:
                    errorMessage = "Firstname or lastname are empty";
                    break;
            }

            return errorMessage;
        }

        private List<string> GetObjectData(SqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            List<string> dataList = new List<string>();

            foreach (var item in columns)
                dataList.Add(reader[item].ToString());

            return dataList;
        }     

        [WebMethod]
        public void GetUser(int userId)
        {
            if (userId < 1)
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                return;
            }

            string cmd = "SELECT * FROM Users WHERE UserId ='" + userId.ToString() + "'";

            List<User> userList = new List<User>();

            SqlConnection con = GetDatabaseConnection();

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

                            User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                            userList.Add(user);
                        }
                    }
                }
            }

            WebserviceHelper.WriteResponse(Context, true, userList);
        }

        [WebMethod]
        public void CreateUser(string firstName, string lastName, bool admin)
        {
            if (firstName == "" || lastName == "")
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(2));
                return;
            }

            string cmd = "INSERT INTO Users (FirstName, LastName, Admin) VALUES ('" + firstName + "', '" + lastName + "', '" + admin + "')";

            SqlConnection con = GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();
            }

            WebserviceHelper.WriteResponse(Context, true, "");
        }

        [WebMethod]
        public void DeleteUser(int userId)
        {
            if (userId < 1)
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                return;
            }

            string cmd = "DELETE FROM Users WHERE UserId='" + userId.ToString() + "'";

            SqlConnection con = GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();
            }

            WebserviceHelper.WriteResponse(Context, true, "");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using WebserviceTimeRegistration.Database_objects;
using WebserviceTimeRegistration.Helpers;
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
        #region FUNCTIONS

        /***********************************************************/
        // GET ERROR MESSAGE
        /***********************************************************/
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
                case 3:
                    errorMessage = "Username or lastname are empty";
                    break;
                case 4:
                    errorMessage = "Wrong username and/or password";
                    break;
            }

            return errorMessage;
        }

        /***********************************************************/
        // CREATE USERNAME
        /***********************************************************/
        private string CreateUsername(string firstName, string lastName)
        {
            if (firstName.Length < 2 || lastName.Length < 2)
                throw new Exception("Firstname or lastname is too short");

            string username = firstName.Substring(0, 2) + lastName.Substring(0, 2);

            string cmd = "SELECT IDENT_CURRENT('Users')";

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = WebserviceHelper.GetObjectData(reader);

                            int number = int.Parse(data[0]) + 1;
                            return (username + number).ToLower();
                        }
                    }
                }
            }

            throw new Exception("Fail when creating username");
        }

        #endregion

        #region LOGIN

        /***********************************************************/
        // CHECK LOGIN
        /***********************************************************/
        [WebMethod]
        public void CheckLogin(string username, string password)
        {
            string cmd = "SELECT * FROM Users WHERE Username ='" + username + "' AND Password='" + EncryptionHelper.Encrypt(password) + "'";

            List<User> userList = new List<User>();

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(4));
                            return;
                        }

                        while (reader.Read())
                        {
                            var data = WebserviceHelper.GetObjectData(reader);

                            User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                            WebserviceHelper.WriteResponse(Context, true, user);
                            return;
                        }
                    }
                }
            }
        }

        #endregion

        #region USER METHODS

        /***********************************************************/
        // GET USER
        /***********************************************************/
        [WebMethod]
        public void GetUser(int userId)
        {
            if (userId < 1)
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                return;
            }

            string cmd = "SELECT * FROM Users WHERE UserId ='" + userId.ToString() + "'";

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = WebserviceHelper.GetObjectData(reader);

                            User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                            WebserviceHelper.WriteResponse(Context, true, user);
                            return;
                        }
                    }
                }
            }
        }

        /***********************************************************/
        // GET USER
        /***********************************************************/
        [WebMethod]
        public void GetUsers()
        {
            string cmd = "SELECT * FROM Users";

            List<User> usersList = new List<User>();

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = WebserviceHelper.GetObjectData(reader);

                            User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                            usersList.Add(user);
                        }
                    }
                }
            }

            WebserviceHelper.WriteResponse(Context, true, usersList);
        }

        /***********************************************************/
        // CREATE USER
        /***********************************************************/
        [WebMethod]
        public void CreateUser(string firstName, string lastName, bool admin, string password)
        {
            if (firstName == "" || lastName == "" || password == "")
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(2));
                return;
            }

            string username = CreateUsername(firstName, lastName);

            string cmd = "INSERT INTO Users (FirstName, LastName, Admin, Password, Username) VALUES ('" + firstName + "', '" + lastName + "', '" + admin + "', '" + EncryptionHelper.Encrypt(password) + "', '" + username + "')";

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();

            }

            WebserviceHelper.WriteResponse(Context, true, "");
        }

        /***********************************************************/
        // DELETE USER
        /***********************************************************/
        [WebMethod]
        public void DeleteUser(int userId)
        {
            if (userId < 1)
            {
                WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                return;
            }

            string cmd = "DELETE FROM Users WHERE UserId='" + userId.ToString() + "'";

            SqlConnection con = WebserviceHelper.GetDatabaseConnection();

            using (con)
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(cmd, con))
                    command.ExecuteNonQuery();
            }

            WebserviceHelper.WriteResponse(Context, true, "");
        }

        #endregion
    }
}

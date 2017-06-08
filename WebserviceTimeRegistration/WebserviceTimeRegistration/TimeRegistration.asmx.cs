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
    [WebService(Namespace = "http://timeregistration.test/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class TimeRegistration : WebService
    {
        #region FUNCTIONS

        /***********************************************************/
        // GET ERROR MESSAGE - Get a specific error message
        /***********************************************************/
        private string GetErrorMessage(int id)
        {
            string errorMessage = "Error";

            switch (id)
            {
                case 1:
                    errorMessage = "Please give an user ID";
                    break;
                case 2:
                    errorMessage = "Firstname or lastname are empty";
                    break;
                case 3:
                    errorMessage = "No user with that user ID";
                    break;
                case 4:
                    errorMessage = "Wrong username and/or password";
                    break;
                case 5:
                    errorMessage = "Firstname or lastname is too short";
                    break;
                case 6:
                    errorMessage = "Firstname or lastname is too short";
                    break;
                case 101:
                    errorMessage = "Please give an order ID";
                    break;
                case 102:
                    errorMessage = "No order with that order ID";
                    break;
            }

            return errorMessage;
        }



        /***********************************************************/
        // CREATE USERNAME - Create unique username (Example: [Ja]kob + [Pr]echt + userId = japr8)
        /***********************************************************/
        private string CreateUsername(string firstName, string lastName)
        {
            if (firstName.Length < 2 || lastName.Length < 2)
                throw new Exception(GetErrorMessage(5));

            string username = firstName.Substring(0, 2) + lastName.Substring(0, 2);

            string cmd = "SELECT IDENT_CURRENT('Users')";

            List<string> data = (List<string>)DatabaseHelper.GetObjectsFromSQLReader(cmd).FirstOrDefault();
            int number = int.Parse(data[0]) + 1;
            return (username + number).ToLower();            
        }

        #endregion

        #region LOGIN

        /***********************************************************/
        // CHECK LOGIN - Checking login for users username and password
        /***********************************************************/
        [WebMethod]
        public void CheckLogin(string username, string password)
        {
            try
            {
                string cmd = string.Format("SELECT * FROM Users WHERE Username ='{0}' AND Password='{1}'", username, EncryptionHelper.Encrypt(password));

                List<string> data = (List<string>)DatabaseHelper.GetObjectsFromSQLReader(cmd).FirstOrDefault();

                if (data == null)
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(4));
                else
                {
                    User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                    WebserviceHelper.WriteResponse(Context, true, user);
                }
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // RESET PASSWORD - Reset a password for a user
        /***********************************************************/
        [WebMethod]
        public void ResetPassword(int userId, string password)
        {
            try
            {
                string cmd = string.Format("UPDATE Users SET Password='{0}' WHERE UserId='{1}'", EncryptionHelper.Encrypt(password), userId.ToString());

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        #endregion

        #region USER METHODS

        /***********************************************************/
        // GET USER - Get a specific user from their id
        /***********************************************************/
        [WebMethod]
        public void GetUser(int userId)
        {
            try
            {
                if (userId < 1)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                    return;
                }

                string cmd = string.Format("SELECT * FROM Users WHERE UserId ='{0}'", userId.ToString());

                List<string> data = (List<string>)DatabaseHelper.GetObjectsFromSQLReader(cmd).FirstOrDefault();

                if (data != null)
                {
                    User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                    WebserviceHelper.WriteResponse(Context, true, user);
                }
                else
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(3));
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // GET USERS - Get a list of all the users in the database
        /***********************************************************/
        [WebMethod]
        public void GetUsers()
        {
            try
            {
                string cmd = "SELECT * FROM Users";

                List<User> usersList = new List<User>();

                SqlConnection con = DatabaseHelper.GetDatabaseConnection();

                using (con)
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(cmd, con))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = DatabaseHelper.GetObjectData(reader);

                                User user = new User(int.Parse(data[0]), data[1], data[2], bool.Parse(data[3]));
                                usersList.Add(user);
                            }
                        }
                    }
                }

                WebserviceHelper.WriteResponse(Context, true, usersList);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // CREATE USER - Create a new user. A unique username is made together with an encrypted password
        /***********************************************************/
        [WebMethod]
        public void CreateUser(string firstName, string lastName, bool admin, string password)
        {
            try
            {
                if (firstName == "" || lastName == "" || password == "")
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(2));
                    return;
                }

                string username = CreateUsername(firstName, lastName);

                string cmd = string.Format("INSERT INTO Users (FirstName, LastName, Admin, Password, Username) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", firstName, lastName, admin, EncryptionHelper.Encrypt(password), username);

                SqlConnection con = DatabaseHelper.GetDatabaseConnection();

                using (con)
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(cmd, con))
                        command.ExecuteNonQuery();
                }

                WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // DELETE USER - Delete user with the users id
        /***********************************************************/
        [WebMethod]
        public void DeleteUser(int userId)
        {
            try
            {
                if (userId < 1)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
                    return;
                }

                string cmd = string.Format("DELETE FROM Users WHERE UserId='{0}'", userId.ToString());

                SqlConnection con = DatabaseHelper.GetDatabaseConnection();

                using (con)
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(cmd, con))
                        command.ExecuteNonQuery();
                }

                WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        #endregion

        #region ORDER METHODS



        /***********************************************************/
        // GET ORDER - Get a specific order from id
        /***********************************************************/
        [WebMethod]
        public void GetOrder(int orderId)
        {
            try
            {
                if (orderId < 1)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(101));
                    return;
                }

                string cmd = string.Format("SELECT * FROM Orders WHERE OrderId='{0}'", orderId.ToString());

                List<string> data = (List<string>)DatabaseHelper.GetObjectsFromSQLReader(cmd).FirstOrDefault();

                if (data != null)
                {
                    Order order = new Order(int.Parse(data[0]), data[1], data[2], int.Parse(data[3]));
                    WebserviceHelper.WriteResponse(Context, true, order);
                }
                else
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(102));
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        #endregion

        #region ROLE METHODS



        #endregion
    }
}

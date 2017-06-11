﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            List<object> data = (List<object>)DatabaseHelper.GetObjectsFromSQLReader(cmd).FirstOrDefault();
            int number = int.Parse(data[0].ToString()) + 1;
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

                User user = (User)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(User)).FirstOrDefault();

                if (user == null)
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(4));
                else
                    WebserviceHelper.WriteResponse(Context, true, user);
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

                User user = (User)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(User)).FirstOrDefault();

                if (user != null)
                    WebserviceHelper.WriteResponse(Context, true, user);
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

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(User));

                foreach (User obj in objectList)
                    usersList.Add(obj);

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

                if (DatabaseHelper.ExecuteCommand(cmd))
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

                if (DatabaseHelper.ExecuteCommand(cmd))
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
        // GET ORDERS - Get a list of all the orders associated with the user
        /***********************************************************/
        [WebMethod]
        public void GetOrders(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("GetOrders");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                List<Order> ordersList = new List<Order>();

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Order));

                foreach (Order obj in objectList)
                    ordersList.Add(obj);

                WebserviceHelper.WriteResponse(Context, true, ordersList);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // GET ORDER - Get a specific order associated with the user
        /***********************************************************/
        [WebMethod]
        public void GetOrder(int userId, int orderId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("GetOrder");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                Order order = (Order)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Order)).FirstOrDefault();

                if (order != null)
                    WebserviceHelper.WriteResponse(Context, true, order);
                else
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(102));
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // CREATE ORDER - Create an order where user is given the role Creator
        /***********************************************************/
        [WebMethod]
        public void CreateOrder(int userId, string name, string description, int customerId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("CreateOrder");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = description;
                cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        /***********************************************************/
        // DELETE ORDER - Delete order and all the OrderRoles associated with it
        /***********************************************************/
        [WebMethod]
        public void DeleteOrder(int orderId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("DeleteOrder");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        #endregion

        #region ROLE METHODS

        [WebMethod]
        public void GetRoles()
        {
            try
            {
                string cmd = "SELECT * FROM Roles";

                List<Role> rolesList = new List<Role>();

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Role));

                foreach (Role obj in objectList)
                    rolesList.Add(obj);

                WebserviceHelper.WriteResponse(Context, true, rolesList);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        [WebMethod]
        public void CreateRole(string name)
        {
            try
            {
                string cmd = string.Format("INSERT INTO Roles (Name) VALUES ('{0}')", name);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

        [WebMethod]
        public void DeleteRole(int roleId)
        {
            try
            {
                if (roleId <= 1)
                    return;

                string cmd = string.Format("DELETE FROM Roles WHERE RoleId = {0}", roleId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes);
            }
        }

         [WebMethod]
         public void GetOrderRoles(int orderId)
         {
             try
             {
                 SqlCommand cmd = new SqlCommand("GetOrderRoles");
                 cmd.CommandType = CommandType.StoredProcedure;
                 cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                 List<OrderRole> orderRolesList = new List<OrderRole>();

                 var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(OrderRole));

                 foreach (OrderRole obj in objectList)
                    orderRolesList.Add(obj);

                 WebserviceHelper.WriteResponse(Context, true, orderRolesList);
             }
             catch (Exception mes)
             {
                 WebserviceHelper.WriteResponse(Context, false, mes);
             }
         }

        #endregion

        #region CUSTOMER METHODS


        #endregion

        #region TIME REGISTRATION METHODS



        #endregion
    }
}

using System;
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

        private string CreateUsername(string firstName, string lastName, int userId)
        {
            if (firstName.Length < 2 || lastName.Length < 2)
                throw new Exception(GetErrorMessage(5));

            string username = firstName.Substring(0, 2) + lastName.Substring(0, 2);
            return (username + userId.ToString()).ToLower();
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // UPDATE USER - Get a list of all the users in the database
        /***********************************************************/
        [WebMethod]
        public void UpdateUser(int userId, string firstName, string lastName, bool admin)
        {
            try
            {
                string username = CreateUsername(firstName, lastName, userId);

                string cmd = string.Format("UPDATE Users SET FirstName='{0}', LastName='{1}', Username='{2}', Admin='{3}' WHERE UserId={4}", firstName, lastName, username, admin, userId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion

        #region ROLE METHODS

        /***********************************************************/
        // GET ROLES - Get all roles
        /***********************************************************/
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // CREATE ROLE - Create a role with a name
        /***********************************************************/
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // DELETE ROLE - Deletes all OrderRoles associated with this roleId and then deletes the role
        /***********************************************************/
        [WebMethod]
        public void DeleteRole(int roleId)
        {
            try
            {
                if (roleId <= 1)
                    return;

                SqlCommand cmd = new SqlCommand("DeleteRole");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion

        #region ORDER ROLES METHOD

        /***********************************************************/
        // GET ORDER ROLES - Gets all Order roles associated with an order
        /***********************************************************/
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
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // CREATE ORDER ROLE - OrderId, UserId, RoleId
        /***********************************************************/
        [WebMethod]
        public void CreateOrderRole(int orderId, int userId, int roleId)
        {
            try
            {
                string cmd = string.Format("INSERT INTO OrderRoles (OrderId, UserId, RoleId) VALUES ({0}, {1}, {2})", orderId, userId, roleId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // UPDATE ORDER ROLE - OrderId, UserId, RoleId
        /***********************************************************/
        [WebMethod]
        public void UpdateOrderRole(int orderId, int userId, int roleId)
        {
            try
            {
                string cmd = string.Format("UPDATE OrderRoles SET RoleId={2} WHERE OrderId={0} AND UserId={1}", orderId, userId, roleId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // DELETE ORDER ROLE - OrderId, UserId, RoleId
        /***********************************************************/
        [WebMethod]
        public void DeleteOrderRole(int orderRoleId)
        {
            try
            {
                string cmd = string.Format("DELETE FROM OrderRoles WHERE OrderRoleId={0}", orderRoleId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion

        #region CUSTOMER METHODS

        /***********************************************************/
        // GET CUSTOMERS - Get customers 
        /***********************************************************/
        [WebMethod]
        public void GetCustomers()
        {
            try
            {
                string cmd = "SELECT * FROM Customers";

                List<Customer> list = new List<Customer>();

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Customer));

                foreach (Customer obj in objectList)
                    list.Add(obj);

                WebserviceHelper.WriteResponse(Context, true, list);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // CREATE CUSTOMER - Name of the customer
        /***********************************************************/
        [WebMethod]
        public void CreateCustomer(string name)
        {
            try
            {
                string cmd = string.Format("INSERT INTO Customers (Name) VALUES ('{0}')", name);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // DELETE CUSTOMER - Delete
        /***********************************************************/
        [WebMethod]
        public void DeleteCustomer(int customerId)
        {
            try
            {
                string cmd = string.Format("DELETE FROM Customers WHERE CustomerId={0}", customerId.ToString());

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion

        #region TIME REGISTRATION METHODS

        /***********************************************************/
        // GET TIME REGISTRATIONS - UserId
        /***********************************************************/
        [WebMethod]
        public void GetTimeRegistrations(int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("GetTimeRegistrations");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                List<Database_objects.TimeRegistration> list = new List<Database_objects.TimeRegistration>();

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Database_objects.TimeRegistration));

                foreach (Database_objects.TimeRegistration obj in objectList)
                    list.Add(obj);

                WebserviceHelper.WriteResponse(Context, true, list);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion
    }
}

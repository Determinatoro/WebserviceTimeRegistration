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
                    errorMessage = "Firstname, lastname or password are empty";
                    break;
                case 2:
                    errorMessage = "No user with that user ID";
                    break;
                case 3:
                    errorMessage = "Wrong username and/or password";
                    break;
                case 4:
                    errorMessage = "Firstname or lastname is too short";
                    break;
                case 5:
                    errorMessage = "Firstname or lastname is too short";
                    break;                
                case 101:
                    errorMessage = "No order with that order ID or you are not associated with the order";
                    break;                
                case 201:
                    errorMessage = "Order relation already exists";
                    break;
                case 202:
                    errorMessage = "There has to be at least one leader role associated with an order";
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
        // CHECK LOGIN - Check login for user - Returns success[true; false], User
        /***********************************************************/
        [WebMethod]
        public void CheckLogin(string username, string password)
        {
            try
            {
                string cmd = string.Format("SELECT * FROM Users WHERE Username ='{0}' AND Password='{1}'", username, EncryptionHelper.Encrypt(password));

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
        // RESET PASSWORD - Reset a password for a user - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void ResetPassword(int userId, string password)
        {
            try
            {
                string cmd = string.Format("UPDATE Users SET Password='{0}' WHERE UserId={1}", EncryptionHelper.Encrypt(password), userId.ToString());

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
        // GET USER - Get a specific user from their id - Returns success[true; false], User
        /***********************************************************/
        [WebMethod]
        public void GetUser(int userId)
        {
            try
            {
                string cmd = string.Format("SELECT * FROM Users WHERE UserId ='{0}'", userId.ToString());

                User user = (User)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(User)).FirstOrDefault();

                if (user != null)
                    WebserviceHelper.WriteResponse(Context, true, user);
                else
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(2));
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // UPDATE USER - Update user information - Returns success[true; false] 
        /***********************************************************/
        [WebMethod]
        public void UpdateUser(int userId, string firstName, string lastName, bool admin)
        {
            try
            {
                string cmd = string.Format("UPDATE Users SET FirstName='{0}', LastName='{1}', Admin='{2}' WHERE UserId={3}", firstName, lastName, admin, userId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // GET USERS - Get a list of all the users in the database - Returns success[true; false], List<User>
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
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(1));
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
        // DELETE USER - Delete user - Returns success[true; false] 
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

                List<OrderWithRoles> orderWithRolesList = new List<OrderWithRoles>();
                objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(OrderWithRoles));

                foreach (OrderWithRoles obj in objectList)
                    orderWithRolesList.Add(obj);

                var list = orderWithRolesList.Select(x => x.OrderId).Distinct().ToList();

                List<OrderWithRoles> temp = new List<OrderWithRoles>();

                foreach (int item in list)
                {
                    var obj = orderWithRolesList.Where(x => x.OrderId == item).FirstOrDefault();

                    var roles = ordersList.Where(x => x.OrderId == item).Select(x => x.RoleName);

                    var rolesList = new List<Role>();

                    foreach (string roleName in roles)
                    {
                        string command = string.Format("SELECT * FROM Roles WHERE Name='{0}'", roleName);
                        Role role = (Role)DatabaseHelper.GetObjectsFromSQLReader(command, typeof(Role)).FirstOrDefault();
                        rolesList.Add(role);
                    }

                    obj.RolesList = rolesList;

                    temp.Add(obj);
                }                

                WebserviceHelper.WriteResponse(Context, true, temp);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // GET ORDER - Get a specific order associated with the user - Returns success[true; false], Order
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

                var orderWithRoles = (OrderWithRoles)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(OrderWithRoles)).FirstOrDefault();

                if (orderWithRoles == null)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(101));
                    return;
                }

                List<Order> ordersList = new List<Order>();

                var objectList = DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(Order));

                foreach (Order obj in objectList)
                    ordersList.Add(obj);

                var roles = ordersList.Select(x => x.RoleName);

                var rolesList = new List<Role>();

                foreach (string roleName in roles)
                {
                    string command = string.Format("SELECT * FROM Roles WHERE Name='{0}'", roleName);
                    Role role = (Role)DatabaseHelper.GetObjectsFromSQLReader(command, typeof(Role)).FirstOrDefault();
                    rolesList.Add(role);
                }

                orderWithRoles.RolesList = rolesList;

                WebserviceHelper.WriteResponse(Context, true, orderWithRoles);
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // CREATE ORDER - Create an order where user is given the role Creator - Returns success[true; false]
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
        // DELETE ORDER - Delete order and all the OrderRoles associated with it - Returns success[true; false]
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
        // GET ROLES - No parameters - Returns success[true; false], List<Roles>
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
        // CREATE ROLE - Name of the role - Returns success[true; false]
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
        // DELETE ROLE - Deletes all OrderRoles associated with this roleId and then deletes the role - Returns success[true; false]
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
        // GET ORDER ROLES - Gets all Order roles associated with an order - Returns success[true; false], List<Order>
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
        // CREATE ORDER ROLE - Create a relation between a user and an order - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void CreateOrderRole(int orderId, int userId, int roleId)
        {
            try
            {
                string cmd = string.Format("SELECT * FROM OrderRoles WHERE RoleId={2} AND OrderId={0} AND UserId={1}", orderId, userId, roleId);

                var orderRole = (OrderRole)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(OrderRole)).FirstOrDefault();

                if (orderRole != null)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(201));
                    return;
                }

                cmd = string.Format("INSERT INTO OrderRoles (OrderId, UserId, RoleId) VALUES ({0}, {1}, {2})", orderId, userId, roleId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // UPDATE ORDER ROLE - Update information in relation - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void UpdateOrderRole(int orderRoleId, int orderId, int userId, int roleId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("CheckLeaderOnUpdateOrder");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@OrderRoleId", SqlDbType.Int).Value = orderRoleId;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;

                var mayUpdate = (bool)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(List<object>)).FirstOrDefault();

                if (!mayUpdate)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(202));
                    return;
                }

                string command = string.Format("SELECT * FROM OrderRoles WHERE RoleId={2} AND OrderId={0} AND UserId={1}", orderId, userId, roleId);

                var orderRole = (OrderRole)DatabaseHelper.GetObjectsFromSQLReader(command, typeof(OrderRole)).FirstOrDefault();

                if (orderRole != null)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(201));
                    return;
                }

                command = string.Format("UPDATE OrderRoles SET RoleId={1} WHERE OrderRoleId={0}", orderRoleId, roleId);

                if (DatabaseHelper.ExecuteCommand(command))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // DELETE ORDER ROLE - Delete relation between order and user - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void DeleteOrderRole(int orderRoleId, int orderId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("CheckLeaderOnOrder");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@OrderRoleId", SqlDbType.Int).Value = orderRoleId;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                var mayDelete = (bool)DatabaseHelper.GetObjectsFromSQLReader(cmd, typeof(List<object>)).FirstOrDefault();

                if (!mayDelete)
                {
                    WebserviceHelper.WriteResponse(Context, false, GetErrorMessage(202));
                    return;
                }

                string command = string.Format("DELETE FROM OrderRoles WHERE OrderRoleId={0}", orderRoleId);

                if (DatabaseHelper.ExecuteCommand(command))
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
        // GET CUSTOMERS - Get customers - Returns success[true; false], List<Customer>
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
        // CREATE CUSTOMER - Name of the customer - Returns success[true; false]
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
        // DELETE CUSTOMER - ID of the customer - Returns success[true; false]
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
        // GET TIME REGISTRATIONS - Get user's time registrations - Returns success[true; false], List<TimeRegistration>
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

        /***********************************************************/
        // START TIME REGISTRATION - Start a time registration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void StartTimeRegistration(DateTime startTime, int orderId, int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("StartTimeRegistration");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = startTime;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // END TIME REGISTRATION - Ends time registration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void EndTimeRegistration(int timeRegId, DateTime endTime)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("EndTimeRegistration");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
                cmd.Parameters.Add("@TimeRegId", SqlDbType.Int).Value = timeRegId;                

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // SET NOTE FOR TIME REGISTRATION - Set a note for a timeregistration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void SetNoteForTimeRegistration(int timeRegId, string note)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SetNoteForTimeRegistration");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TimeRegId", SqlDbType.Int).Value = timeRegId;
                cmd.Parameters.Add("@Note", SqlDbType.NVarChar).Value = note;                

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // UPDATE TIME REGISTRATION - Updates time registration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void UpdateTimeRegistration(int timeRegId, DateTime startTime, DateTime endTime)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("UpdateTimeRegistration");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TimeRegId", SqlDbType.Int).Value = timeRegId;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = startTime;
                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // DELETE TIME REGISTRATION - Deletes time registration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void DeleteTimeRegistration(int timeRegId)
        {
            try
            {
                string cmd = string.Format("DELETE FROM TimeRegistrations WHERE TimeRegId={0}", timeRegId);

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // CREATE TIME REGISTRATION - Create a time registration - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void CreateTimeRegistration(DateTime startTime, DateTime endTime, int orderId, int userId)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("CreateTimeRegistration");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@StartTime", SqlDbType.DateTime).Value = startTime;
                cmd.Parameters.Add("@EndTime", SqlDbType.DateTime).Value = endTime;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                if (DatabaseHelper.ExecuteCommand(cmd))
                    WebserviceHelper.WriteResponse(Context, true, "");
            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        /***********************************************************/
        // GET TIMEREGISTRATIONS PER ORDER - Get timeregistrations per order - Returns success[true; false]
        /***********************************************************/
        [WebMethod]
        public void GetTimeRegistrationsPerOrder(DateTime startTime, DateTime endTime, int orderId, int userId)
        {
            try
            {

            }
            catch (Exception mes)
            {
                WebserviceHelper.WriteResponse(Context, false, mes.Message);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class OrderRole
    {
        private int orderId;
        private string orderName;
        private string description;
        private int userId;
        private string firstName;
        private string lastName;
        private int roleId;
        private string roleName;

        public OrderRole(int orderId, string orderName, string description, int userId, string firstName, string lastName, int roleId, string roleName)
        {
            this.orderId = orderId;
            this.orderName = orderName;
            this.description = description;
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
            this.roleId = roleId;
            this.roleName = roleName;
        }

        public OrderRole()
        {

        }

        public int OrderId
        {
            get
            {
                return orderId;
            }

            set
            {
                orderId = value;
            }
        }

        public string OrderName
        {
            get
            {
                return orderName;
            }

            set
            {
                orderName = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public int UserId
        {
            get
            {
                return userId;
            }

            set
            {
                userId = value;
            }
        }

        public string FirstName
        {
            get
            {
                return firstName;
            }

            set
            {
                firstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return lastName;
            }

            set
            {
                lastName = value;
            }
        }

        public int RoleId
        {
            get
            {
                return roleId;
            }

            set
            {
                roleId = value;
            }
        }

        public string RoleName
        {
            get
            {
                return roleName;
            }

            set
            {
                roleName = value;
            }
        }
    }
}
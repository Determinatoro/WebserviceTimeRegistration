using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class OrderWithRoles
    {
        private int orderId;
        private string orderName;
        private string description;
        private string customerName;
        private List<Role> rolesList;

        public OrderWithRoles()
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

        public string CustomerName
        {
            get
            {
                return customerName;
            }

            set
            {
                customerName = value;
            }
        }

        public List<Role> RolesList
        {
            get
            {
                return rolesList;
            }

            set
            {
                rolesList = value;
            }
        }
    }
}
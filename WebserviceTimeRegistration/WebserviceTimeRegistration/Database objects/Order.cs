using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class Order
    {
        private int orderId;
        private string orderName;
        private string description;
        private string customerName;
        private string roleName;

        public Order(int orderId, string orderName, string description, string customerName, string roleName)
        {
            this.orderId = orderId;
            this.orderName = orderName;
            this.description = description;
            this.customerName = customerName;
            this.roleName = roleName;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class Order
    {
        private int orderId;
        private string name;
        private string description;
        private int customerId;

        public Order(int orderId, string name, string description, int customerId)
        {
            this.orderId = orderId;
            this.name = name;
            this.description = description;
            this.customerId = customerId;
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

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
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

        public int CustomerId
        {
            get
            {
                return customerId;
            }

            set
            {
                customerId = value;
            }
        }
    }
}
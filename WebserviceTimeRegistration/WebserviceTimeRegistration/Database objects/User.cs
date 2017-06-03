using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class User
    {
        private int userId;
        private string firstName;
        private string lastName;

        public User(int userId, string firstName, string lastName)
        {
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
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
    }
}
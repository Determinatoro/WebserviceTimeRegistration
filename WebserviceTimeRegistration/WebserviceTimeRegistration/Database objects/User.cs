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
        private string username;
        private bool admin;

        public User()
        {

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

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }

        public bool Admin
        {
            get
            {
                return admin;
            }

            set
            {
                admin = value;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class Role
    {
        private int roleId;
        private string name;

        public Role(int roleId, string name)
        {
            this.roleId = roleId;
            this.name = name;
        }

        public Role()
        {

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
    }
}
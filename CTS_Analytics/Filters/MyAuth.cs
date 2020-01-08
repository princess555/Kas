using System;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using CTS_Models.Role;
using System.Collections;
using CTS_Models.DBContext;
using System.Collections.Generic;
using System.Diagnostics;
using System.Configuration;
using System.Data.SqlClient;

namespace CTS_Analytics.Filters
{
    public class MyAuthAttribute : AuthorizeAttribute
    {
        object loginDB, domainDB, rolesDB;

        private string[] allowedUsers = new string[] { };
        private string[] allowedRoles = new string[] { };

        //private string role = null;

        public MyAuthAttribute() { }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var users = httpContext.User;

            if (!String.IsNullOrEmpty(base.Users))
            {
                allowedUsers = base.Users.Split(new char[] { ',' });
                for (int i = 0; i < allowedUsers.Length; i++)
                {
                    allowedUsers[i] = allowedUsers[i].Trim();
                }
            }

            if (!String.IsNullOrEmpty(base.Roles))
            {
                allowedRoles = base.Roles.Split(new char[] { ',' });

                for (int i = 0; i < allowedRoles.Length; i++)
                {
                    allowedRoles[i] = allowedRoles[i].Trim();
                }
            }

            return httpContext.Request.IsAuthenticated && Role(httpContext);
        }

        private bool Role(HttpContextBase httpContext)
        {
            string name = httpContext.User.Identity.Name.ToString();
            name = name.Remove(0, 7);

            string allowRole = allowedRoles[0];
            bool flag = false;
            string connectionString = ConfigurationManager.ConnectionStrings["CentralDbConnection"].ConnectionString;

            List<string> roleList = new List<string>();

            string sql = $"SELECT CtsUser_Login, CtsUser_Domain, CtsRole_RoleName FROM CtsUserCtsRoles WHERE CtsUser_Login = '{name}'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            loginDB = reader.GetValue(0);
                            domainDB = reader.GetValue(1);
                            rolesDB = reader.GetValue(2);

                            if (loginDB.ToString().ToLower() == name.ToLower().ToString())
                            {
                                if (rolesDB.ToString().ToLower() == allowRole.ToString().ToLower())
                                {
                                    flag = true;

                                }
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                { }

                if (flag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}





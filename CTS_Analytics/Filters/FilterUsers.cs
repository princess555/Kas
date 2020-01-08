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
       

    public class FilterUsers : AuthorizeAttribute
    {
        object loginDB, domainDB;

        public FilterUsers() { }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var users = httpContext.User;

            return httpContext.Request.IsAuthenticated && User(httpContext);
        }
        private bool User(HttpContextBase httpContext)
        {
            string name = httpContext.User.Identity.Name.ToString();
            string connectionString = ConfigurationManager.ConnectionStrings["CentralDbConnection"].ConnectionString;
           
            name = name.Remove(0, 7);

            
            string sql = $"SELECT CtsUser_Login, CtsUser_Domain, CtsRole_RoleName FROM CtsUserCtsRoles WHERE CtsUser_Login = '{name}'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

          
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            loginDB = reader.GetValue(0);
                            domainDB = reader.GetValue(1);
                            loginDB = String.Concat("Europe\\", loginDB);


                        }
                    }
             
                reader.Close();

                if (loginDB.ToString().ToLower() == httpContext.User.Identity.Name.ToLower())
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
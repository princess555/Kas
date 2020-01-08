using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CTS_Models;
using CTS_Models.DBContext;

using System.Configuration;
using System.Security.Principal;
using CTS_Core;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace CTS_Analytics.Helpers
{
    public static class ReportRoleHelper
    {


        public static bool UserHasAnyReportRole(IIdentity user)
        {
            if (Cacher.Instance.TryRead(user.Name + "CheckUserReportRole") is bool)
                return (bool)Cacher.Instance.TryRead(user.Name + "CheckUserReportRole");

            var userHasReportRole = CtsAuthorizeProvider.CheckIsInRole(user, GetReprtRoles());
            Cacher.Instance.Write(user.Name + "CheckUserReportRole", userHasReportRole);
            return userHasReportRole;
        }

        private static string[] GetReprtRoles()
        {
            var cdb = new CtsDbContext();
            return cdb.CtsRole
                   .Where(r => r.RoleName.ToLower().StartsWith("rep"))
                   .Select(r => r.RoleName)
                  .ToArray();
        }

        public static bool UserHasLocationRole(IIdentity user, string locationID)
        {
            bool flag = false;
            object loginDB, domainDB, rolesDB;

                string users = user.Name;
                users = users.Remove(0, 7);

                using (var cdb = new CtsDbContext())
                {
                    locationID = cdb.Locations.Find(locationID).LocationName;

                    string connectionString = ConfigurationManager.ConnectionStrings["CentralDbConnection"].ConnectionString;

                    List<string> roleList = new List<string>();

                    string sql = $"SELECT CtsUser_Login, CtsUser_Domain, CtsRole_RoleName FROM CtsUserCtsRoles WHERE CtsUser_Login = '{users}'";

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

                                    if (loginDB.ToString().ToLower() == users.ToLower().ToString())
                                    {
                                        if (rolesDB.ToString().ToLower() == locationID.ToString().ToLower())
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
}
using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CTS_Models.DBContext;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CTS_Models.Role
{
    public class CtsUserCtsRoles
    {
        [Key]
        public string CtsUser_login { get; set; }
        public string CtsUser_Domain { get; set; }
        public string CtsRole_RoleName { get; set; }
    }
}

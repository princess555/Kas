using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTS_Models.Role
{
    public class Role
    {
        [Key]
        public int ID { get; set; }
        public string NameRoles { get; set; }
    }
}

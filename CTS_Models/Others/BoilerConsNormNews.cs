using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTS_Models.Others
{
    public class BoilerConsNormNews
    {
        [Key]
        public int ID { get; set; }
        public int CornsNorm { get; set; }

        public System.DateTime TimeData { get; set; }
        public string IDLocations { get; set; }
        public string Location_ID { get; set; }
    }
}

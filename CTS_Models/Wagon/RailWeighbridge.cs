using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTS_Models.Wagon
{
    [Table("RailWeighbridges")]
    public class RailWeighbridges 
    {
        [Key]
        public int ID { get; set; }

        public DateTime TransferTimeStamp { get; set; }
        public Double? Weight { get; set; }
    }
}

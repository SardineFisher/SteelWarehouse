using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelWarehouse.Domain
{
    public class SteelRoll
    {
        public int Id { get; set; }
        public double Weight { get; set; }
        public double Length { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateRemoved { get; set; }
    }
}

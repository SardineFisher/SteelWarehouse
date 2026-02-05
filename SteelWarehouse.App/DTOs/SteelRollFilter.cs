using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelWarehouse.App.DTOs
{
    public class SteelRollFilter
    {
        public int? MinId { get; set; }
        public int? MaxId { get; set; }

        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }

        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public DateTime? AddedFrom { get; set; }
        public DateTime? AddedTo { get; set; }

        public DateTime? RemovedFrom { get; set; }
        public DateTime? RemovedTo { get; set; }
    }
}

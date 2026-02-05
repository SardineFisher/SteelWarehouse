using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelWarehouse.App.DTOs
{
    public class WarehouseStatsDto
    {
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public double AvgWeight { get; set; }  
        public double MinWeight { get; set; }
        public double MaxWeight { get; set; }
        public double AvgLength { get; set; }
        public double MinLength { get; set; }
        public double MaxLength { get; set; }

        public double TotalWeightCurrent { get; set; }

        public TimeSpan? MinStorageDuration { get; set; }
        public TimeSpan? MaxStorageDuration { get; set; }

        public DateTime? DayWithMaxRolls { get; set; }
        public DateTime? DayWithMinRolls { get; set; }
        public DateTime? DayWithMaxWeight { get; set; }
        public DateTime? DayWithMinWeight { get; set; }



    }
}

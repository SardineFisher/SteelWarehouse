using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.Domain;

namespace SteelWarehouse.App.Interfaces
{
    public interface ISteelRollRepository
    {
        Task<SteelRoll> AddAsync(SteelRoll steelRoll);
        Task<IEnumerable<SteelRoll>> GetAllAsync(SteelRollFilter filter);
        Task<SteelRoll?> GetByIdAsync(int id);
        Task<SteelRoll> RemoveAsync(int id);

        Task<IEnumerable<SteelRoll>> GetRollsInPeriodAsync(DateTime from, DateTime to);
    }
}

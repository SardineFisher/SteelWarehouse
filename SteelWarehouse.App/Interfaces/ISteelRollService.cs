using SteelWarehouse.App.DTOs;
using SteelWarehouse.Domain;

namespace SteelWarehouse.App.Interfaces
{
    public interface ISteelRollService
    {
        Task<SteelRoll> AddAsync(double weight, double length);
        Task<SteelRoll> RemoveAsync(int id);
        Task<IEnumerable<SteelRoll>> GetAllAsync(SteelRollFilter filter);
        Task<WarehouseStatsDto> GetStatsAsync(DateTime from, DateTime to);
        Task<SteelRoll?> GetByIdAsync(int id);
    }
}
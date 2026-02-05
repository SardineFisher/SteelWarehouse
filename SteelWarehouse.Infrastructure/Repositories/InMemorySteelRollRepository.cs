using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.Domain;

namespace SteelWarehouse.Infrastructure.Repositories
{
    public class InMemorySteelRollRepository : ISteelRollRepository
    {
        private static readonly List<SteelRoll> _storage = new();

        private static int _lastId = 0;

        public Task<SteelRoll> AddAsync(SteelRoll steelRoll)
        {
            _lastId++;
            steelRoll.Id = _lastId;
            _storage.Add(steelRoll);

            return Task.FromResult(steelRoll);
        }

        public Task<SteelRoll?> GetByIdAsync(int id)
        {
            var roll = _storage.FirstOrDefault(r => r.Id == id);

            return Task.FromResult(roll);
        }

        public Task<SteelRoll> RemoveAsync(int id)
        {
            var roll = _storage.FirstOrDefault(r => r.Id == id && r.DateRemoved == null);

            if (roll == null)
            {
                throw new KeyNotFoundException($"Рулон с ID {id} не найден или уже удален");
            }
            roll.DateRemoved = DateTime.Now;

            return Task.FromResult(roll);
        }

        public Task<IEnumerable<SteelRoll>> GetRollsInPeriodAsync(DateTime from, DateTime to)
        {
            var result = _storage.Where(r =>
                r.DateAdded <= to &&
                (r.DateRemoved == null || r.DateRemoved >= from)
            );

            return Task.FromResult<IEnumerable<SteelRoll>>(result.ToList());
        }

        public Task<IEnumerable<SteelRoll>> GetAllAsync(SteelRollFilter filter)
        {
            IEnumerable<SteelRoll> query = _storage;

            if (filter.MinId.HasValue)
                query = query.Where(r => r.Id >= filter.MinId.Value);

            if (filter.MaxId.HasValue)
                query = query.Where(r => r.Id <= filter.MaxId.Value);

            if (filter.MinWeight.HasValue)
                query = query.Where(r => r.Weight >= filter.MinWeight.Value);

            if (filter.MaxWeight.HasValue)
                query = query.Where(r => r.Weight <= filter.MaxWeight.Value);

            if (filter.MinLength.HasValue)
                query = query.Where(r => r.Length >= filter.MinLength.Value);

            if (filter.MaxLength.HasValue)
                query = query.Where(r => r.Length <= filter.MaxLength.Value);

            if (filter.AddedFrom.HasValue)
                query = query.Where(r => r.DateAdded >= filter.AddedFrom.Value);

            if (filter.AddedTo.HasValue)
                query = query.Where(r => r.DateAdded <= filter.AddedTo.Value);

            if (filter.RemovedFrom.HasValue)
                query = query.Where(r => r.DateRemoved >= filter.RemovedFrom.Value);

            if (filter.RemovedTo.HasValue)
                query = query.Where(r => r.DateRemoved <= filter.RemovedTo.Value);

            return Task.FromResult<IEnumerable<SteelRoll>>(query.ToList());
        }
    }
}

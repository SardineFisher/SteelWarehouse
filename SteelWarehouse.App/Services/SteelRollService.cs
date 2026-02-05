using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.Domain;

namespace SteelWarehouse.App.Services
{
    public class SteelRollService : ISteelRollService
    {
        private readonly ISteelRollRepository _repository;

        public SteelRollService(ISteelRollRepository repository)
        {
            _repository = repository;
        }

        public Task<SteelRoll> AddAsync(double weight, double length)
        {
            if (weight <= 0 || length <= 0)
            {
                throw new ArgumentException("Вес и длина должны быть положительны (>0)");
            }

            var steelRoll = new SteelRoll
            {
                Weight = weight,
                Length = length,
                DateAdded = DateTime.Now
            };

            return _repository.AddAsync(steelRoll);
        }

        public Task<SteelRoll> RemoveAsync(int id)
        {
            return _repository.RemoveAsync(id);
        }

        public Task<IEnumerable<SteelRoll>> GetAllAsync(SteelRollFilter filter)
        {
            return _repository.GetAllAsync(filter);
        }

        public async Task<WarehouseStatsDto> GetStatsAsync(DateTime from, DateTime to)
        {
            var allRolls = (await _repository.GetRollsInPeriodAsync(from, to)).ToList();
            if (!allRolls.Any()) return new WarehouseStatsDto();

            var addedInPeriod = allRolls.Count(r => r.DateAdded >= from && r.DateAdded <= to);
            var removedInPeriod = allRolls.Count(r => r.DateRemoved >= from && r.DateRemoved <= to);

            var avgWeight = allRolls.Average(r => r.Weight);
            var minWeight = allRolls.Min(r => r.Weight);
            var maxWeight = allRolls.Max(r => r.Weight);

            var avgLength = allRolls.Average(r => r.Length);
            var minLength = allRolls.Min(r => r.Length);
            var maxLength = allRolls.Max(r => r.Length);


            var calcEndDate = to > DateTime.Now ? DateTime.Now : to;
            var durations = allRolls.Select(r =>
            {
                var effectiveEnd = (r.DateRemoved.HasValue && r.DateRemoved < calcEndDate)
                                    ? r.DateRemoved.Value
                                    : calcEndDate;

                return effectiveEnd - r.DateAdded;
            }).ToList();
            var minDuration = durations.Min();
            var maxDuration = durations.Max();

            var totalWeightCurrent = allRolls.Sum(r => r.Weight);

            // Статистика за дни
            var dailyStats = new List<(DateTime Date, int Count, double TotalWeight)>();
            for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
            {
                var rollsOnDay = allRolls.Where(r =>
                    r.DateAdded.Date <= day &&
                    (r.DateRemoved == null || r.DateRemoved.Value.Date >= day)
                ).ToList();

                dailyStats.Add((day, rollsOnDay.Count, rollsOnDay.Sum(r => r.Weight)));
            }

            var maxRollsDay = dailyStats
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Date)
                .First().Date;

            var minRollsDay = dailyStats
                .OrderBy(x => x.Count)
                .ThenBy(x => x.Date)
                .First().Date;

            var maxWeightDay = dailyStats
                .OrderByDescending(x => x.TotalWeight)
                .ThenBy(x => x.Date)
                .First().Date;

            var minWeightDay = dailyStats
                .OrderBy(x => x.TotalWeight)
                .ThenBy(x => x.Date)
                .First().Date;

            return new WarehouseStatsDto
            {
                AddedCount = addedInPeriod,
                RemovedCount = removedInPeriod,
                AvgWeight = avgWeight,
                MinWeight = minWeight,
                MaxWeight = maxWeight,
                AvgLength = avgLength,
                MinLength = minLength,
                MaxLength = maxLength,
                TotalWeightCurrent = totalWeightCurrent,
                MinStorageDuration = minDuration,
                MaxStorageDuration = maxDuration,
                DayWithMaxRolls = maxRollsDay,
                DayWithMinRolls = minRollsDay,
                DayWithMaxWeight = maxWeightDay,
                DayWithMinWeight = minWeightDay
            };
        }

        public Task<SteelRoll?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }
    }
}

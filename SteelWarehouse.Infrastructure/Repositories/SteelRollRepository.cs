using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.Domain;

namespace SteelWarehouse.Infrastructure.Repositories
{
    public class SteelRollRepository : ISteelRollRepository
    {
        private readonly string _connectionString;
        public SteelRollRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<SteelRoll> AddAsync(SteelRoll steelRoll)
        {
            const string query = 
                """
                INSERT INTO steel_rolls (weight, length, date_added)
                VALUES (@Weight, @Length, @DateAdded)
                RETURNING *;
                """;

            using var db = GetConnection();
            return await db.QuerySingleAsync<SteelRoll>(query, steelRoll);
        }

        public async Task<IEnumerable<SteelRoll>> GetAllAsync(SteelRollFilter filter)
        {
            var query = new StringBuilder("SELECT * FROM steel_rolls WHERE 1=1");
            var parameters = new DynamicParameters();

            if (filter.MinId.HasValue)
            {
                query.Append(" AND id >= @MinId");
                parameters.Add("MinId", filter.MinId.Value);
            }
            if (filter.MaxId.HasValue)
            {
                query.Append(" AND id <= @MaxId");
                parameters.Add("MaxId", filter.MaxId.Value);
            }
            if (filter.MinWeight.HasValue)
            {
                query.Append(" AND weight >= @MinWeight");
                parameters.Add("MinWeight", filter.MinWeight.Value);
            }
            if (filter.MaxWeight.HasValue)
            {
                query.Append(" AND weight <= @MaxWeight");
                parameters.Add("MaxWeight", filter.MaxWeight.Value);
            }
            if (filter.MinLength.HasValue)
            {
                query.Append(" AND length >= @MinLength");
                parameters.Add("MinLength", filter.MinLength.Value);
            }
            if (filter.MaxLength.HasValue)
            {
                query.Append(" AND length <= @MaxLength");
                parameters.Add("MaxLength", filter.MaxLength.Value);
            }
            if (filter.AddedFrom.HasValue)
            {
                query.Append(" AND date_added >= @AddedFrom");
                parameters.Add("AddedFrom", filter.AddedFrom.Value);
            }
            if (filter.AddedTo.HasValue)
            {
                query.Append(" AND date_added <= @AddedTo");
                parameters.Add("AddedTo", filter.AddedTo.Value);
            }
            if (filter.RemovedFrom.HasValue)
            {
                query.Append(" AND date_removed >= @RemovedFrom");
                parameters.Add("RemovedFrom", filter.RemovedFrom.Value);
            }
            if (filter.RemovedTo.HasValue)
            {
                query.Append(" AND date_removed <= @RemovedTo");
                parameters.Add("RemovedTo", filter.RemovedTo.Value);
            }

            using var db = GetConnection();
            var result = await db.QueryAsync<SteelRoll>(query.ToString(), parameters);

            return result.ToList();
        }

        public async Task<SteelRoll> RemoveAsync(int id)
        {
            var currentTime = DateTime.Now;

            const string query =
                """
                UPDATE steel_rolls
                SET date_removed = @CurrentTime
                WHERE id = @id and date_removed IS NULL
                RETURNING *;
                """;

            using var db = GetConnection();
            var result = await db.QuerySingleOrDefaultAsync<SteelRoll>(query, new { id, CurrentTime = currentTime });

            if (result == null)
            {
                throw new KeyNotFoundException($"Рулон с id: {id} не найден или уже удален.");
            }

            return result;
        }

        public async Task<SteelRoll?> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM steel_rolls WHERE id = @Id;";
            
            using var db = GetConnection();
            return await db.QuerySingleOrDefaultAsync<SteelRoll>(query, new { Id = id });
        }

        public async Task<IEnumerable<SteelRoll>> GetRollsInPeriodAsync(DateTime from, DateTime to)
        {
            const string query =
                """
                SELECT * FROM steel_rolls 
                WHERE date_added <= @To 
                AND (date_removed IS NULL OR date_removed >= @From)
                """;

            using var db = GetConnection();
            return await db.QueryAsync<SteelRoll>(query, new { From = from, To = to });
        }
    }
}

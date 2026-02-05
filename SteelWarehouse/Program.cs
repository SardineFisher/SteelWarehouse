using System.Reflection;
using DbUp;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.App.Services;
using SteelWarehouse.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Конфигурация типа хранилища и строки подключения
var storageType = builder.Configuration["Storage:Type"];
var connectionString = builder.Configuration.GetConnectionString("Default");

// Инициализация таблицы в PostgreSQL при старте приложения
if (storageType == "Postgres")
{
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Строка подключения пустая");

    EnsureDatabase.For.PostgresqlDatabase(connectionString);

    var upgrader = DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .LogToConsole()
        .Build();

    int retries = 10;
    while (retries-- > 0)
    {
        if (upgrader.TryConnect(out _))
        {
            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
                throw result.Error;

            break;
        }

        Thread.Sleep(2000);
    }
}

builder.Services.AddScoped<ISteelRollService, SteelRollService>();

builder.Services.AddScoped<ISteelRollRepository>(sp =>
{
    return storageType switch
    {
        "InMemory" => new InMemorySteelRollRepository(),

        "Postgres" => new SteelRollRepository(connectionString),

        _ => throw new InvalidOperationException(
            $"Unknown storage type: {storageType}")
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

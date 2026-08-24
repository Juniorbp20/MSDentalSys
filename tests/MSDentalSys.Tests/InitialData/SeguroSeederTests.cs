using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MSDentalSys.Data.Context;
using MSDentalSys.Data.InitialData;
using MSDentalSys.Data.Models;
using Xunit;

namespace MSDentalSys.Tests.InitialData;

public class SeguroSeederTests
{
    [Fact]
    public async Task SeedAsync_CreaLosSegurosOficialesCuandoNoExisten()
    {
        await using var database = await TestDatabase.CreateAsync();

        await SeguroSeeder.SeedAsync(database.Context);

        var seguros = await database.Context.Seguros.ToListAsync();
        Assert.Equal(SeguroSeeder.NombresOficiales.Count, seguros.Count);
        Assert.All(seguros, seguro => Assert.True(seguro.Estado));
    }

    [Fact]
    public async Task SeedAsync_EjecutadoDosVeces_NoDuplicaRegistros()
    {
        await using var database = await TestDatabase.CreateAsync();

        await SeguroSeeder.SeedAsync(database.Context);
        await SeguroSeeder.SeedAsync(database.Context);

        Assert.Equal(SeguroSeeder.NombresOficiales.Count, await database.Context.Seguros.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_ConNombreExistente_NoCreaDuplicado()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Seguros.Add(new Seguro { Nombre = "ARS CMD", Estado = false });
        await database.Context.SaveChangesAsync();

        await SeguroSeeder.SeedAsync(database.Context);

        Assert.Equal(1, await database.Context.Seguros.CountAsync(seguro => seguro.Nombre == "ARS CMD"));
        var existing = await database.Context.Seguros.SingleAsync(seguro => seguro.Nombre == "ARS CMD");
        Assert.False(existing.Estado);
    }

    [Fact]
    public async Task SeedAsync_ConservaSeguroManualDiferente()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Seguros.Add(new Seguro { Nombre = "Seguro manual", Estado = false });
        await database.Context.SaveChangesAsync();

        await SeguroSeeder.SeedAsync(database.Context);

        var manual = await database.Context.Seguros.SingleAsync(seguro => seguro.Nombre == "Seguro manual");
        Assert.False(manual.Estado);
    }

    [Fact]
    public async Task SeedAsync_NoEliminaRegistrosExistentes()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Seguros.Add(new Seguro { Nombre = "Seguro manual" });
        await database.Context.SaveChangesAsync();

        await SeguroSeeder.SeedAsync(database.Context);

        Assert.Contains(await database.Context.Seguros.ToListAsync(), seguro => seguro.Nombre == "Seguro manual");
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, ApplicationDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}

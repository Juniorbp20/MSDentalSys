using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MSDentalSys.Data.Context;
using MSDentalSys.Data.Models;
using MSDentalSys.Web.Controllers;
using MSDentalSys.Web.Models.ViewModels;
using Xunit;

namespace MSDentalSys.Tests.Controllers;

public class SegurosControllerTests
{
    [Fact]
    public async Task Index_Administrador_PuedeListar()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Seguros.Add(database.CreateSeguro("Seguro Uno"));
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IReadOnlyList<Seguro>>(view.Model);
        Assert.Single(model);
        Assert.Equal("Seguro Uno", model[0].Nombre);
    }

    [Fact]
    public async Task Create_Administrador_CreaSeguroActivo()
    {
        await using var database = await TestDatabase.CreateAsync();
        var controller = database.CreateController();

        var result = await controller.Create(new SeguroFormViewModel { Nombre = "Seguro Uno" });

        Assert.IsType<RedirectToActionResult>(result);
        var seguro = await database.Context.Seguros.SingleAsync();
        Assert.Equal("Seguro Uno", seguro.Nombre);
        Assert.True(seguro.Estado);
        Assert.NotEqual(default, seguro.FechaCreacion);
    }

    [Fact]
    public async Task Create_NombreObligatorio_NoCreaSeguro()
    {
        await using var database = await TestDatabase.CreateAsync();
        var controller = database.CreateController();

        var result = await controller.Create(new SeguroFormViewModel { Nombre = "   " });

        Assert.IsType<ViewResult>(result);
        Assert.Empty(await database.Context.Seguros.ToListAsync());
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_NombreDuplicado_EsRechazado()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Seguros.Add(database.CreateSeguro("Seguro Uno"));
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Create(new SeguroFormViewModel { Nombre = "Seguro Uno" });

        Assert.IsType<ViewResult>(result);
        Assert.Single(await database.Context.Seguros.ToListAsync());
        Assert.Contains("Ya existe", controller.ModelState[nameof(SeguroFormViewModel.Nombre)]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task Edit_Administrador_ActualizaNombreYConservaEstadoYFecha()
    {
        await using var database = await TestDatabase.CreateAsync();
        var originalDate = new DateTime(2030, 1, 1, 8, 30, 0);
        var seguro = database.CreateSeguro("Seguro Inicial", false, originalDate);
        database.Context.Seguros.Add(seguro);
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Edit(seguro.SeguroId, new SeguroFormViewModel
        {
            SeguroId = seguro.SeguroId,
            Nombre = "Seguro Actualizado"
        });

        Assert.IsType<RedirectToActionResult>(result);
        var stored = await database.Context.Seguros.SingleAsync();
        Assert.Equal("Seguro Actualizado", stored.Nombre);
        Assert.False(stored.Estado);
        Assert.Equal(originalDate, stored.FechaCreacion);
    }

    [Fact]
    public async Task Deactivate_ConservaRegistroYLoMarcaInactivo()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seguro = database.CreateSeguro("Seguro Activo");
        database.Context.Seguros.Add(seguro);
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Deactivate(seguro.SeguroId);

        Assert.IsType<RedirectToActionResult>(result);
        var stored = await database.Context.Seguros.SingleAsync();
        Assert.False(stored.Estado);
        Assert.Equal(seguro.SeguroId, stored.SeguroId);
        Assert.Equal(1, await database.Context.Seguros.CountAsync());
    }

    [Fact]
    public async Task Activate_ConservaRegistroYLoMarcaActivo()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seguro = database.CreateSeguro("Seguro Inactivo", false);
        database.Context.Seguros.Add(seguro);
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Activate(seguro.SeguroId);

        Assert.IsType<RedirectToActionResult>(result);
        var stored = await database.Context.Seguros.SingleAsync();
        Assert.True(stored.Estado);
        Assert.Equal("Seguro Inactivo", stored.Nombre);
        Assert.Equal(1, await database.Context.Seguros.CountAsync());
    }

    [Fact]
    public async Task Details_Administrador_PuedeConsultarSeguro()
    {
        await using var database = await TestDatabase.CreateAsync();
        var seguro = database.CreateSeguro("Seguro Uno");
        database.Context.Seguros.Add(seguro);
        await database.Context.SaveChangesAsync();
        var controller = database.CreateController();

        var result = await controller.Details(seguro.SeguroId);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(seguro.SeguroId, Assert.IsType<Seguro>(view.Model).SeguroId);
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

        public SegurosController CreateController()
        {
            var httpContext = new DefaultHttpContext();
            var controller = new SegurosController(Context)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, new NullTempDataProvider())
            };
            return controller;
        }

        public Seguro CreateSeguro(string name, bool state = true, DateTime? createdAt = null)
        {
            return new Seguro
            {
                Nombre = name,
                Estado = state,
                FechaCreacion = createdAt ?? new DateTime(2030, 1, 1, 8, 0, 0)
            };
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) => new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
        }
    }
}

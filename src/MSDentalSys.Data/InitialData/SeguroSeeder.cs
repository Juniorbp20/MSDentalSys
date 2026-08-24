using Microsoft.EntityFrameworkCore;
using MSDentalSys.Data.Context;
using MSDentalSys.Data.Models;

namespace MSDentalSys.Data.InitialData;

public static class SeguroSeeder
{
    public static IReadOnlyList<string> NombresOficiales { get; } =
    [
        "ARS Banco Central",
        "ARS CMD",
        "ARS GMA",
        "ARS MAPFRE",
        "ARS Monumental",
        "ARS Primera",
        "ARS Renacer",
        "ARS Reservas",
        "SeNaSa"
    ];

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        foreach (var nombre in NombresOficiales)
        {
            if (await context.Seguros.AnyAsync(seguro => seguro.Nombre == nombre))
            {
                continue;
            }

            context.Seguros.Add(new Seguro
            {
                Nombre = nombre,
                Estado = true,
                FechaCreacion = DateTime.Now
            });
        }

        await context.SaveChangesAsync();
    }
}

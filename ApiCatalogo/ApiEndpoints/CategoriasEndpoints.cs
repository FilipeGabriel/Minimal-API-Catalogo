using ApiCatalogo.Context;
using ApiCatalogo.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogo.ApiEndpoints;

public static class CategoriasEndpoints
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        app.MapGet("/categorias", async (AppDbContext context) => await context.Categorias.ToListAsync()).WithTags("Categorias").RequireAuthorization();

        app.MapGet("/categorias/{id:int}", async (AppDbContext context, int id) =>
        {
            return await context.Categorias.FindAsync(id)
                                  is Categoria categoria
                                  ? Results.Ok(categoria)
                                  : Results.NotFound($"Categoria com ID {id} não encontrada.");
        });

        app.MapPost("/categorias", async (AppDbContext context, Categoria categoria) =>
        {
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
        });

        app.MapPut("/categorias/{id:int}", async (AppDbContext context, Categoria categoria, int id) =>
        {
            if (categoria.CategoriaId != id)
                return Results.BadRequest("ID inválido.");

            var categoriaDb = await context.Categorias.FindAsync(id);

            if (categoriaDb is null)
                return Results.NotFound($"Categoria com ID {id} não encontrada.");

            categoriaDb.Nome = categoria.Nome;
            categoriaDb.Descricao = categoria.Descricao;

            await context.SaveChangesAsync();
            return Results.Ok(categoriaDb);
        });

        app.MapDelete("/categorias/{id:int}", async (AppDbContext context, int id) =>
        {
            var categoria = await context.Categorias.FindAsync(id);

            if (categoria is null)
                return Results.NotFound($"Categoria com ID {id} não encontrada.");

            context.Categorias.Remove(categoria);
            await context.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}

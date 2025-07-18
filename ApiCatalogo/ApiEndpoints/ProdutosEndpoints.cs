using ApiCatalogo.Context;
using ApiCatalogo.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogo.ApiEndpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this WebApplication app)
    {
        app.MapGet("/produtos", async (AppDbContext context) => await context.Produtos.ToListAsync()).WithTags("Categorias").RequireAuthorization();

        app.MapGet("/produtos/{id:int}", async (AppDbContext context, int id) =>
        {
            return await context.Produtos.FindAsync(id)
                                  is Produto produto
                                  ? Results.Ok(produto)
                                  : Results.NotFound($"Produto com ID {id} não encontrado.");
        });

        app.MapPost("/produtos", async (AppDbContext context, Produto produto) =>
        {
            context.Produtos.Add(produto);
            await context.SaveChangesAsync();

            return Results.Created($"/produtos/{produto.CategoriaId}", produto);
        });

        app.MapPut("/produtos/{id:int}", async (AppDbContext context, Produto produto, int id) =>
        {
            if (produto.ProdutoId != id)
                return Results.BadRequest("ID inválido.");

            var produtoDb = await context.Produtos.FindAsync(id);

            if (produtoDb is null)
                return Results.NotFound($"Produto com ID {id} não encontrado.");

            produtoDb.Nome = produto.Nome;
            produtoDb.Descricao = produto.Descricao;
            produtoDb.Preco = produto.Preco;
            produtoDb.Imagem = produto.Imagem;
            produtoDb.DataCompra = produto.DataCompra;
            produtoDb.Estoque = produto.Estoque;
            produtoDb.CategoriaId = produto.CategoriaId;

            await context.SaveChangesAsync();
            return Results.Ok(produtoDb);
        });

        app.MapDelete("/produtos/{id:int}", async (AppDbContext context, int id) =>
        {
            var produto = await context.Produtos.FindAsync(id);

            if (produto is null)
                return Results.NotFound($"Produto com ID {id} não encontrado.");

            context.Produtos.Remove(produto);

            await context.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

using ApiCatalogo.Context;
using ApiCatalogo.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

app.MapGet("/", () => "Catálogo de Produtos - 2025");

app.MapGet("/categorisas", async (AppDbContext context) => await context.Categorias.ToListAsync());

app.MapGet("/categoria/{id:int}", async (AppDbContext context, int id) =>
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

using ApiCatalogo.Context;
using ApiCatalogo.Models;
using ApiCatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme. Enter 'Bearer'[space].Exemple: \'Bearer 12345abcdef\'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//-------------- Endpoints para Login --------------//

app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel is null)
        return Results.BadRequest("Login inválido.");

    if (userModel.UserName == "filipe" && userModel.Password == "123456")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:Key"],
                                                  app.Configuration["Jwt:Issuer"],
                                                  app.Configuration["Jwt:Audience"],
                                                  userModel);
        return Results.Ok(new { Token = tokenString });
    }
    else
    {
        return Results.BadRequest("Login inválido.");
    }
}).Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .WithName("Login")
            .WithTags("Autenticação");

//-------------- Endpoints para Categorias --------------//

app.MapGet("/", () => "Catálogo de Produtos - 2025");

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

//-------------- Endpoints para Produtos --------------//

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();

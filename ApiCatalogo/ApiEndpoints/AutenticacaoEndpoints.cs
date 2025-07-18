using ApiCatalogo.Models;
using ApiCatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;

namespace ApiCatalogo.ApiEndpoints;

public static class AutenticacaoEndpoints
{
    public static void MapAutenticacaoEndpoints(this WebApplication app)
    {
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
    }
}

using System.Security.Claims;
using FeteCam.Api.Data;
using FeteCam.Api.Dtos;
using FeteCam.Api.Models;
using FeteCam.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeteCam.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/auth");

        g.MapPost("/register", async (RegisterRequest req, AppDbContext db, PasswordHasher hasher, TokenService tokens) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return Results.BadRequest(new { error = "Adresse e-mail invalide." });
            if (req.Password.Length < 6)
                return Results.BadRequest(new { error = "Le mot de passe doit contenir au moins 6 caractères." });
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Le nom est requis." });
            if (await db.Users.AnyAsync(u => u.Email == email))
                return Results.Conflict(new { error = "Un compte existe déjà avec cette adresse." });

            var user = new User
            {
                Email = email,
                Name = req.Name.Trim(),
                Phone = req.Phone?.Trim() ?? "",
                City = req.City?.Trim() ?? "",
                PasswordHash = hasher.Hash(req.Password)
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Ok(new AuthResponse(tokens.CreateToken(user), ToDto(user)));
        });

        g.MapPost("/login", async (LoginRequest req, AppDbContext db, PasswordHasher hasher, TokenService tokens) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null || !hasher.Verify(req.Password, user.PasswordHash))
                return Results.Json(new { error = "E-mail ou mot de passe incorrect." }, statusCode: 401);

            return Results.Ok(new AuthResponse(tokens.CreateToken(user), ToDto(user)));
        });

        g.MapGet("/me", [Authorize] async (ClaimsPrincipal principal, AppDbContext db) =>
        {
            var user = await db.Users.FindAsync(principal.GetUserId());
            return user is null ? Results.Unauthorized() : Results.Ok(ToDto(user));
        });
    }

    public static int GetUserId(this ClaimsPrincipal principal)
        => int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? principal.FindFirstValue("sub") ?? "0");

    public static UserDto ToDto(User u) => new(u.Id, u.Email, u.Name, u.Phone, u.City);
}

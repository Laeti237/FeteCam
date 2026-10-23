using System.Security.Claims;
using FeteCam.Api.Data;
using FeteCam.Api.Dtos;
using FeteCam.Api.Models;
using FeteCam.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeteCam.Api.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitations(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/invitations").RequireAuthorization();

        g.MapPost("/", async (CreateInvitationRequest req, ClaimsPrincipal principal, AppDbContext db, LinkBuilder links) =>
        {
            var evt = await db.Events.FindAsync(req.EventId);
            if (evt is null) return Results.NotFound(new { error = "Événement introuvable." });

            var inviterId = principal.GetUserId();
            var inv = new Invitation
            {
                Token = InviteTokens.NewToken(),
                EventId = evt.Id,
                EventName = evt.Name,
                Role = string.IsNullOrWhiteSpace(req.Role) ? "Membre du comité" : req.Role.Trim(),
                TaskTitle = string.IsNullOrWhiteSpace(req.TaskTitle) ? null : req.TaskTitle.Trim(),
                InviteeName = string.IsNullOrWhiteSpace(req.InviteeName) ? null : req.InviteeName.Trim(),
                InviterUserId = inviterId,
                Status = "En attente"
            };
            db.Invitations.Add(inv);
            await db.SaveChangesAsync();

            return Results.Ok(ToDto(inv, links));
        });

        g.MapGet("/sent", async (ClaimsPrincipal principal, AppDbContext db, LinkBuilder links) =>
        {
            var id = principal.GetUserId();
            var list = await db.Invitations
                .Where(i => i.InviterUserId == id)
                .OrderByDescending(i => i.CreatedAt)
                .Include(i => i.Invitee)
                .Include(i => i.Inviter)
                .ToListAsync();
            return Results.Ok(list.Select(i => ToDto(i, links)));
        });

        g.MapGet("/received", async (ClaimsPrincipal principal, AppDbContext db, LinkBuilder links) =>
        {
            var id = principal.GetUserId();
            var list = await db.Invitations
                .Where(i => i.InviteeUserId == id)
                .OrderByDescending(i => i.CreatedAt)
                .Include(i => i.Inviter)
                .Include(i => i.Invitee)
                .ToListAsync();
            return Results.Ok(list.Select(i => ToDto(i, links)));
        });

        app.MapGet("/api/invitations/by-token/{token}", async (string token, ClaimsPrincipal principal, AppDbContext db) =>
        {
            var inv = await db.Invitations.Include(i => i.Inviter)
                .FirstOrDefaultAsync(i => i.Token == token);
            if (inv is null) return Results.NotFound(new { error = "Invitation introuvable ou expirée." });

            var userId = principal.GetUserId();
            var member = await db.Memberships.AnyAsync(m => m.EventId == inv.EventId && m.UserId == userId);
            var canRespond = inv.Status == "En attente" && inv.InviterUserId != userId;
            return Results.Ok(new InvitationDetailDto(
                inv.Token, inv.EventName, inv.Role, inv.TaskTitle, inv.Status,
                inv.Inviter?.Name ?? "Organisateur", canRespond, member));
        });

        g.MapPost("/by-token/{token}/accept", async (string token, ClaimsPrincipal principal, AppDbContext db) =>
            await RespondAsync(token, principal, db, accept: true));

        g.MapPost("/by-token/{token}/decline", async (string token, ClaimsPrincipal principal, AppDbContext db) =>
            await RespondAsync(token, principal, db, accept: false));

        g.MapDelete("/{id:int}", async (int id, ClaimsPrincipal principal, AppDbContext db) =>
        {
            var inv = await db.Invitations.FindAsync(id);
            if (inv is null) return Results.NotFound();
            if (inv.InviterUserId != principal.GetUserId()) return Results.Forbid();
            db.Invitations.Remove(inv);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static async Task<IResult> RespondAsync(string token, ClaimsPrincipal principal, AppDbContext db, bool accept)
    {
        var inv = await db.Invitations.FirstOrDefaultAsync(i => i.Token == token);
        if (inv is null) return Results.NotFound(new { error = "Invitation introuvable." });

        var userId = principal.GetUserId();
        if (inv.InviterUserId == userId)
            return Results.BadRequest(new { error = "Vous ne pouvez pas répondre à votre propre invitation." });
        if (inv.Status != "En attente")
            return Results.Conflict(new { error = "Cette invitation a déjà été traitée." });

        inv.InviteeUserId = userId;
        inv.RespondedAt = DateTime.UtcNow;
        inv.Status = accept ? "Acceptée" : "Refusée";

        if (accept)
        {
            var already = await db.Memberships.AnyAsync(m => m.EventId == inv.EventId && m.UserId == userId);
            if (!already)
                db.Memberships.Add(new Membership
                {
                    EventId = inv.EventId,
                    UserId = userId,
                    Role = inv.Role,
                    Status = "Confirmé"
                });
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { status = inv.Status });
    }

    public static InvitationDto ToDto(Invitation i, LinkBuilder links) => new(
        i.Id, i.Token, links.Web(i.Token), links.App(i.Token),
        i.EventId, i.EventName, i.Role, i.TaskTitle, i.Status,
        i.Inviter?.Name ?? "Organisateur",
        i.Invitee?.Name ?? i.InviteeName, i.CreatedAt, i.RespondedAt);
}

public class LinkBuilder(IConfiguration config)
{
    private readonly string _base = config["PublicBaseUrl"] ?? "http://localhost:5210";
    public string Web(string token) => $"{_base}/i/{token}";
    public string App(string token) => $"fetecam://invite/{token}";
}

using System.Security.Claims;
using FeteCam.Api.Data;
using FeteCam.Api.Dtos;
using FeteCam.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FeteCam.Api.Endpoints;

public static class CoreEndpoints
{
    public static void MapCore(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/api/events").RequireAuthorization();

        events.MapGet("/", async (ClaimsPrincipal p, AppDbContext db) =>
        {
            var id = p.GetUserId();
            var own = await db.Events.Where(e => e.OwnerUserId == id).OrderByDescending(e => e.CreatedAt).ToListAsync();
            var invited = await (
                from i in db.Invitations
                where i.InviteeUserId == id && (i.Status == "Acceptée")
                join e in db.Events on i.EventId equals e.Id
                orderby e.CreatedAt descending
                select e).ToListAsync();
            var all = own.Concat(invited.Where(e => !own.Any(o => o.Id == e.Id))).ToList();

            var result = new List<EventDto>();
            foreach (var e in all)
            {
                var role = e.OwnerUserId == id ? "Organisateur"
                    : await (from m in db.Memberships
                             where m.EventId == e.Id && m.UserId == id
                             select m.Role).FirstOrDefaultAsync() ?? "Membre";
                result.Add(new EventDto(e.Id, e.Name, e.Date, e.Location, e.Status, role));
            }
            return Results.Ok(result);
        });

        events.MapPost("/", async (CreateEventRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Le nom de l'événement est requis." });
            var e = new EventRecord
            {
                Name = req.Name.Trim(),
                Date = req.Date,
                Location = req.Location?.Trim() ?? "",
                Status = string.IsNullOrWhiteSpace(req.Status) ? "En préparation" : req.Status,
                OwnerUserId = p.GetUserId()
            };
            db.Events.Add(e);
            await db.SaveChangesAsync();
            return Results.Ok(new EventDto(e.Id, e.Name, e.Date, e.Location, e.Status, "Organisateur"));
        });

        events.MapPut("/{id:int}", async (int id, UpdateEventRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            var e = await db.Events.FindAsync(id);
            if (e is null) return Results.NotFound(new { error = "Événement introuvable." });
            if (e.OwnerUserId != p.GetUserId()) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Le nom de l'événement est requis." });
            e.Name = req.Name.Trim();
            e.Date = req.Date;
            e.Location = req.Location?.Trim() ?? "";
            e.Status = string.IsNullOrWhiteSpace(req.Status) ? e.Status : req.Status;
            await db.SaveChangesAsync();
            return Results.Ok(new EventDto(e.Id, e.Name, e.Date, e.Location, e.Status, "Organisateur"));
        });

        events.MapDelete("/{id:int}", async (int id, ClaimsPrincipal p, AppDbContext db) =>
        {
            var e = await db.Events.FindAsync(id);
            if (e is null) return Results.NotFound();
            if (e.OwnerUserId != p.GetUserId()) return Results.Forbid();
            db.Events.Remove(e);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        events.MapGet("/{id:int}/committee", async (int id, AppDbContext db) =>
        {
            var members = await db.Memberships.Where(m => m.EventId == id)
                .Include(m => m.User)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();
            return Results.Ok(members.Select(m => new MembershipDto(
                m.Id, m.EventId, m.UserId, m.User?.Name ?? "", m.User?.Phone ?? "",
                m.Role, m.Status, m.JoinedAt)));
        });

        events.MapDelete("/{id:int}/committee/{membershipId:int}", async (int id, int membershipId, ClaimsPrincipal p, AppDbContext db) =>
        {
            var evt = await db.Events.FindAsync(id);
            if (evt is null) return Results.NotFound(new { error = "Événement introuvable." });
            var userId = p.GetUserId();
            var m = await db.Memberships.FindAsync(membershipId);
            if (m is null || m.EventId != id) return Results.NotFound(new { error = "Membre introuvable." });
            if (evt.OwnerUserId != userId && m.UserId != userId) return Results.Forbid();
            db.Memberships.Remove(m);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        var guests = app.MapGroup("/api/events/{eventId:int}/guests").RequireAuthorization();

        guests.MapGet("/", async (int eventId, AppDbContext db) =>
        {
            var list = await db.Guests.Where(g => g.EventId == eventId).OrderBy(g => g.Name).ToListAsync();
            return Results.Ok(list.Select(ToDto).ToList());
        });

        guests.MapPost("/", async (int eventId, GuestRecord guest, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            guest.Id = 0;
            guest.EventId = eventId;
            db.Guests.Add(guest);
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(guest));
        });

        guests.MapPut("/{guestId:int}", async (int eventId, int guestId, UpdateGuestRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            var g = await db.Guests.FindAsync(guestId);
            if (g is null || g.EventId != eventId) return Results.NotFound(new { error = "Invité introuvable." });
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Le nom de l'invité est requis." });
            g.Name = req.Name.Trim();
            g.Phone = req.Phone?.Trim() ?? "";
            g.Category = string.IsNullOrWhiteSpace(req.Category) ? g.Category : req.Category;
            g.Status = string.IsNullOrWhiteSpace(req.Status) ? g.Status : req.Status;
            g.PersonCount = req.PersonCount <= 0 ? 1 : req.PersonCount;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(g));
        });

        guests.MapDelete("/{guestId:int}", async (int eventId, int guestId, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            var g = await db.Guests.FindAsync(guestId);
            if (g is null || g.EventId != eventId) return Results.NotFound(new { error = "Invité introuvable." });
            db.Guests.Remove(g);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        var tasks = app.MapGroup("/api/events/{eventId:int}/tasks").RequireAuthorization();

        tasks.MapGet("/", async (int eventId, AppDbContext db) =>
        {
            var list = await db.Tasks.Where(t => t.EventId == eventId).OrderBy(t => t.DueDate).ToListAsync();
            return Results.Ok(list.Select(ToDto).ToList());
        });

        tasks.MapPost("/", async (int eventId, TaskRecord task, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            task.Id = 0;
            task.EventId = eventId;
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(task));
        });

        tasks.MapPut("/{taskId:int}", async (int eventId, int taskId, UpdateTaskRequest req, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            var t = await db.Tasks.FindAsync(taskId);
            if (t is null || t.EventId != eventId) return Results.NotFound(new { error = "Tâche introuvable." });
            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest(new { error = "Le titre de la tâche est requis." });
            t.Title = req.Title.Trim();
            t.Category = string.IsNullOrWhiteSpace(req.Category) ? t.Category : req.Category;
            t.DueDate = req.DueDate;
            t.DueLabel = req.DueLabel;
            t.Status = string.IsNullOrWhiteSpace(req.Status) ? t.Status : req.Status;
            t.AssigneeName = req.AssigneeName?.Trim() ?? "";
            t.Details = req.Details?.Trim() ?? "";
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(t));
        });

        tasks.MapDelete("/{taskId:int}", async (int eventId, int taskId, ClaimsPrincipal p, AppDbContext db) =>
        {
            if (!await CanManageAsync(db, eventId, p.GetUserId())) return Results.Forbid();
            var t = await db.Tasks.FindAsync(taskId);
            if (t is null || t.EventId != eventId) return Results.NotFound(new { error = "Tâche introuvable." });
            db.Tasks.Remove(t);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
    }

    private static async Task<bool> CanManageAsync(AppDbContext db, int eventId, int userId)
    {
        var evt = await db.Events.FindAsync(eventId);
        if (evt is null) return false;
        if (evt.OwnerUserId == userId) return true;
        return await db.Memberships.AnyAsync(m => m.EventId == eventId && m.UserId == userId);
    }

    private static GuestDto ToDto(GuestRecord g) =>
        new(g.Id, g.EventId, g.Name, g.Phone, g.Category, g.Status, g.PersonCount);

    private static TaskDto ToDto(TaskRecord t) =>
        new(t.Id, t.EventId, t.Title, t.Category, t.DueDate, t.DueLabel, t.Status, t.AssigneeName, t.Details);
}

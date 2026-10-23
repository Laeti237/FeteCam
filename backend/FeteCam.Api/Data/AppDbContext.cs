using FeteCam.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FeteCam.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<EventRecord> Events => Set<EventRecord>();
    public DbSet<GuestRecord> Guests => Set<GuestRecord>();
    public DbSet<TaskRecord> Tasks => Set<TaskRecord>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Invitation>().HasIndex(i => i.Token).IsUnique();
        mb.Entity<Membership>().HasIndex(m => new { m.EventId, m.UserId }).IsUnique();

        mb.Entity<Invitation>(e =>
        {
            e.HasOne(i => i.Inviter).WithMany().HasForeignKey(i => i.InviterUserId);
            e.HasOne(i => i.Invitee).WithMany().HasForeignKey(i => i.InviteeUserId);
        });
    }
}

namespace FeteCam.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EventRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Date { get; set; }
    public string Location { get; set; } = "";
    public string Status { get; set; } = "En préparation";
    public int OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class GuestRecord
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Category { get; set; } = "Invité";
    public string Status { get; set; } = "En attente";
    public int PersonCount { get; set; } = 1;
    public int? ResponsibleUserId { get; set; }
}

public class TaskRecord
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "Logistique";
    public DateOnly? DueDate { get; set; }
    public string? DueLabel { get; set; }
    public string Status { get; set; } = "En préparation";
    public string AssigneeName { get; set; } = "";
    public string Details { get; set; } = "";
    public int? AssigneeUserId { get; set; }
}

public class Invitation
{
    public int Id { get; set; }
    public string Token { get; set; } = "";
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string Role { get; set; } = "Membre du comité";
    public string? TaskTitle { get; set; }
    public string? InviteeName { get; set; }
    public int InviterUserId { get; set; }
    public int? InviteeUserId { get; set; }
    public string Status { get; set; } = "En attente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public User? Inviter { get; set; }
    public User? Invitee { get; set; }
}

public class Membership
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "Membre du comité";
    public string Status { get; set; } = "Confirmé";
    public int? TaskId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

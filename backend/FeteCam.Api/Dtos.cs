namespace FeteCam.Api.Dtos;

public record RegisterRequest(string Email, string Password, string Name, string? Phone, string? City);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);
public record UserDto(int Id, string Email, string Name, string Phone, string City);

public record EventDto(int Id, string Name, DateOnly? Date, string Location, string Status, string Role);
public record CreateEventRequest(string Name, DateOnly? Date, string Location, string Status);
public record UpdateEventRequest(string Name, DateOnly? Date, string Location, string Status);

public record GuestDto(int Id, int EventId, string Name, string Phone, string Category, string Status, int PersonCount);
public record UpdateGuestRequest(string Name, string Phone, string Category, string Status, int PersonCount);

public record TaskDto(int Id, int EventId, string Title, string Category, DateOnly? DueDate, string? DueLabel,
    string Status, string? AssigneeName, string? Details);
public record UpdateTaskRequest(string Title, string Category, DateOnly? DueDate, string? DueLabel,
    string Status, string? AssigneeName, string? Details);

public record MembershipDto(int Id, int EventId, int UserId, string Name, string Phone, string Role, string Status, DateTime JoinedAt);

public record InvitationDto(
    int Id, string Token, string Link, string DeepLink, int EventId, string EventName,
    string Role, string? TaskTitle, string Status, string InviterName, string? InviteeName,
    DateTime CreatedAt, DateTime? RespondedAt);

public record CreateInvitationRequest(int EventId, string? Role, string? TaskTitle, int? TaskId, string? InviteeName);
public record InvitationDetailDto(
    string Token, string EventName, string Role, string? TaskTitle, string Status,
    string InviterName, bool CanRespond, bool AlreadyMember);

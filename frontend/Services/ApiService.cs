using System.Net.Http.Headers;
using System.Text.Json;

namespace FeteCam.Services;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
}

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Date { get; set; }
    public string Location { get; set; } = "";
    public string Status { get; set; } = "";
    public string Role { get; set; } = "";
}

public class InvitationDto
{
    public int Id { get; set; }
    public string Token { get; set; } = "";
    public string Link { get; set; } = "";
    public string DeepLink { get; set; } = "";
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? TaskTitle { get; set; }
    public string Status { get; set; } = "";
    public string InviterName { get; set; } = "";
    public string? InviteeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class InvitationDetailDto
{
    public string Token { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? TaskTitle { get; set; }
    public string Status { get; set; } = "";
    public string InviterName { get; set; } = "";
    public bool CanRespond { get; set; }
    public bool AlreadyMember { get; set; }
}

public class GuestDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public int PersonCount { get; set; } = 1;
}

public class TaskDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string? DueDate { get; set; }
    public string? DueLabel { get; set; }
    public string Status { get; set; } = "";
    public string? AssigneeName { get; set; }
    public string? Details { get; set; }
}

public class MembershipDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime JoinedAt { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public UserDto User { get; set; } = new();
}

public class ApiService
{
    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string BaseUrl => Preferences.Default.Get("api.baseUrl", "http://localhost:5210");
    public static string Token { get; private set; } = Preferences.Default.Get("api.token", "");
    public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public string? LastError { get; private set; }

    private ApiService() { }

    private HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
        if (!string.IsNullOrEmpty(Token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        return client;
    }

    private static string? ExtractError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var e))
                return e.GetString();
        }
        catch { }
        return null;
    }

    private async Task<T?> PostAsync<T>(string url, object payload) where T : class
    {
        LastError = null;
        try
        {
            using var client = CreateClient();
            using var content = new StringContent(JsonSerializer.Serialize(payload, Json), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                LastError = ExtractError(body) ?? $"Erreur serveur ({(int)resp.StatusCode}).";
                return null;
            }
            return JsonSerializer.Deserialize<T>(body, Json);
        }
        catch (HttpRequestException)
        {
            LastError = "Serveur injoignable. Vérifiez que le backend est démarré.";
            return null;
        }
        catch (TaskCanceledException)
        {
            LastError = "Délai d'attente dépassé.";
            return null;
        }
        catch (JsonException)
        {
            LastError = "Réponse du serveur invalide.";
            return null;
        }
    }

    private async Task<T?> PutAsync<T>(string url, object payload) where T : class
    {
        LastError = null;
        try
        {
            using var client = CreateClient();
            using var content = new StringContent(JsonSerializer.Serialize(payload, Json), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PutAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                LastError = ExtractError(body) ?? $"Erreur serveur ({(int)resp.StatusCode}).";
                return null;
            }
            return JsonSerializer.Deserialize<T>(body, Json);
        }
        catch (HttpRequestException)
        {
            LastError = "Serveur injoignable. Vérifiez que le backend est démarré.";
            return null;
        }
        catch (TaskCanceledException)
        {
            LastError = "Délai d'attente dépassé.";
            return null;
        }
        catch (JsonException)
        {
            LastError = "Réponse du serveur invalide.";
            return null;
        }
    }

    private async Task<T?> GetAsync<T>(string url) where T : class
    {
        LastError = null;
        try
        {
            using var client = CreateClient();
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                LastError = ExtractError(body) ?? $"Erreur serveur ({(int)resp.StatusCode}).";
                return null;
            }
            return JsonSerializer.Deserialize<T>(body, Json);
        }
        catch (HttpRequestException)
        {
            LastError = "Serveur injoignable. Vérifiez que le backend est démarré.";
            return null;
        }
        catch (TaskCanceledException)
        {
            LastError = "Délai d'attente dépassé.";
            return null;
        }
        catch (JsonException)
        {
            LastError = "Réponse du serveur invalide.";
            return null;
        }
    }

    private async Task<bool> DeleteAsync(string url)
    {
        LastError = null;
        try
        {
            using var client = CreateClient();
            var resp = await client.DeleteAsync(url);
            if (!resp.IsSuccessStatusCode)
                LastError = $"Erreur serveur ({(int)resp.StatusCode}).";
            return resp.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            LastError = "Serveur injoignable.";
            return false;
        }
        catch (TaskCanceledException)
        {
            LastError = "Délai d'attente dépassé.";
            return false;
        }
    }

    public async Task<bool> PingAsync()
    {
        var result = await GetAsync<HealthDto>("/api/health");
        return result is not null;
    }

    private class HealthDto { public string Status { get; set; } = ""; }

    public async Task<UserDto?> RegisterAsync(string email, string password, string name, string phone, string city)
    {
        var result = await PostAsync<AuthResponse>("/api/auth/register",
            new { email, password, name, phone, city });
        if (result is null) return null;
        StoreSession(result);
        return result.User;
    }

    public async Task<UserDto?> LoginAsync(string email, string password)
    {
        var result = await PostAsync<AuthResponse>("/api/auth/login", new { email, password });
        if (result is null) return null;
        StoreSession(result);
        return result.User;
    }

    private static void StoreSession(AuthResponse result)
    {
        Token = result.Token;
        Preferences.Default.Set("api.token", result.Token);
        var acc = AccountService.Instance;
        acc.Update(result.User.Name, result.User.Phone, result.User.City, result.User.Email);
    }

    public void Logout()
    {
        Token = "";
        Preferences.Default.Remove("api.token");
    }

    private readonly Dictionary<string, int> _eventIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _ensureLock = new(1, 1);

    public async Task<int?> EnsureEventIdAsync(string name, string location, string status)
    {
        if (_eventIds.TryGetValue(name, out var cached))
            return cached;

        await _ensureLock.WaitAsync();
        try
        {
            if (_eventIds.TryGetValue(name, out cached))
                return cached;

            var events = await GetAsync<List<EventDto>>("/api/events");
            if (events is null) return null;
            var existing = events.FirstOrDefault(e =>
                string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                _eventIds[name] = existing.Id;
                return existing.Id;
            }

            var created = await PostAsync<EventDto>("/api/events", new { name, location, status });
            if (created is null) return null;
            _eventIds[name] = created.Id;
            return created.Id;
        }
        finally
        {
            _ensureLock.Release();
        }
    }

    public Task<EventDto?> UpdateEventAsync(int eventId, string name, string? date, string location, string status)
        => PutAsync<EventDto>($"/api/events/{eventId}", new { name, date, location, status });

    public Task<InvitationDto?> CreateInvitationAsync(int eventId, string role, string? taskTitle = null, string? inviteeName = null)
        => PostAsync<InvitationDto>("/api/invitations", new { eventId, role, taskTitle, inviteeName });

    public Task<List<InvitationDto>?> GetSentInvitationsAsync()
        => GetAsync<List<InvitationDto>>("/api/invitations/sent");

    public Task<InvitationDetailDto?> GetInvitationAsync(string token)
        => GetAsync<InvitationDetailDto>($"/api/invitations/by-token/{Uri.EscapeDataString(token)}");

    public Task<InvitationDto?> AcceptInvitationAsync(string token)
        => PostAsync<InvitationDto>($"/api/invitations/by-token/{Uri.EscapeDataString(token)}/accept", new { });

    public Task<InvitationDto?> DeclineInvitationAsync(string token)
        => PostAsync<InvitationDto>($"/api/invitations/by-token/{Uri.EscapeDataString(token)}/decline", new { });

    public Task<bool> DeleteInvitationAsync(int id)
        => DeleteAsync($"/api/invitations/{id}");

    public Task<List<GuestDto>?> GetGuestsAsync(int eventId)
        => GetAsync<List<GuestDto>>($"/api/events/{eventId}/guests");

    public Task<GuestDto?> CreateGuestAsync(int eventId, string name, string phone, int personCount, string status)
        => PostAsync<GuestDto>($"/api/events/{eventId}/guests",
            new { name, phone, personCount, status, category = "Invité" });

    public Task<GuestDto?> UpdateGuestAsync(int eventId, int guestId, string name, string phone, int personCount, string status)
        => PutAsync<GuestDto>($"/api/events/{eventId}/guests/{guestId}",
            new { name, phone, personCount, status, category = "Invité" });

    public Task<bool> DeleteGuestAsync(int eventId, int guestId)
        => DeleteAsync($"/api/events/{eventId}/guests/{guestId}");

    public Task<List<TaskDto>?> GetTasksAsync(int eventId)
        => GetAsync<List<TaskDto>>($"/api/events/{eventId}/tasks");

    public Task<TaskDto?> CreateTaskAsync(int eventId, string title, string assigneeName, string dueLabel, string status, string details)
        => PostAsync<TaskDto>($"/api/events/{eventId}/tasks",
            new { title, assigneeName, dueLabel, status, details, category = "Logistique" });

    public Task<TaskDto?> UpdateTaskAsync(int eventId, int taskId, string title, string assigneeName, string dueLabel, string status, string details)
        => PutAsync<TaskDto>($"/api/events/{eventId}/tasks/{taskId}",
            new { title, assigneeName, dueLabel, status, details, category = "Logistique" });

    public Task<bool> DeleteTaskAsync(int eventId, int taskId)
        => DeleteAsync($"/api/events/{eventId}/tasks/{taskId}");

    public Task<List<MembershipDto>?> GetCommitteeAsync(int eventId)
        => GetAsync<List<MembershipDto>>($"/api/events/{eventId}/committee");

    public Task<bool> DeleteCommitteeMemberAsync(int eventId, int membershipId)
        => DeleteAsync($"/api/events/{eventId}/committee/{membershipId}");
}

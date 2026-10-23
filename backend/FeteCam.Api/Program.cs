using System.Security.Cryptography;
using System.Text;
using FeteCam.Api.Data;
using FeteCam.Api.Endpoints;
using FeteCam.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=fetecam.db"));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    jwtKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton(new TokenService(builder.Configuration, jwtKey));
builder.Services.AddSingleton<LinkBuilder>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    TryAddColumn(db, "Invitations", "InviteeName", "TEXT NULL");
    TryAddColumn(db, "Guests", "PersonCount", "INTEGER NOT NULL DEFAULT 1");
    TryAddColumn(db, "Tasks", "DueLabel", "TEXT NULL");
    TryAddColumn(db, "Tasks", "AssigneeName", "TEXT NOT NULL DEFAULT ''");
    TryAddColumn(db, "Tasks", "Details", "TEXT NOT NULL DEFAULT ''");
}

static void TryAddColumn(AppDbContext db, string table, string column, string type)
{
    try { db.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {type}"); }
    catch { }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuth();
app.MapInvitations();
app.MapCore();

app.MapGet("/i/{token}", async (string token, AppDbContext db) =>
{
    var inv = await db.Invitations.Include(i => i.Inviter)
        .FirstOrDefaultAsync(i => i.Token == token);
    if (inv is null) return Results.Text("Invitation introuvable.", statusCode: 404);

    Func<string, string> esc = s => System.Net.WebUtility.HtmlEncode(s);
    var html = $$"""
<!doctype html>
<html lang="fr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Invitation — FêteCam</title>
<style>
  body{font-family:system-ui,sans-serif;background:#F6F7F4;margin:0;display:flex;justify-content:center;padding:24px;color:#1D211F}
  .card{background:#fff;border:1px solid #E3E7E4;border-radius:16px;max-width:440px;width:100%;padding:28px}
  h1{font-size:22px;margin:0 0 4px} .sub{color:#5F6B66;font-size:14px;margin-bottom:20px}
  .row{display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #EEF1EF;font-size:15px}
  .k{color:#5F6B66} .v{font-weight:600}
  .badge{display:inline-block;background:#DDF0E8;color:#0B6B4F;border-radius:8px;padding:4px 10px;font-size:13px;font-weight:600}
  .btn{display:block;width:100%;text-align:center;border-radius:10px;padding:13px;font-size:16px;font-weight:600;border:none;cursor:pointer;margin-top:12px}
  .accept{background:#0B6B4F;color:#fff} .decline{background:#fff;color:#B3261E;border:1px solid #E5C6C3}
  .open{background:#1D211F;color:#fff;text-decoration:none;margin-top:16px}
  .msg{margin-top:14px;font-size:15px;font-weight:600;text-align:center;min-height:20px}
</style>
</head>
<body>
<div class="card">
  <h1>{{esc(inv.EventName)}}</h1>
  <div class="sub">Invitation de {{esc(inv.Inviter?.Name ?? "l'organisateur")}} à rejoindre le comité d'organisation</div>
  <div class="row"><span class="k">Rôle proposé</span><span class="v">{{esc(inv.Role)}}</span></div>
  {{(inv.InviteeName is null ? "" : $"<div class=\"row\"><span class=\"k\">Destinataire</span><span class=\"v\">{esc(inv.InviteeName)}</span></div>")}}
  {{(inv.TaskTitle is null ? "" : $"<div class=\"row\"><span class=\"k\">Responsabilité</span><span class=\"v\">{esc(inv.TaskTitle)}</span></div>")}}
  <div class="row"><span class="k">Statut</span><span class="badge">{{esc(inv.Status)}}</span></div>
  <div id="actions">
    <button class="btn accept" onclick="respond('accept')">Accepter l'invitation</button>
    <button class="btn decline" onclick="respond('decline')">Refuser l'invitation</button>
    <a class="btn open" href="fetecam://invite/{{inv.Token}}">Ouvrir dans l'application FêteCam</a>
  </div>
  <div class="msg" id="msg"></div>
</div>
<script>
async function respond(action){
  document.getElementById('actions').style.display='none';
  const msg=document.getElementById('msg');
  const token='{{inv.Token}}';
  try{
    const stored=localStorage.getItem('fetecam_token');
    if(!stored){ msg.textContent='Connectez-vous d\'abord dans l\'application FêteCam pour répondre.'; setTimeout(()=>location.reload(),50); return; }
    const r=await fetch('/api/invitations/by-token/'+token+'/'+action, {method:'POST', headers:{'Authorization':'Bearer '+stored} });
    const data=await r.json();
    if(!r.ok){ msg.textContent=data.error||'Erreur.'; setTimeout(()=>location.reload(),60); return; }
    msg.textContent=data.status==='Acceptée'?'Invitation acceptée. Bienvenue dans le comité !':'Invitation refusée.';
  }catch(e){ msg.textContent='Serveur injoignable. Ouvrez le lien dans l\'application.'; }
}
</script>
</body>
</html>
""";
    return Results.Text(html, "text/html");
});

app.Run();

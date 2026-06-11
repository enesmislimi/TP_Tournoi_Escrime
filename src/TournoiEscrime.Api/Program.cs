using Microsoft.EntityFrameworkCore;
using TournoiEscrime;
using TournoiEscrime.Data;
using TournoiEscrime.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Enregistrement de la base de données SQLite
// Le fichier tournoi.db sera créé automatiquement au premier lancement
builder.Services.AddDbContext<TournamentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=tournoi.db"));

// Injection de dépendances : quand on demande ITournamentRepository → on reçoit TournamentRepository
builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<IScoreCalculator, ScoreCalculator>();

// CORS : permet à l'app Blazor (autre port) d'appeler l'API
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Créer la base de données au démarrage si elle n'existe pas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TournamentDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

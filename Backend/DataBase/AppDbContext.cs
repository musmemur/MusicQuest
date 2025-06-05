using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.DataBase;

public class AppDbContext(IConfiguration configuration) : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<PlaylistTrack> PlaylistTracks => Set<PlaylistTrack>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация связи many-to-many
        modelBuilder.Entity<PlaylistTrack>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId }); // Составной ключ

        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Playlist)
            .WithMany(p => p.PlaylistTracks)
            .HasForeignKey(pt => pt.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);
    
        modelBuilder.Entity<PlaylistTrack>()
            .HasOne(pt => pt.Track)
            .WithMany(t => t.PlaylistTracks)
            .HasForeignKey(pt => pt.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь User-Playlist (one-to-many)
        modelBuilder.Entity<Playlist>()
            .HasOne<User>() // Указываем тип сущности User
            .WithMany(u => u.Playlists) // Указываем навигационное свойство
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // User-Room (один пользователь может создать много комнат)
        modelBuilder.Entity<Room>()
            .HasOne(r => r.HostUser)
            .WithMany() // У User нет обратной навигации на Room
            .HasForeignKey(r => r.HostUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room-Player (в комнате много игроков)
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Room)
            .WithMany(r => r.Players)
            .HasForeignKey(p => p.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Player-User (игрок — это пользователь)
        modelBuilder.Entity<Player>()
            .HasOne(p => p.User)
            .WithMany() // У User нет обратной навигации на Player
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Room-GameSession (у комнаты одна активная сессия)
        modelBuilder.Entity<GameSession>()
            .HasOne(gs => gs.Room)
            .WithMany() // У Room нет обратной навигации на GameSession
            .HasForeignKey(gs => gs.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // GameSession-QuizQuestion (в сессии много вопросов)
        modelBuilder.Entity<QuizQuestion>()
            .HasOne(q => q.GameSession)
            .WithMany(gs => gs.Questions)
            .HasForeignKey(q => q.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }}
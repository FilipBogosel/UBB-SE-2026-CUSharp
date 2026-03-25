#nullable enable

using Microsoft.EntityFrameworkCore;
using MovieApp.Core.Models;

namespace MovieApp.Core.Data;

/// <summary>
/// Entity Framework Core database context for MovieApp.
/// </summary>
public class MovieAppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovieAppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public MovieAppDbContext(DbContextOptions<MovieAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Battle> Battles => Set<Battle>();

    public DbSet<Bet> Bets => Set<Bet>();

    public DbSet<UserStats> UserStats => Set<UserStats>();

    public DbSet<Badge> Badges => Set<Badge>();

    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(movie => movie.MovieId);
            entity.Property(movie => movie.Title).HasMaxLength(200).IsRequired();
            entity.Property(movie => movie.PosterUrl).HasMaxLength(500).IsRequired();
            entity.Property(movie => movie.Genre).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.UserId);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(review => review.ReviewId);
            entity.Property(review => review.Content).HasMaxLength(5000);
            entity.Property(review => review.ExtraReviewContent).HasMaxLength(12000);
            entity.Property(review => review.CinematographyText).HasMaxLength(2000);
            entity.Property(review => review.ActingText).HasMaxLength(2000);
            entity.Property(review => review.CgiText).HasMaxLength(2000);
            entity.Property(review => review.PlotText).HasMaxLength(2000);
            entity.Property(review => review.SoundText).HasMaxLength(2000);
            entity.HasIndex(review => new { review.UserId, review.MovieId }).IsUnique();
            entity.HasOne(review => review.User)
                .WithMany(user => user.Reviews)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(review => review.Movie)
                .WithMany(movie => movie.Reviews)
                .HasForeignKey(review => review.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(comment => comment.MessageId);
            entity.Property(comment => comment.Content).HasMaxLength(10000).IsRequired();
            entity.HasOne(comment => comment.Author)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(comment => comment.Movie)
                .WithMany(movie => movie.Comments)
                .HasForeignKey(comment => comment.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(comment => comment.ParentComment)
                .WithMany(comment => comment.Replies)
                .HasForeignKey(comment => comment.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Battle>(entity =>
        {
            entity.HasKey(battle => battle.BattleId);
            entity.Property(battle => battle.Status).HasMaxLength(50).IsRequired();
            entity.HasOne(battle => battle.FirstMovie)
                .WithMany(movie => movie.FirstMovieBattles)
                .HasForeignKey(battle => battle.FirstMovieId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(battle => battle.SecondMovie)
                .WithMany(movie => movie.SecondMovieBattles)
                .HasForeignKey(battle => battle.SecondMovieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bet>(entity =>
        {
            entity.HasKey(bet => new { bet.UserId, bet.BattleId });
            entity.HasOne(bet => bet.User)
                .WithMany(user => user.Bets)
                .HasForeignKey(bet => bet.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bet => bet.Battle)
                .WithMany(battle => battle.Bets)
                .HasForeignKey(bet => bet.BattleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(bet => bet.Movie)
                .WithMany(movie => movie.Bets)
                .HasForeignKey(bet => bet.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserStats>(entity =>
        {
            entity.HasKey(stats => stats.StatsId);
            entity.HasIndex(stats => stats.UserId).IsUnique();
            entity.HasOne(stats => stats.User)
                .WithOne(user => user.UserStats)
                .HasForeignKey<UserStats>(stats => stats.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(badge => badge.BadgeId);
            entity.Property(badge => badge.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(userBadge => new { userBadge.UserId, userBadge.BadgeId });
            entity.HasOne(userBadge => userBadge.User)
                .WithMany(user => user.UserBadges)
                .HasForeignKey(userBadge => userBadge.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(userBadge => userBadge.Badge)
                .WithMany(badge => badge.UserBadges)
                .HasForeignKey(userBadge => userBadge.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

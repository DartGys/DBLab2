using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FootBallWebLaba1.Models;

public partial class FootBallBdContext : DbContext
{
    public FootBallBdContext()
    {
    }

    public FootBallBdContext(DbContextOptions<FootBallBdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Championship> Championships { get; set; }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<ScoredGoal> ScoredGoals { get; set; }

    public virtual DbSet<Stadium> Stadiums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-CFOLTDF\\SQLEXPRESS;Database=FootBallBD;Trusted_Connection=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Championship>(entity =>
        {
            entity.ToTable("Championship");

            entity.Property(e => e.ChampionshipCountry)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ChampionshipName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Club>(entity =>
        {
            entity.ToTable("Club");

            entity.Property(e => e.ClubCoachName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ClubEstablishmentDate).HasColumnType("date");
            entity.Property(e => e.ClubName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ClubOrigin)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("Match");

            entity.Property(e => e.MatchDate).HasColumnType("date");

            entity.HasOne(d => d.Championship).WithMany(p => p.Matches)
                .HasForeignKey(d => d.ChampionshipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_Championship");

            entity.HasOne(d => d.GuestClub).WithMany(p => p.MatchGuestClubs)
                .HasForeignKey(d => d.GuestClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_GuestClub");

            entity.HasOne(d => d.HostClub).WithMany(p => p.MatchHostClubs)
                .HasForeignKey(d => d.HostClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_HostClub");

            entity.HasOne(d => d.Stadium).WithMany(p => p.Matches)
                .HasForeignKey(d => d.StaidumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_Stadium");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");

            entity.Property(e => e.PlayerBirthDate).HasColumnType("date");
            entity.Property(e => e.PlayerName)
                .IsRequired()
                .HasMaxLength(40);
            entity.Property(e => e.PlayerSalary).HasColumnType("money");

            entity.HasOne(d => d.Club).WithMany(p => p.Players)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Player_Club");

            entity.HasOne(d => d.Position).WithMany(p => p.Players)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Player_Position");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Position");

            entity.Property(e => e.PositionName)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("Position");
        });

        modelBuilder.Entity<ScoredGoal>(entity =>
        {
            entity.ToTable("ScoredGoal");

            entity.HasOne(d => d.Match).WithMany(p => p.ScoredGoals)
                .HasForeignKey(d => d.MatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScoredGoal_Match");

            entity.HasOne(d => d.Player).WithMany(p => p.ScoredGoals)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ScoredGoal_Player");
        });

        modelBuilder.Entity<Stadium>(entity =>
        {
            entity.ToTable("Stadium");

            entity.Property(e => e.StadiumEstablismentDate).HasColumnType("date");
            entity.Property(e => e.StadiumLocation)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Club).WithMany(p => p.Stadiums)
                .HasForeignKey(d => d.ClubId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stadium_Club");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

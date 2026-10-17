
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(
            DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Organiser> Organisers => Set<Organiser>();
        public DbSet<Participant> Participants => Set<Participant>();

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<Sponsor> Sponsors => Set<Sponsor>();
        public DbSet<EventSponsor> EventSponsors => Set<EventSponsor>();
        public DbSet<WeatherLog> WeatherLogs => Set<WeatherLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Each email address must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Each user can have one organiser profile
            modelBuilder.Entity<Organiser>()
                .HasIndex(o => o.UserId)
                .IsUnique();

            // Each user can have one participant profile
            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Organiser-to-User relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organiser)
                .WithOne(o => o.User)
                .HasForeignKey<Organiser>(o => o.UserId);

            // Participant-to-User relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Participant)
                .WithOne(p => p.User)
                .HasForeignKey<Participant>(p => p.UserId);
            modelBuilder.Entity<Event>()

    .HasOne(e => e.Organiser)
    .WithMany()
    .HasForeignKey(e => e.OrganiserId)
    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Category>()
    .HasOne(c => c.Event)
    .WithMany()
    .HasForeignKey(c => c.EventId)
    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Enrolment>()
    .HasOne(e => e.Participant)
    .WithMany()
    .HasForeignKey(e => e.ParticipantId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => e.BibNumber)
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.CategoryId })
                .IsUnique();
            modelBuilder.Entity<Payment>()
    .HasOne(p => p.Enrolment)
    .WithOne()
    .HasForeignKey<Payment>(p => p.EnrolmentId)
    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Result>()
    .HasOne(r => r.Enrolment)
    .WithMany()
    .HasForeignKey(r => r.EnrolmentId)
    .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<EventSponsor>()
    .HasOne(es => es.Event)
    .WithMany()
    .HasForeignKey(es => es.EventId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EventSponsor>()
                .HasOne(es => es.Sponsor)
                .WithMany()
                .HasForeignKey(es => es.SponsorId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<WeatherLog>()
    .HasOne(w => w.Event)
    .WithMany()
    .HasForeignKey(w => w.EventId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EventSponsor>()
                .HasIndex(es => new { es.EventId, es.SponsorId })
                .IsUnique();

        }
    }
}

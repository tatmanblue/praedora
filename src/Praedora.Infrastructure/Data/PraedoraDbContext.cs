using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Praedora.Core.Entities;
using JobApplication = Praedora.Core.Entities.Application;

namespace Praedora.Infrastructure.Data;

public class PraedoraDbContext(DbContextOptions<PraedoraDbContext> options) : DbContext(options)
{
    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<StatusEvent> StatusEvents => Set<StatusEvent>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<CandidateEvent> CandidateEvents => Set<CandidateEvent>();
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(application => application.Id);
            entity.HasIndex(application => application.CompanyName);

            entity.Property(application => application.JdExtract)
                .HasConversion(
                    extract => extract == null ? null : JsonSerializer.Serialize(extract, JsonSerializerOptions.Web),
                    json => json == null ? null : JsonSerializer.Deserialize<JobDescriptionExtract>(json, JsonSerializerOptions.Web));

            entity.HasMany(application => application.StatusHistory)
                .WithOne()
                .HasForeignKey(statusEvent => statusEvent.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(application => application.Contacts)
                .WithOne()
                .HasForeignKey(contact => contact.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusEvent>().HasKey(statusEvent => statusEvent.Id);
        modelBuilder.Entity<Contact>().HasKey(contact => contact.Id);
        modelBuilder.Entity<CandidateEvent>().HasKey(candidateEvent => candidateEvent.Id);
        modelBuilder.Entity<LogEntry>().HasKey(logEntry => logEntry.Id);

        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasKey(email => email.Id);
            entity.HasIndex(email => email.ExternalMessageId).IsUnique();
        });

        modelBuilder.Entity<AppSetting>().HasKey(setting => setting.Key);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        FixUpNewStatusEvents();
        return base.SaveChangesAsync(cancellationToken);
    }

    // StatusEvent is append-only (design doc §6.2) — it is never legitimately updated. But since
    // its Id is a client-assigned Guid, EF Core's DetectChanges can't tell "newly appended to a
    // tracked Application.StatusHistory" apart from "an existing row whose values changed," and
    // defaults to Modified. Coerce it back to Added; an UPDATE against a StatusEvent that isn't
    // in the database yet would otherwise fail with a spurious DbUpdateConcurrencyException.
    private void FixUpNewStatusEvents()
    {
        foreach (EntityEntry<StatusEvent> entry in ChangeTracker.Entries<StatusEvent>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.State = EntityState.Added;
            }
        }
    }
}

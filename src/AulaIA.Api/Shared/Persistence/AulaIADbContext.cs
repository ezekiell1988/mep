using AulaIA.Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Shared.Persistence;

public sealed class AulaIADbContext(DbContextOptions<AulaIADbContext> options) : DbContext(options)
{
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<EvaluationActivity> EvaluationActivities => Set<EvaluationActivity>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AulaIADbContext).Assembly);
    }
}

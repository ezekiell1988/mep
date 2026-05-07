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
    public DbSet<CurriculumUnit> CurriculumUnits => Set<CurriculumUnit>();
    public DbSet<CurriculumExtraction> CurriculumExtractions => Set<CurriculumExtraction>();
    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Accommodation> Accommodations => Set<Accommodation>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentRequest> PaymentRequests => Set<PaymentRequest>();
    public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AulaIADbContext).Assembly);
    }
}

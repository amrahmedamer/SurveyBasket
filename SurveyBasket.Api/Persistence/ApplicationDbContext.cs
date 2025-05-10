

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Api.Extensions;
using System.Reflection;
using System.Security.Claims;

namespace SurveyBasket.Api.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Poll> Polls { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<VoteAnswer> VoteAnswers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        var cascadeFKs = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;


        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        foreach (var entityEntry in entries)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId!;
            }
            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

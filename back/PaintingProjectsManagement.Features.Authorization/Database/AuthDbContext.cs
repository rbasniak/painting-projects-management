using Microsoft.EntityFrameworkCore;
using rbkApiModules.Authentication;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Authorization;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RoleToClaim> RolesToClaims => Set<RoleToClaim>();
    public DbSet<UserToRole> UsersToRoles => Set<UserToRole>();
    public DbSet<UserToClaim> UsersToClaims => Set<UserToClaim>();

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<User>("auth", "Users");
        SchemaRegistry.AddRelationalMapping<Role>("auth", "Roles");
        SchemaRegistry.AddRelationalMapping<Claim>("auth", "Claims");
        SchemaRegistry.AddRelationalMapping<Tenant>("auth", "Tenants");
        SchemaRegistry.AddRelationalMapping<RoleToClaim>("auth", "RolesToClaims");
        SchemaRegistry.AddRelationalMapping<UserToRole>("auth", "UsersToRoles");
        SchemaRegistry.AddRelationalMapping<UserToClaim>("auth", "UsersToClaims");

        modelBuilder.Entity<User>().ToTable("Users", "auth");
        modelBuilder.Entity<Role>().ToTable("Roles", "auth");
        modelBuilder.Entity<Claim>().ToTable("Claims", "auth");
        modelBuilder.Entity<Tenant>().ToTable("Tenants", "auth");
        modelBuilder.Entity<RoleToClaim>().ToTable("RolesToClaims", "auth");
        modelBuilder.Entity<UserToRole>().ToTable("UsersToRoles", "auth");
        modelBuilder.Entity<UserToClaim>().ToTable("UsersToClaims", "auth");

        modelBuilder.Entity<OutboxDomainMessage>().ToTable("OutboxDomainMessages").ExcludeFromMigrations();
        modelBuilder.Entity<OutboxIntegrationEvent>().ToTable("OutboxIntegrationEvents").ExcludeFromMigrations();

        modelBuilder.AddJsonFields();
        modelBuilder.SetupTenants();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeWithoutKindConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeWithoutKindConverter>();
    }
}

using Core.Domain;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.EF;

public class DatabaseContext(IOptions<DatabaseOptions> databaseConfig) : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseConfig.Value.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(accountBuilder =>
        {
            accountBuilder.Property<Guid>("Id");
            accountBuilder.Property<bool>("activated").HasColumnName("Activated");

            accountBuilder
                .Property(u => u.Role)
                .HasConversion(role => role.Name, name => Role.ParseOrFail(name))
                .HasColumnType("varchar(50)");

            accountBuilder.OwnsMany<AuthSession>(
                "sessions",
                sessionBuilder =>
                {
                    sessionBuilder.Property<Guid>("Id");
                    sessionBuilder.Property<DateTime>("expiresAt").HasColumnName("ExpiresAt");
                    sessionBuilder.Property<Guid>("AccountId").HasColumnName("AccountId");
                    sessionBuilder.Property<Guid>("currentTokenId").HasColumnName("CurrentTokenId");
                    sessionBuilder.WithOwner().HasForeignKey("AccountId");
                }
            );
        });
    }
}

using Microsoft.EntityFrameworkCore;
using RapidPay.Auth;
using RapidPay.Cards;
using RapidPay.Payments;

namespace RapidPay;

public class RapidPayDbContext(DbContextOptions<RapidPayDbContext> options) : DbContext(options)
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ApiUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            modelBuilder.Entity<ApiUser>().HasData(
                new ApiUser
                {
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    PasswordHash = "AQAAAAIAAYagAAAAENsPH7x2YmVC9gRFvX17oCENPOnJyWBf4SRMRd/kdFOaNY/0gwuGDSbWATQtA8faeg==", // Adm1n@
                    SecurityStamp = "5PUYQLDNZJFPXNMU5HVM7XD6HMMZQ6KE",
                    ConcurrencyStamp = "c74dfea3-ed90-43d6-8855-74936d732669",
                    Salt = "s2wKlQasJQBFiVcSUDOnjjXZwo9Ot3uAwEgzhxYbnyo="
                });
        }
    }
}

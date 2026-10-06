using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(employee =>
        {
            employee.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            employee.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            employee.Property(e => e.PhoneNumber).HasMaxLength(14).IsRequired();

            // NOCASE collation makes the unique index treat "Jane@x.com" and "jane@x.com" as the same email.
            employee.Property(e => e.Email).HasMaxLength(254).IsRequired().UseCollation("NOCASE");
            employee.HasIndex(e => e.Email).IsUnique();

            employee.HasOne(e => e.Address)
                .WithOne(a => a.Employee)
                .HasForeignKey<Address>(a => a.EmployeeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(address =>
        {
            address.Property(a => a.Address1).HasMaxLength(200).IsRequired();
            address.Property(a => a.Address2).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(2).IsRequired();
            address.Property(a => a.Zip).HasMaxLength(5).IsRequired();
        });

        SeedData.Apply(modelBuilder);
    }
}
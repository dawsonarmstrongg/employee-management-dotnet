using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Server.Data;

// Seed rows are part of the model, so they are inserted by the initial migration.
// HasData requires explicit primary keys.
internal static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", PhoneNumber = "(555)-123-4567", Age = 34 },
            new Employee { Id = 2, FirstName = "John", LastName = "Smith", Email = "john.smith@example.com", PhoneNumber = "(555)-234-5678", Age = 45 },
            new Employee { Id = 3, FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@example.com", PhoneNumber = "(555)-345-6789", Age = 29 },
            new Employee { Id = 4, FirstName = "David", LastName = "Lee", Email = "david.lee@example.com", PhoneNumber = "(555)-456-7890", Age = 52 },
            new Employee { Id = 5, FirstName = "Aisha", LastName = "Khan", Email = "aisha.khan@example.com", PhoneNumber = "(555)-567-8901", Age = 38 });

        modelBuilder.Entity<Address>().HasData(
            new Address { Id = 1, EmployeeId = 1, Address1 = "123 Main St", Address2 = "Apt 4B", City = "Springfield", State = "IL", Zip = "62701" },
            new Address { Id = 2, EmployeeId = 2, Address1 = "456 Oak Ave", City = "Austin", State = "TX", Zip = "73301" },
            new Address { Id = 3, EmployeeId = 3, Address1 = "789 Pine Rd", Address2 = "Suite 200", City = "Denver", State = "CO", Zip = "80202" },
            new Address { Id = 4, EmployeeId = 4, Address1 = "321 Maple Dr", City = "Seattle", State = "WA", Zip = "98101" },
            new Address { Id = 5, EmployeeId = 5, Address1 = "654 Elm St", City = "Raleigh", State = "NC", Zip = "27601" });
    }
}
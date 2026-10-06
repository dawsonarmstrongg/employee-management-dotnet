namespace EmployeeManagement.Server.Data;

public class Address
{
    public int Id { get; set; }
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
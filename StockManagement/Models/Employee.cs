using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagement.Models;

public class Employee : BaseEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Position { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Operation> Operations { get; set; } = [];
}

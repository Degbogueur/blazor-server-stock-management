using System.ComponentModel.DataAnnotations;

namespace StockManagement.Models;

public class CustomSetting
{
    [Key]
    public required string Key { get; set; }
    public string? Value { get; set; }
    public required string Text { get; set; }
}

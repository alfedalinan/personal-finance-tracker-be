using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.Entities;

public class Transaction
{
    public int Id { get; set; }

    public DateTime DateOfTransaction { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BeforeBalance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AfterBalance { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.Entities;

public class Account
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] // also configured in DbContext Fluent API
    public decimal CurrentBalance { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

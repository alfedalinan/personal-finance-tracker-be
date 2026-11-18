using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Entities;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

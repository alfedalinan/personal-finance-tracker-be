using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Services.Implementations;

public class TenantContext : ITenantContext
{
    public string TenantId { get; set; } = string.Empty;
}

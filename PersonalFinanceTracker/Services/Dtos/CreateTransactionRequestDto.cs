using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Services.Dtos;

public record CreateTransactionRequestDto(
    int AccountId,
    int CategoryId,
    string Description,
    decimal Amount,
    [property: JsonPropertyName("date")]
    [property: JsonConverter(typeof(FlexibleNullableDateTimeConverter))]
    DateTime? DateOfTransaction = null
);

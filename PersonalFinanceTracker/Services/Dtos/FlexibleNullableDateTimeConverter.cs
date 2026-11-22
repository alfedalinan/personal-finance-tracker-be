using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Services.Dtos;

public class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
            {
                return dateTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime();
            }

            throw new JsonException($"Unable to parse \"{stringValue}\" as a valid DateTime.");
        }

        throw new JsonException($"Unexpected token parsing DateTime: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToUniversalTime().ToString("O"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

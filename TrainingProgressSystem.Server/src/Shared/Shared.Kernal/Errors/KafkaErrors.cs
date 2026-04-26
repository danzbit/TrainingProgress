namespace Shared.Kernal.Errors;

public static class KafkaErrors
{
    public static readonly Error DeserializationFailed = new(ErrorCode.DeserializationFailed, "Failed to deserialize message.");
}
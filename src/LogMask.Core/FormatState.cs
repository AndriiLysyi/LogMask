namespace LogMask.Core;

public readonly record struct FormatState(
    string Log,
    DateTimeOffset UtcTimestamp,
    CardMaskingOptions Options);
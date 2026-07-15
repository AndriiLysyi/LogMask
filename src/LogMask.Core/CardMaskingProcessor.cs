using System.Globalization;

namespace LogMask.Core;

public sealed class CardMaskingProcessor
{
    private static bool IsAllowedSeparator(char val) => val is ' ' or '-';
    private static bool IsAsciiDigit(char val) => val is >= '0' and <= '9';
    
    private const int TimeStampLength = 25;
    private const int PrefixLength = TimeStampLength + 3;

    private const int TimestampStart = 1;
    private const char OpenBracket = '[';
    private const char CloseBracket = ']';

    private static ReadOnlySpan<char> TimeStampFormat =>
        "yyyy-MM-ddTHH:mm:ss.ffffZ";
    
    private readonly CardMaskingOptions _options;
    private readonly TimeProvider _timeProvider;
    
    public CardMaskingProcessor(CardMaskingOptions? options = null, TimeProvider? timeProvider = null)
    {
        _options = options ?? new CardMaskingOptions();
        _timeProvider = timeProvider ?? TimeProvider.System;
        _options.Validate();
    }

    public string?[] Process(string?[] logs)
    {
        ArgumentNullException.ThrowIfNull(logs);
        
        var result = new string?[logs.Length];

        for (var index = 0; index < logs.Length; index++)
        {
            var log = logs[index];
            if (string.IsNullOrWhiteSpace(log))
            {
                continue;
            }

            result[index] = ProcessOne(log);
        }
        return result;
    }

    private string ProcessOne(string log)
    {
        var state = new FormatState(log, _timeProvider.GetUtcNow(), _options);

        return string.Create(
            PrefixLength + log.Length,
            state,
            static (destination, currentState) =>
            {
                WritePrefix(destination, currentState.UtcTimestamp);

                var source = currentState.Log.AsSpan();
                var logDestination = destination.Slice(PrefixLength);
                
                source.CopyTo(logDestination);
                MaskCardCandidates(source, logDestination, currentState.Options);
            });
    }
    
    private static void WritePrefix(Span<char> destination, DateTimeOffset utcTimestamp)
    {
        destination[0] = OpenBracket;

        var timestampDestination = destination.Slice(TimestampStart, TimeStampLength);
        var success = utcTimestamp.TryFormat(
            timestampDestination,
            out var charsWritten,
            TimeStampFormat,
            CultureInfo.InvariantCulture);

        if (!success || charsWritten != TimeStampLength)
        {
            throw new InvalidOperationException("UTC formatting failed.");
        }

        destination[TimestampStart + TimeStampLength] = CloseBracket;
        destination[TimestampStart + TimeStampLength + 1] = ' ';
    }
    
    private static void MaskCardCandidates(
        ReadOnlySpan<char> source,
        Span<char> destination,
        CardMaskingOptions options)
    {
        var index = 0;

        while (index < source.Length)
        {
            if (!IsAsciiDigit(source[index]))
            {
                index++;
                continue;
            }
            
            if (TryMatchGroup(source, index, out var groupEnd))
            {
                MaskDigits(source, destination, index, groupEnd, options.MaskingCharacter);
                
                index = groupEnd;
                continue;
            }

            var continuousEnd = index + 1;

            while (continuousEnd < source.Length && IsAsciiDigit(source[continuousEnd]))
            {
                continuousEnd++;
            }
            
            var digitCount = continuousEnd - index;

            if (digitCount >= options.MinContinuousDigits && digitCount <= options.MaxContinuousDigits)
            {
                MaskDigits(source, destination, index, continuousEnd, options.MaskingCharacter);
            }
            
            index = continuousEnd;
        }
    }

    private static bool TryMatchGroup(
        ReadOnlySpan<char> source,
        int start,
        out int end
    )
    {
        end = start;
        const int maxGroupLength = 19;
        if (source.Length - start < maxGroupLength)
        {
            return false;
        }

        if (start > 0 && IsAsciiDigit(source[start - 1]))
        {
            return false;
        }

        var index = start;
        var separator = '-';

        for (var group = 0; group < 4; group++)
        {
            for (var digit = 0; digit < 4; digit++)
            {
                if (index >= source.Length || !IsAsciiDigit(source[index]))
                {
                    return false;
                }

                index++;
            }

            if (group == 3)
            {
                break;
            }

            if (index >= source.Length || !IsAllowedSeparator(source[index]))
            {
                return false;
            }

            if (group == 0)
            {
                separator = source[index];
            }
            else if (source[index] != separator)
            {
                return false;
            }

            index++;
        }

        if (index < source.Length && IsAsciiDigit(source[index]))
        {
            return false;
        }

        end = index;
        return true;
    }

    private static void MaskDigits(
        ReadOnlySpan<char> source,
        Span<char> destination,
        int start,
        int end,
        char maskCharacter)
    {
        for (var index = start; index < end; index++)
        {
            if (IsAsciiDigit(source[index]))
            {
                destination[index] = maskCharacter;
            }
        }
    }
}
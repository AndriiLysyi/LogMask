namespace LogMask.Core;

public sealed record CardMaskingOptions
{
    public int MinContinuousDigits { get; init; } = 13;
    public int MaxContinuousDigits { get; init; } = 19;
    public char MaskingCharacter { get; init; } = '*';

    internal void Validate()
    {
        if (MinContinuousDigits < 1 || MaxContinuousDigits <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinContinuousDigits));
        }
        if (MinContinuousDigits > MaxContinuousDigits)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxContinuousDigits));
        }
    }
    
}
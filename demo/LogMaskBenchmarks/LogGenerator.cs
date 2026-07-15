namespace LogMaskBenchmarks;

public static class LogGenerator
{
    private static readonly string[] Templates =
    {
        "INFO  Payment authorized for card {card} amount {amt} USD",
        "WARN  Retry 2/3 for txn {guid}, card {card} declined",
        "ERROR Gateway timeout — no card data in this line, request {guid}",
        "DEBUG Cache hit for user {guid}, no PII present",
        "INFO  Batch settle: {card}, {card}, {card} total {amt}",
        "TRACE Raw payload: {{\"pan\":\"{card}\",\"cvv\":\"***\",\"exp\":\"12/28\"}}",
    };

    public static string?[] Generate(int count, int seed = 42)
    {
        var rnd = new Random(seed);   // seeded → repeatable benchmarks
        var result = new string?[count];

        for (int i = 0; i < count; i++)
        {
            // ~2% nulls — Process() accepts string?[], so exercise that
            if (rnd.NextDouble() < 0.02) { result[i] = null; continue; }

            var template = Templates[rnd.Next(Templates.Length)];
            var sb = new System.Text.StringBuilder(template);
            sb.Replace("{amt}", (rnd.NextDouble() * 5000).ToString("F2"));
            sb.Replace("{guid}", Guid.NewGuid().ToString());

            // replace {card} occurrences one at a time so they differ
            while (true)
            {
                var s = sb.ToString();
                int idx = s.IndexOf("{card}", StringComparison.Ordinal);
                if (idx < 0) break;
                sb.Remove(idx, 6).Insert(idx, RandomCard(rnd));
            }

            result[i] = sb.ToString();
        }
        return result;
    }

    private static string RandomCard(Random rnd)
    {
        // vary issuer prefix, length, and separator style
        var (prefix, length) = rnd.Next(4) switch
        {
            0 => ("4",    16),  // Visa
            1 => ("51",   16),  // Mastercard
            2 => ("34",   18),  // Amex
            _ => ("6011", 16),  // Discover
        };

        var digits = new char[length];
        for (int i = 0; i < length; i++)
            digits[i] = i < prefix.Length ? prefix[i] : (char)('0' + rnd.Next(10));
        

        var raw = new string(digits);
        return rnd.Next(3) switch
        {
            0 => raw,                                     // 4111111111111111
            1 => Chunk(raw, ' '),                         // 4111 1111 1111 1111
            _ => Chunk(raw, '-'),                         // 4111-1111-1111-1111
        };
    }

    private static string Chunk(string s, char sep)
    {
        if (s.Length != 16) return s;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(sep);
            sb.Append(s[i]);
        }
        return sb.ToString();
    }
}
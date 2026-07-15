using Microsoft.Extensions.Time.Testing;

namespace LogMask.Core.Tests;

public class CardMaskingProcessorTests
{
    private static CardMaskingProcessor CreateProcessor()
    {
        var clock = new FakeTimeProvider(
            new DateTimeOffset(2026, 1, 10,
                2, 1, 30, TimeSpan.Zero));

        return new CardMaskingProcessor(
            new CardMaskingOptions(), clock);
    }
    
    [Fact]
    public void Process_ContinuousCardNumber_ShouldMask()
    {
        var processor = CreateProcessor();
        var logs = new[] {"payment card =4444111133335555 transferred" };
        var result = processor.Process(logs);
        
        Assert.Equal(
            "[2026-01-10T02:01:30.0000Z] " +
            "payment card =**************** transferred"
            , result[0]);
    }
    
    [Fact]
    public void Process_ContinuousCardNumberAtEnd_ShouldMask()
    {
        var processor = CreateProcessor();
        var logs = new[] {"It is a long established fact that a reader will be distracted payment card =4444111133335555" };
        var result = processor.Process(logs);
        
        Assert.Equal(
            "[2026-01-10T02:01:30.0000Z] " +
            "It is a long established fact that a reader will be distracted payment card =****************"
            , result[0]);
    }
    
    [Fact]
    public void Process_GroupedCardNumber_ShouldMask()
    {
        var processor = CreateProcessor();
        var logs = new[] {"payment card =4444-1111-3333-5555 transferred" };
        var result = processor.Process(logs);
        
        Assert.Equal(
            "[2026-01-10T02:01:30.0000Z] " +
            "payment card =****-****-****-**** transferred"
            , result[0]);
    }
}
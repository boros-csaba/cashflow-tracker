namespace CashflowTracker;

public class RollingAverageDto
{
    public DateOnly Day { get; set; }
    public required string Category { get; set; }
    public decimal Amount { get; set; }
}

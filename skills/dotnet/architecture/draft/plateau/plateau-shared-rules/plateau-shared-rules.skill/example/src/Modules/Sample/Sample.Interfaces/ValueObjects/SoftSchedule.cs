namespace Sample.Interfaces.ValueObjects;

public record SoftSchedule(DateTimeOffset? StartDateTime, DateTimeOffset? DueDateTime);

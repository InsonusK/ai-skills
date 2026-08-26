namespace Sample.Interfaces.DTOs;

public record TaskScheduleDto(DateTimeOffset? StartDateTime, DateTimeOffset? DueDateTime);

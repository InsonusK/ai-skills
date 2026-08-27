using Sample.Interfaces.ValueObjects;

namespace Sample.Interfaces.DTOs;

public record TaskContactDto(
    SoftTitle Title,
    SoftEmail AssigneeEmail,
    DateTimeOffset? StartDateTime,
    DateTimeOffset? DueDateTime);

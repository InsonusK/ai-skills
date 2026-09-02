---
description: Add the Kafka client registration and options to App.Infrastructure
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/messaging-infrastructure
  - element/app-infrastructure-csproj
---

> Draft — shape only; the `KafkaRegistration` body and serializer are finalized with the first real consumer/producer.

# Goals
- One place for the Kafka client, its options, and its health check.

# Structure

## Project Structure
```
/App.Infrastructure
  /Messaging
    KafkaRegistration.cs
    KafkaOptions.cs
```

# Implementation changes (sketch)

```csharp
// App.Infrastructure/Messaging/KafkaOptions.cs
public sealed class KafkaOptions
{
    public string BootstrapServers { get; init; } = "";
    public string ConsumerGroupId  { get; init; } = "";
}

// App.Infrastructure/Messaging/KafkaRegistration.cs
public static class KafkaRegistration
{
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
        // register client factory + serializer + health check here
        return services;
    }
}
```

`Program.cs`: `builder.Services.AddKafkaMessaging(builder.Configuration);`

# Allowed Dependencies
- `Shared`
- the Kafka client NuGet package (final choice pending)

# Rules

## MUST
- Bind broker settings from `configuration.GetSection("Kafka")`, never hard-code.
  - Risk: environment values in code cannot vary per environment.
  - Fix: `Configure<KafkaOptions>`.

# Check list
- [ ] `Messaging/KafkaRegistration.cs` + `KafkaOptions.cs` exist.
- [ ] `AddKafkaMessaging` binds options from configuration.

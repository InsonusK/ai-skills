---
description: Ensure App.Host calls AddRepositories during composition
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Wire repository registrations into the application composition root

# Core Principles
- App.Host is the single composition root — all DI registration happens here or via extension methods called from here

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs
  Program.cs
```

# Implementation changes

Add the following call in `Program.cs`:

```csharp
builder.Services.AddRepositories();
```

# Rules

MUST:
- `AddRepositories()` called in `Program.cs`

# Check list
- [ ] `builder.Services.AddRepositories()` present in `Program.cs`

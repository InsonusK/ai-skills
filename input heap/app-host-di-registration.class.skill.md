```csharp
// App.Host/Program.cs
builder.Services
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration);
```
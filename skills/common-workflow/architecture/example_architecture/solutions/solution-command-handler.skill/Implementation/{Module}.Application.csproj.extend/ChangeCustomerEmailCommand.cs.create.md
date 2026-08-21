---
description: Immutable write-intent record for changing a customer's email
project_name: "{Module}.Application"
name: ChangeCustomerEmailCommand
element_kind: class
change_kind: create
tags:
  - solution/command-handler
  - element/changecustomeremailcommand-cs
---

# Implementation changes
```csharp
// {Module}.Application/Commands/ChangeCustomerEmailCommand.cs
public sealed record ChangeCustomerEmailCommand(int CustomerId, string NewEmail);
```

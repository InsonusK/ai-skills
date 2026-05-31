---
name: secure-coding
description: Defines secure coding practices.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - secure-coding
---
## Rules

Never trust external input.

Always validate:

* commands
* query parameters
* DTOs

## Secrets

Never hardcode secrets.

Use configuration providers.

## Logging

Do not log:

* passwords
* tokens
* personal data

## Database

Always use parameterized queries.

Avoid raw SQL unless necessary.

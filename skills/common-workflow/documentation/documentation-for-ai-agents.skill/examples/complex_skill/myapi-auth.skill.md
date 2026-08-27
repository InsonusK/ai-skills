---
name: myapi-auth
description: How to authenticate against myapi and manage session tokens
whenToUse: when an agent needs to log in to myapi, refresh a token, or log out
tags:
  - skill/documentation/for-ai
  - skill/example
  - stack
  - concern/documentation

---

# Goal
Authenticate an agent against myapi and manage the resulting session token.

# Prerequisites
Install and configure myapi first — see [myapi.skill.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/complex_skill/myapi.skill/myapi.skill.md) and its attached [installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/complex_skill/myapi.skill/installation.md).

# Methods

## `login`

```python
client.auth.login(api_key: str) -> dict
```

### Parameters

| Parameter | Type  | Required | Default | Description                |
| --------- | ----- | -------- | ------- | -------------------------- |
| `api_key` | `str` | yes      | —       | API key from the dashboard. |

### Returns

A dictionary with `token` and `expires_at`.

### Errors

- `AuthenticationError` — the API key is invalid or revoked.

### Example

```python
import os
from myapi import Client

client = Client()
session = client.auth.login(api_key=os.environ["MYAPI_KEY"])
print(session["token"])
```

## `refresh`

```python
client.auth.refresh(token: str) -> dict
```

### Parameters

| Parameter | Type  | Required | Default | Description              |
| --------- | ----- | -------- | ------- | ------------------------ |
| `token`   | `str` | yes      | —       | Current valid token.     |

### Returns

A new dictionary with `token` and `expires_at`.

### Example

```python
new_session = client.auth.refresh(token=session["token"])
```

# Rule

## MUST
- Refresh the token before `expires_at`.
- Reuse the same token across calls in one session.
- Never document installation or base URL here — link to the root skill instead.

# Auth

This page covers authentication: logging in, refreshing a token, and logging out.

## Setup

Install and configure myapi first — see the [README](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/README.md) and [installation guide](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/installation.md).

## `login(api_key)`

Use this method to exchange an API key for a session token.

### Parameters

- `api_key` (`str`, required): API key from the myapi dashboard.

### Returns

A dictionary with `token` and `expires_at`.

### Example

```python
import os
from myapi import Client

client = Client()
session = client.auth.login(api_key=os.environ["MYAPI_KEY"])
print(session["token"])
```

### Common errors

- `AuthenticationError`: the API key is invalid or revoked.

## `refresh(token)`

Use this method to obtain a new session token before the current one expires.

### Parameters

- `token` (`str`, required): current valid token.

### Returns

A new dictionary with `token` and `expires_at`.

### Example

```python
new_session = client.auth.refresh(token=session["token"])
```

## `logout()`

Use this method to invalidate the current session token.

### Example

```python
client.auth.logout()
```

## See also

- [Billing](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/api/billing.md) — create charges and list invoices after authenticating.

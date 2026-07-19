# myapi

A REST API for authentication and billing operations.

## Why

Use myapi when you need to manage user sessions and create or query billing records through a single HTTP API.

## Installation

Install the official client and set your API key.

```bash
pip install myapi-client==2.0.0
export MYAPI_KEY="sk-..."
```

See [docs/installation.md](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/installation.md) for detailed setup.

## Quick start

```python
import os
from myapi import Client

client = Client(api_key=os.environ["MYAPI_KEY"])
session = client.auth.login()
charge = client.billing.create_charge(
    amount=1000,
    currency="USD",
    customer_id="cust_123",
)
print(charge["id"])
```

## Documentation

| Topic   | Docs                          | Covers                         |
| ------- | ----------------------------- | ------------------------------ |
| Auth    | [docs/api/auth.md](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/api/auth.md)     | Login, token refresh, logout.  |
| Billing | [docs/api/billing.md](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/api/billing.md) | Charges, refunds, invoices.    |

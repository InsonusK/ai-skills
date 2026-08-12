---
name: myapi-billing
description: How to call myapi's billing endpoints — charges, refunds, and invoices
whenToUse: when an agent needs to create a charge, issue a refund, or list invoices in myapi
tags:
  - skill/documentation/for-ai
  - skill/example
  - stack
  - concern/documentation

---

# Goal
Create and manage charges, refunds, and invoices through myapi's billing endpoints.

# Prerequisites
Install and authenticate against myapi first — see [myapi.skill.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/complex_skill/myapi.skill/myapi.skill.md) and its attached [installation.md](skills/common-workflow/documentation/documentation-for-ai-agents.skill/examples/complex_skill/myapi.skill/installation.md).

# Methods

## `create_charge`

```python
client.billing.create_charge(amount: int, currency: str, customer_id: str) -> dict
```

### Parameters

| Parameter    | Type  | Required | Default | Description                        |
| ------------ | ----- | -------- | ------- | ---------------------------------- |
| `amount`     | `int` | yes      | —       | Amount in the smallest currency unit (cents). |
| `currency`   | `str` | yes      | —       | Three-letter currency code, e.g. `USD`. |
| `customer_id`| `str` | yes      | —       | Existing customer identifier.      |

### Returns

A dictionary with `id`, `status`, and `amount`.

### Errors

- `ValueError` — `currency` is not a supported code.
- `NotFoundError` — `customer_id` does not exist.

### Example

```python
from myapi import Client
import os

client = Client(api_key=os.environ["MYAPI_KEY"])
charge = client.billing.create_charge(
    amount=1000,
    currency="USD",
    customer_id="cust_123",
)
print(charge["id"])
```

## `list_invoices`

```python
client.billing.list_invoices(customer_id: str, status: str | None = None) -> list[dict]
```

### Parameters

| Parameter     | Type          | Required | Default | Description                                |
| ------------- | ------------- | -------- | ------- | ------------------------------------------ |
| `customer_id` | `str`         | yes      | —       | Customer whose invoices to list.           |
| `status`      | `str \| None` | no       | `None`  | Filter by `open`, `paid`, or `void`.       |

### Returns

A list of invoice dictionaries.

### Example

```python
invoices = client.billing.list_invoices(
    customer_id="cust_123",
    status="open",
)
```

# Rule

## MUST
- Ensure the customer exists before creating a charge.
- Use the currency unit consistent with myapi's convention (cents for USD).

## MUST NOT
- Document installation or authentication here — link to the root skill instead.

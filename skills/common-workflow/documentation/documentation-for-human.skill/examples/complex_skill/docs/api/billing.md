# Billing

This page covers billing operations: creating charges and listing invoices.

## Setup

Install, configure, and authenticate against myapi first — see the [README](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/README.md), [installation guide](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/installation.md), and [auth reference](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/api/auth.md).

## `create_charge(amount, currency, customer_id)`

Use this method to charge a customer.

### Parameters

- `amount` (`int`, required): amount in the smallest currency unit (for example, cents for `USD`).
- `currency` (`str`, required): three-letter currency code, for example `USD`.
- `customer_id` (`str`, required): existing customer identifier.

### Returns

A dictionary with `id`, `status`, and `amount`.

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

### Common errors

- `ValueError`: `currency` is not supported.
- `NotFoundError`: `customer_id` does not exist.

## `list_invoices(customer_id, status=None)`

Use this method to list a customer's invoices.

### Parameters

- `customer_id` (`str`, required): customer whose invoices to list.
- `status` (`str | None`, optional): filter by `open`, `paid`, or `void`.

### Returns

A list of invoice dictionaries.

### Example

```python
invoices = client.billing.list_invoices(
    customer_id="cust_123",
    status="open",
)
```

## See also

- [Auth](skills/common-workflow/documentation/documentation-for-human.skill/examples/complex_skill/docs/api/auth.md) — obtain a token before calling billing endpoints.

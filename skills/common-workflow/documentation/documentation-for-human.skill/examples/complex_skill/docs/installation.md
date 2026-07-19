# Installation

## Requirements

- Python 3.10 or later.
- A myapi account and API key.

## Install the client

```bash
pip install myapi-client==2.0.0
```

## Configure credentials

Set the API key as an environment variable:

```bash
export MYAPI_KEY="sk-..."
```

On Windows PowerShell:

```powershell
$env:MYAPI_KEY="sk-..."
```

## Verify

```python
from myapi import Client
client = Client(api_key=os.environ["MYAPI_KEY"])
print(client.auth.whoami())
```

## Troubleshooting

- `AuthenticationError`: the API key is missing or invalid. Check `MYAPI_KEY`.
- `ConnectionError`: verify that `https://api.myapi.example.com` is reachable.

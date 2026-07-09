# Installation and access

## MUST
- Base URL: `https://api.myapi.example.com/v1`.
- Obtain API key from the myapi dashboard.
- Set the key as an environment variable.
  ```bash
  export MYAPI_KEY="sk-..."
  ```
- Install the official client.
  ```bash
  pip install myapi-client==2.0.0
  ```
- Import the client.
  ```python
  from myapi import Client
  client = Client(api_key=os.environ["MYAPI_KEY"])
  ```

## SHOULD
- Verify connectivity by calling `client.auth.whoami()`.

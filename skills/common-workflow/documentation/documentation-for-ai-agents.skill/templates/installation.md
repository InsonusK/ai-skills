# Installation and access

Document how an AI agent obtains and accesses the library, CLI, or API.

## MUST
- Provide the exact installation command for the target environment.
  - Package manager and package name (for example, `pip install mylib`, `npm install mylib`, `dotnet add package MyLib`).
  - Required version or version range if the latest version is not safe.
- Provide the exact import or access statement.
  - Python: `from mylib.core import process_data`.
  - JavaScript/TypeScript: `import { processData } from 'mylib/core'`.
  - .NET: `using MyLib.Core;`.
  - CLI: full command name and how to confirm it is installed (for example, `mycli --version`).
- List environment prerequisites that can block execution:
  - runtime version,
  - required credentials or tokens,
  - network access,
  - required environment variables.
- Never assume the agent already knows how to install or import the tool.
- Never leave installation steps as links to external pages without copying the essential commands.

## SHOULD
- Include a minimal verification snippet that confirms the installation works.
- Mention optional dependencies and when they are needed.

## Example

```bash
pip install mylib==1.2.3
```

```python
from mylib.core import process_data, fetch_records

# Verify the import
print(process_data.__doc__)
```

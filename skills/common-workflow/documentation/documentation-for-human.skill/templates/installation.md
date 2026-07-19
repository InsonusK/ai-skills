# Installation and setup

Write installation and setup instructions that a human reader can follow without guessing.

## Location
- Keep quick installation steps in `README.md`.
- Move platform-specific, detailed, or troubleshooting instructions to `docs/installation.md` and link from `README.md`.

## MUST
- State supported platforms and runtimes.
- Provide the exact command for each supported package manager or install method.
  - Python: `pip install mylib`.
  - Node.js: `npm install mylib`.
  - .NET: `dotnet add package MyLib`.
  - CLI: download link, unzip command, and how to add to `PATH`.
- List required environment variables, credentials, or configuration files.
- Provide a verification step so the reader can confirm the installation succeeded.

## SHOULD
- Include troubleshooting for the most common install failures.
- Mention optional dependencies and what they enable.

## MUST NOT
- Assume the reader knows the project's ecosystem.
- Provide only a link to an external install guide without a summary.

## Example

### README.md

```markdown
## Installation

Requires Python 3.10 or later.

```bash
pip install mylib==1.2.3
```

Verify the installation:

```bash
python -c "import mylib; print(mylib.__version__)"
```

For platform-specific notes, see `docs/installation.md`.
```

### docs/installation.md

```markdown
# Installation

## Windows
...

## macOS / Linux
...

## Troubleshooting
...
```

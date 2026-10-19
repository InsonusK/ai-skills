# todo

The base plateau in Go with the CLI API variation point attached: `todo {resource} {action}
[flags]` runs one command, prints its result on stdout - one JSON line, or text with
`--output text` - writes errors on stderr, and exits 0, 1 (failure; a retry may help), or 2
(rejected, or a usage error).

```bash
todo items plan --title "buy milk" --title "walk the dog"      # {"items":[...]}
todo items plan --title "buy milk" --output text               # [ ] buy milk
todo --help                                                    # resources, actions, flags
LOG_LEVEL=error todo items plan --title a --title a; echo $?   # Error: invalid: ... / 2
```

```bash
make init    # once: module dependencies
make run     # build and run `todo items plan` with two titles
make test    # every scenario
```

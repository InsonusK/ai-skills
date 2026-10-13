# Ansible `combine` filter

The **`combine`** filter is a built-in Ansible/Jinja2 filter that merges two or more dictionaries (mappings) into one new dictionary, with later arguments overriding earlier ones on matching keys.

## Why it exists

Ansible resolves variables from many sources (role defaults, `group_vars`, `host_vars`, extra-vars, ...) using a fixed precedence order, but that precedence applies per variable *name*, not per *key inside a dictionary*. If a role wants "start from a full set of safe defaults, then let a consumer override just one or two keys," a plain variable reference is not enough — overriding `my_role` in `host_vars` replaces the *entire* dictionary, wiping out every default key the consumer did not explicitly restate. `combine` solves this by merging dictionaries key-by-key instead of replacing one with the other.

## How it works

`a | combine(b)` returns a new dictionary: every key from `a` is kept, then every key from `b` is applied on top, overwriting `a`'s value for any key that exists in both. Chaining `combine` calls applies each argument in the order it is written:

```yaml
result: "{{ tier1 | combine(tier2 | default({})) | combine(tier3 | default({})) }}"
```

`tier3` wins over `tier2`, which wins over `tier1`, for any key present in more than one tier.

Two behaviors matter for this solution:
- **`| default({})`** — without it, referencing an undefined variable (e.g. no `group_vars` entry for a group) raises an "undefined variable" error instead of simply contributing nothing to the merge.
- **`recursive=True`** — by default, `combine` is *shallow*: if a key's value is itself a nested dictionary, the later tier's nested dictionary *replaces* the earlier one wholesale, dropping any sibling keys the later tier didn't restate. `combine(other, recursive=True)` instead merges nested dictionaries key-by-key at every depth, so overriding one nested key leaves its siblings from the earlier tier intact.

## How it is structured

- Arguments are evaluated left to right; each one is combined onto the accumulated result so far.
- `recursive` (bool, default `False`) — merge nested dictionaries instead of replacing them.
- `list_merge` (str, default `replace`) — controls how list-valued keys are combined (`replace`, `keep`, `append`, `prepend`, `append_rp`, `prepend_rp`); relevant only when a tier's value is a list rather than a scalar or nested mapping.

## Example

From this solution's merge pattern in `vars/main.yml`:

```yaml
_{{ role_name }}_config: >-
  {{
    _{{ role_name }}_defaults
    | combine({{ role_name }}_group | default({}), recursive=True)
    | combine({{ role_name }} | default({}), recursive=True)
  }}
```

If `_{{ role_name }}_defaults` is `{a: 1, nested: {x: 1, y: 1}}` and `{{ role_name }}_group` sets `{nested: {y: 2}}`, the result is `{a: 1, nested: {x: 1, y: 2}}` — because `recursive=True` merged `nested` key-by-key instead of replacing it with `{y: 2}` alone.

## Related concepts

- [[../solution-ansible-role-var.skill.md|solution-ansible-role-var]] — the solution that standardizes this filter's use across role variable tiers.

## Sources

- Ansible documentation: [Combining dictionaries/hashes](https://docs.ansible.com/ansible/latest/playbook_guide/playbook_filters.html#combining-hashes-dictionaries)

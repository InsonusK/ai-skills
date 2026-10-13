# SOPS-encrypted inventory secrets

**SOPS** (Secrets OPerationS, by Mozilla) is a tool that encrypts individual values inside a structured file (YAML/JSON/ENV/INI) while leaving the file's keys and structure readable, so an encrypted file still diffs and reviews like the plaintext it protects. In this solution, SOPS is the mechanism used to store the `{{ role_name }}_group_secret` / `{{ role_name }}_secret` variable tiers safely in inventory.

## Why it exists

Ansible has no first-class notion of "this inventory value is a secret" — `group_vars`/`host_vars` are just YAML files, so committing a real credential in one means committing it in plaintext to version control. Ansible's own answer, `ansible-vault`, encrypts a whole file as one opaque blob: readable structure is lost, and mixing one secret key with ten non-secret keys forces encrypting all eleven. SOPS instead encrypts each leaf value independently while keeping keys and structure in the clear, so a secrets file can still be reviewed/diffed for *which keys changed*, and integrates with age, PGP, or a cloud KMS for key management instead of a single shared vault password.

## How it works

1. An author writes (or edits) a plaintext YAML file, e.g. `group_vars/webservers/secrets.sops.yaml`, with ordinary variables.
2. Running `sops -e -i secrets.sops.yaml` encrypts every leaf value in place (each becomes an `ENC[...]` blob) and appends a `sops:` metadata block recording which recipients (age/PGP/KMS keys) can decrypt it. The file, now ciphertext, is what gets committed.
3. At Ansible run time, the `community.sops.sops` **vars plugin** (from the `community.sops` collection) scans `group_vars`/`host_vars` directories for files matching a configured suffix (by default `.sops.yaml`, `.sops.yml`, `.sops.json`), decrypts them on the fly using whatever key backend is available in the environment (e.g. `SOPS_AGE_KEY_FILE`), and merges the resulting variables into the normal Ansible variable namespace — exactly as if the file had been plaintext all along.
4. This vars plugin must be explicitly enabled in `ansible.cfg` alongside the plugin that loads ordinary vars files — see [[../Implementation/ansible.cfg.extend.md|ansible.cfg]].

## How it is structured

- **Author-facing view**: plaintext YAML, edited with `sops secrets.sops.yaml` (which transparently decrypts, opens an editor, and re-encrypts on save).
- **At-rest view**: the same key structure, but every leaf scalar replaced by an `ENC[...]` blob, plus a trailing `sops:` metadata block (recipients, MAC, last-modified, version).
- **Runtime view**: fully decrypted, merged into normal Ansible variables — a role's tasks never see ciphertext or call `sops` themselves; they just read `_{{ role_name }}_secret_config` like any other variable, per [[../Implementation/vars/main.yml.create.md|vars/main.yml]].

## Example

See the worked example in [[../Implementation/inventory/group_vars/{group}/secrets.sops.yaml.create.md|group_vars/{{ group }}/secrets.sops.yaml]], which shows both the decrypted logical content and an abbreviated at-rest (encrypted) view of the same file.

## Related concepts

- [[ansible-combine-filter.md|Ansible combine filter]] — how the decrypted `{{ role_name }}_group_secret` / `{{ role_name }}_secret` values are merged into `_{{ role_name }}_secret_config`.

## Sources

- SOPS project: https://github.com/getsops/sops
- `community.sops` collection (vars plugin, lookup plugin, `ansible.cfg` configuration): https://github.com/ansible-collections/community.sops

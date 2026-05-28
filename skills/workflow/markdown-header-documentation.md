---
name: Markdown Documentation Metadata
description: Require structured YAML metadata header for all markdown documentation files
---

# Markdown Documentation Metadata Skill

## Goal

When creating or updating any `*.md` documentation file, always add a compact YAML frontmatter block at the beginning of the file.

The metadata helps:

* AI agents understand document purpose
* improve semantic retrieval
* improve repository navigation
* improve context routing
* reduce hallucinations
* improve architectural understanding

---

# Required Rules

* Every markdown documentation file MUST start with YAML frontmatter.
* The metadata MUST be short and semantic.
* The metadata MUST describe intent and responsibility.
* The metadata MUST NOT duplicate implementation details.
* The metadata MUST be understandable without reading the entire document.

---

# Required Metadata Fields

Every markdown file should contain:

```yaml
---
name:
description:
type:
domain:
tags:
---
```

---

# Field Rules

## `name`

AI agent readable document name.

Example:

```yaml
name: docker-host-role
```

---

## `description`

Short semantic description.

Rules:

* 1-3 lines maximum
* describe responsibility and purpose
* avoid implementation details
* optimized for AI understanding

Good:

```yaml
description: >
  Manages Docker host infrastructure,
  reverse proxy configuration,
  and container publication rules.
```

Bad:

```yaml
description: >
  Contains tasks/main.yml,
  templates/nginx.conf.j2,
  and handlers/restart.yml.
```

---

## `type`

Document category.

Allowed values:

```yaml
type:
  - architecture
  - role
  - service
  - domain
  - api
  - deployment
  - decision
  - guide
  - workflow
  - component
  - infrastructure
```

---

## `domain`

Bounded context or ownership area.

Examples:

```yaml
domain: networking
domain: infrastructure
domain: vpn
domain: monitoring
domain: certificates
```

---

## `tags`

Short semantic keywords.

Rules:

* 3-10 tags
* lowercase
* kebab-case preferred

Example:

```yaml
tags:
  - docker
  - ansible
  - reverse-proxy
  - nginx
  - infrastructure
```

---

# Recommended Optional Fields

Use when useful.

```yaml
responsibility:
depends_on:
related:
constraints:
owned_by:
ai_hints:
```

---

# Example

```markdown
---
name: Docker Host Role

description: >
  Provides Docker host provisioning,
  reverse proxy integration,
  and centralized container exposure rules.

type: infrastructure

domain: containers

tags:
  - docker
  - ansible
  - nginx
  - reverse-proxy

constraints:
  - all ingress must go through reverse proxy
  - containers must not expose ports directly
---

# Docker Host Role
```

---

# AI Behavior Requirements

When generating markdown documentation:

* ALWAYS create metadata first
* infer semantic purpose from codebase context
* prefer concise summaries
* optimize metadata for retrieval and AI understanding
* avoid boilerplate descriptions
* avoid file-level implementation details
* avoid repeating headings inside metadata

---

# Anti-patterns

Do NOT generate:

```yaml
description: This document explains the document.
```

Do NOT generate:

```yaml
description: Contains configuration files.
```

Do NOT generate metadata that merely lists files or directories.

Do NOT generate metadata longer than the actual introduction.

---

# Priority

If existing metadata exists:

* preserve structure
* improve semantic quality
* avoid breaking existing keys

If no metadata exists:

* create it automatically.

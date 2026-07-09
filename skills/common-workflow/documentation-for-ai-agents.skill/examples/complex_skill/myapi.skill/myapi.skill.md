---
name: myapi
description: How to install and access myapi — root skill with links to auth and billing domain skills
whenToUse: when an agent needs to know how to install, authenticate against, or navigate myapi's capabilities
tags:
  - documentation
  - ai-agent
  - example
---

# Goal
Give an agent the installation, authentication, and navigation information for myapi, and point it to the correct domain skill for each task.

# Core Principle
- myapi is a REST API that requires a bearer token for all calls except token creation.
- All domain skills share the same base URL and authentication mechanism.

# Installation and access
See [installation.md](./installation.md) for base URL, credentials, and token setup.

# Domain skills
| Domain  | Skill                                              | Covers                                      |
| ------- | -------------------------------------------------- | ------------------------------------------- |
| Auth    | [myapi-auth.skill.md](../myapi-auth.skill.md)       | Login, token refresh, logout.               |
| Billing | [myapi-billing.skill.md](../myapi-billing.skill.md) | Charges, refunds, invoices.                 |

# Rule

## MUST
- Obtain a token through the auth domain before calling billing endpoints.
- Pass the token in the `Authorization: Bearer <token>` header.
- Refresh the token before it expires.

## MUST NOT
- Duplicate installation or authentication instructions in a domain skill.

# Check list
- [ ] myapi base URL and credentials are configured.
- [ ] The agent routes the task to the correct domain skill.
- [ ] Each domain skill links back to this root skill for installation/access.

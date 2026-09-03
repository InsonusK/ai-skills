---
name: use-concern-devops-tag
description: Decide which concern tag should represent deployment skills in the facet vocabulary for devops-service-deploy.
problem: Should the skill use concern/operations, concern/ci, concern/ci, or a free-form devops tag?
decision: Use concern/ci for the devops-service-deploy skill.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The devops-service-deploy skill needs a `concern/*` tag from the controlled facet vocabulary. The skill is about deployment, but deployment is closely related to CI/CD and the broader DevOps process. We needed to choose a tag that is precise enough to be useful but also discoverable for agents looking for DevOps-related skills.

# Selected variant
**Selected variant:** [[#concern/ci]]
- Aligns with the existing `skills/devops/` folder structure.
- Matches the mental model of the repository maintainers.

# Searched variants

## concern/ci (selected)

### Description
Register `concern/ci` as the controlled facet value for skills that address the DevOps process, including deployment, infrastructure, and CI/CD.

### Benefits
- Matches the top-level `skills/devops/` folder name, making tag-based discovery intuitive.
- Broad enough to cover deployment, CI, and operations without forcing a single narrow label.
- Easy for maintainers and agents to remember.

### Costs
- `devops` is an umbrella term; it is less precise than `concern/operations` for pure deployment skills.
- Existing CI-related skills use `concern/ci`; introducing `concern/ci` creates a sibling relationship that could confuse hierarchy purists.

## concern/operations

### Description
Register `concern/operations` as the controlled facet value for deployment and operations-related skills.

### Benefits
- More precise for deployment and run-time operations.
- Separates operations from CI and infrastructure.

### Costs
- Less discoverable for users who think in terms of "DevOps".
- Does not align with the `skills/devops/` folder structure.

## concern/ci

### Description
Tag the skill with `concern/ci`, treating deployment as part of the CI/CD pipeline.

### Benefits
- Fits naturally with existing CI-related skills.
- Recognizes that deployment is the CD part of CI/CD.

### Costs
- Deployment also happens outside of CI pipelines (manual rollouts, local compose, etc.).
- Too narrow for a skill that covers Docker Compose and Kubernetes operations, not just pipeline configuration.

## Free-form `devops` tag

### Description
Do not register a controlled facet value; instead add `devops` as a free-form keyword tag.

### Benefits
- No need to extend the controlled vocabulary.
- Flexible and quick to apply.

### Costs
- Free-form tags are not resolved by tag-expression queries, so agents searching with `concern/ci` would miss the skill.
- Inconsistent with the rule that every skill must carry at least one `concern/*` value from the controlled vocabulary.

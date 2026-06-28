---
name: plateau-update
description: Define how update plateau skills including there sub skills
whenToUse: when you update skills for building plateau
---

# Rules
MUST: 
- every time you update plateau or templates defined by plateau you must update header property `version` and setup current timestamp in format YYYYMMDDHHMMSS
  - if you update only template in plateu you also must change version of plateau
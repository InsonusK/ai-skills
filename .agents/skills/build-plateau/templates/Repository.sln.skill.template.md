---
uid:
name: plateau-sln-name
description: Describe which plateau repository does skill describe
domain: skill
type: template
version: 20260615
tags:
  - skill/template/sln
  #- tag for skill classification
triggers:
  #What kind of task should agent do to use this repository 
  #- create {Repository}
  #- implement {Repository}
created_by:
  #List of solution which must be implemented in this plateau
  #Example:
  #- "[[link]]"
---
# How Apply this template
- Find in all solutions from created_by files made by Repository.template.md
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.

# Structure

## Project Structure
```hint
Define how solution structure. Summarize all Project Structure from all finded Repository.template.md. 

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `[[solution skill link]] - [[Repository.template.md link]]` (see build-plateau SKILL.md "Applied solutions list format")
```
```example
/src
	/App
		/App.Host 

__Applied solutions:__
- [[Solution link]] - [[Repository.template.md link]]
```

## Directory and class skills
```hint
Define repository Directory and class. Summarize all Directory and class skills from all finded Repository.template.md

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `[[solution skill link]] - [[Repository.template.md link]]` (see build-plateau SKILL.md "Applied solutions list format")
```
```example
| Directory \| file | Description        |
| ----------------- | ------------------ |
| /src/App          | project desciption |
```

| Directory \| file | Description |
| ----------------- | ----------- |
|                   |             |

__Applied solutions:__
- [[Solution link]] - [[Repository.template.md link]]

# Rules
```hint
Define all repository RULES. Summarize all RULES from all finded Repository.template.md

At the end of block writes list to all used templates to build block.

MUST:
- If solution conflicted to each other as user to solve the problem
- Each bullet must be `[[solution skill link]] - [[Repository.template.md link]]` (see build-plateau SKILL.md "Applied solutions list format")
```
```example
MUST:
	- ...
SHOULD:
	- ...
SHOULD NOT:
	- ...
MUST NOT:
	- ...

__Applied solutions:__
- [[Solution link]] - [[Repository.template.md link]]
```


---
name: architect-validator
description: Validate that all architecture pattern was applied correctly
whenToUse: After make code changes
tags:
  - concern/testing
  - stack
  - concern/architecture

---
# Goal
- Check that plateau was applied correctly: (solutions, templates)

# Core Principle
- Validate all classes, check all solution and templates applied to class
- Don't skip any check list

# How to validate
1. get each file
2. define plateau applied to file
3. define solutions which have effect to plateau
4. define templates applied to file
5. get all check list from found plateau, solutions. templates
6. check all points in check list that they are checked
7. add comment `BUG: description` into the file with bug near the place where bug exist. Comment description must describe what was made wrong with link to file where define correct way

```example
public class Todo
{
    //BUG: Id must have internal setter by [class-entity](skill: class-entity)
	public int Id {get; set;}
}
```

# Rule
MUST:
- get ALL plateau, solutions. templates which have effect on file
- left comment with `BUG:`
- comment contain link to skill file

# Anti-patterns
- don't left comment with error
- skip plateau, solutions. templates which have effect on file

# Check list
- [ ] Every file has been checked
---
name: translate
description: Define how to translate skill to another language
whenToUse: when you need to translate skill to another language
---

ATTENSION: Skill could be applied only for skill which have type "Directory skill" (skill which store in sepparate directory and have file SKILL.md inside this directory). If you want to translate skill with another type, ask user what should you do.

# How to translate skill
1. Add sub directory `i18n` into skill directory. Example: `./translate/i18n`
2. Add target language directory into `i18n` directory. Example: `./translate/i18n/ru`
3. Create file `SKILL.md` in target language directory. Example: `./translate/i18n/ru/SKILL.md`
4. If skill contains any files with text content, you should translate them and add into target language directory. Example: `./translate/i18n/ru/Implementation/Repository.changes.md`

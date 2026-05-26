---
name: readme_write
version: 1.0.0
description: use this skill to create readme for unittest writing skill
---

# When to use this skill
Use this skill when you need to create readme file fol library or project which contains instruction how to use library or project. 

# Input data:
- Project or library path
- Readme file path (if exist)
- Define do you need to add comments in code for developers or you need to create readme only for users which will use library without access to code
  
# How to use it
## For readme for users
Use [readme template](./templates/readme_template.md) to create README.md file. you can made docs folder and split readme into several files if it is needed.
In readme describe only components and methods which is public and used by other projects. 

## Comments in code for developers
You need check does enough comments in code for developers. If not - add them. 
Comments should:
- explain bussines logic 
- how to use method
- input parameters
- output parameters
- posible exceptions, include inner exceptions which can be thrown by inner methods and is not catched in method
- but not what do inside the method, because it is for developers and they can see code.
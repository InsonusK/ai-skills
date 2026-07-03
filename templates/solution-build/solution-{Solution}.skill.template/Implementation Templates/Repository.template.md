---
description: Short description waht must be made while creation or change in repository
element_kind: #repository | project | class
change_kind: #create | extend
---
# How Apply this template
- Replace all ```hint```, ```example``` and ```code example``` blocks with real content. Do not keep them in the final skill file.
- header property `depends_on` couldn't have links to solution with order is greater or equal order in this solution. If it happend ask user to solve this problem.

# Structure

## Project Structure
```hint
Define how solution EXTENDS repository structure
```
```example
/src
	/App
		/App.Host 
```

## Directory and class skills
```hint
Define how solution EXTENDS repository directory and files
```
```example
| Directory \| file | Description        |
| ----------------- | ------------------ |
| /src/App          | project desciption |
```

| Directory \| file | Description |
| ----------------- | ----------- |
|                   |             |

# Rules
```hint
Define how solution EXTENDS repository MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```
## MUST
```example
- Command must realize ICommand<Result<DTO>>
```
## SHOULD
```example
- ...
```
## MAY
```example
- ...
```
## SHOULD NOT
```example
- ...
```

## MUST NOT
```example
- ...
```


# Unittest TestCases
```hint
Define how solution EXTENDS repository integrational unittests

RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call Post /record THEN
	- [ ] return expected result
	- [ ] new record exist in Database
- [ ] WHEN call Get /record/{id} and record exist THEN
    - [ ] Return expected result
- [ ] WHEN call Get /record/{id} and record NOT exist THEN
    - [ ] Return 404 error 
	- [ ] Respones has expected body
```

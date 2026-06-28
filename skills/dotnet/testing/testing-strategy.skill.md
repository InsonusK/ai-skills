---
name: testing-strategy
description: Defining rules of testing strategies
whenToUse: change code or write unit tests
tags:
  - unit-testing
  - dotnet
---
# Goal
- define scope of unittesting in project

# Core Principle
- Validators, ValueObjects, Entites has own tests which validate there validation of invalid state
- All scenarios must have tests which cover full service work and assert that all components called in right orders, work as expected
- Other components and classes must covers by test and cover main cases and edge cases

# Rule
MUST:
- Has separate for each validator, ValueObject, Entity. 
	- Testcases must cover:
		- Valid case
		- Corner case
		- Wrong case
	- Assert that test cover all possible values or combinations of values
- Has separate complex test of usecases (inbound sync call, inboud async message, cron jobs)
	- Cover:
		- main success case
		- most important invalid cases
	- Assert:
		- response must be equal expected response
		- module call order must be equal as expected and in the right order
- Other components must be covered as usual: main cases, edge cases etc

COULD:
- Mock sub validators, valueObjects, Entities in validator, ValueObject, Entity unittests
# Anti-patterns
- Test Validator, ValueObject, Entity in test o another component
- Test of Validator, ValueObject, Entity does not cover Corner cases or wrong cases

# Check list
- [ ] All Validators, ValueObjects, Entites has own unit test
- [ ] All possible values as own test which cover it
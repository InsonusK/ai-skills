---
description: The domain service decides which follow-up tasks a change needs, passes them with the write, and exposes a method per task type for the handler
project_name: internal/domain/services
name: "{service}"
element_kind: struct
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/internal-domain-services-service-go
---

# Goals
- Keep the business decision "this change needs follow-up work" in the domain, testable with a stub port.
- Give each task type a domain method its handler calls.

# Core Principles
- The task type name and its payload struct are declared in the domain package — the domain owns the task type (solution-taskbox: payload shape owned by the type's owner).
- Write methods append to their existing body: compute as before, build the follow-up `Task` values, pass them to the port's write — never wrap or reorder the existing logic.

# Implementation changes

### AS IS
```go
func (s *{Service}) {Method}(ctx context.Context, input string) (Result, error) {
	// ... validate, compute ...
	if err := s.{storePort}.{Write}(ctx, entry); err != nil {
		return Result{}, err
	}
	return result, nil
}
```

### TO BE
```go
// Task{FollowUp} is the task type {what it does}; its payload is {FollowUp}.
const Task{FollowUp} = "{follow-up}"

// {FollowUp} is the payload of Task{FollowUp}. Add fields only as optional:
// stored tasks keep the shape they were enqueued with.
type {FollowUp} struct {
	{Key} string `json:"{key}"`
}

func (s *{Service}) {Method}(ctx context.Context, input string) (Result, error) {
	// ... validate, compute (unchanged) ...
	var followUps []interfaces.Task
	if {needsFollowUp} {
		followUps = append(followUps, interfaces.Task{
			Type:    Task{FollowUp},
			Payload: {FollowUp}{{Key}: key},
			Group:   key,                          // tasks about one entity run in order
			RunAt:   time.Now().Add(s.{delay}),    // zero = as soon as possible
		})
	}
	if err := s.{storePort}.{Write}(ctx, entry, followUps...); err != nil {
		return Result{}, err
	}
	return result, nil
}

// {FollowUpMethod} does the follow-up work; its handler calls it. It returns
// the domain's own errors (validation, unavailable dependency) unchanged.
func (s *{Service}) {FollowUpMethod}(ctx context.Context, key string) (Result, error)
```

# Rule changes

## MUST

### Decide follow-ups in the domain
Build the follow-up `Task` values in the domain service, from business state — never in the store adapter.
- Risk: a business rule hidden in infrastructure is invisible to the domain's scenarios.
- Fix: add a scenario asserting which tasks the stub port received with the write.

# Check list
- [ ] Each task type's name and payload struct are declared in the domain package.
- [ ] A scenario asserts the tasks passed with the write, and one asserts that a change needing no follow-up passes none.

# Unittest TestCases
- [ ] WHEN a change needs follow-up work THEN the port's write receives exactly the expected tasks (type, group, payload, delay).
- [ ] WHEN the follow-up method's dependency is unavailable THEN it returns the dependency's error and records nothing.

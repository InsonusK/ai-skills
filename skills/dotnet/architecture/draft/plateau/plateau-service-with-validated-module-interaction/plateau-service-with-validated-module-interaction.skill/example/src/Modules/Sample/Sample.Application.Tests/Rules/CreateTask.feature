Feature: CreateTask

Scenario: Create a task with valid input
	Given a task title "Review example"
	And an assignee id 1
	And an assignee email "reviewer@example.com"
	When the CreateTask command is handled
	Then the task is created successfully

Scenario: Create a task with invalid email is rejected by the validator
	Given a task title "Review example"
	And an assignee id 1
	And an assignee email "not-an-email"
	When the CreateTask command is validated
	Then validation fails with "Email is not valid."

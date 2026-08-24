Feature: CreateTask

Scenario: Create a task with invalid email is rejected by the validator
	Given a task title "Review example"
	And an assignee id 1
	And an assignee email "not-an-email"
	And an action timestamp "2026-08-24T10:00:00+00:00"
	When the CreateTask command is validated
	Then validation fails with "Email is not valid."

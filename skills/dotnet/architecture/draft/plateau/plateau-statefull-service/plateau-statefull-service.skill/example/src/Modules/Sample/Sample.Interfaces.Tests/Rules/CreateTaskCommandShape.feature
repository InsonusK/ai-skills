Feature: CreateTaskCommand shape

Scenario: CreateTaskCommand implements ICommand of CreateTaskResult
	Given a task title "Title"
	And an assignee id 1
	And an assignee email "user@example.com"
	And an action timestamp "2026-08-24T10:00:00+00:00"
	When the CreateTaskCommand is created
	Then it implements ICommand of CreateTaskResult

Feature: TaskItem entity behavior

Scenario: Update title with a valid value
	Given a task with email "user@example.com"
	When the title is updated to "New title"
	Then the title is "New title"

Scenario: Update title with empty value throws
	Given a task with email "user@example.com"
	When the title is updated to ""
	Then a DomainException is thrown

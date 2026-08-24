Feature: ValidationBehavior

Scenario: ValidationBehavior returns invalid when a validator fails
	Given a MediatR pipeline with ValidationBehavior
	When a registered validator fails for the request
	Then the result status is Invalid

Scenario: ValidationBehavior passes through when no validators are registered
	Given a MediatR pipeline with ValidationBehavior
	When no validators are registered for the request
	Then the pipeline reaches the next behavior

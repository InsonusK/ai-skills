Feature: Greet command shape

  Scenario: Greet command implements the shared command contract
    Given a greet command with name "World"
    Then it implements ICommand of string
